import { isPlatformBrowser } from '@angular/common';
import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, OnDestroy, PLATFORM_ID, ViewChild, ViewEncapsulation, computed, inject, input, signal } from '@angular/core';
import * as L from 'leaflet';

type ReverseGeocodeAddress = {
  road?: string;
  postcode?: string;
  suburb?: string;
  city_district?: string;
  neighbourhood?: string;
  city?: string;
  town?: string;
  village?: string;
  state_district?: string;
  state?: string;
  country?: string;
};

type ReverseGeocodeResponse = {
  address?: ReverseGeocodeAddress;
};

type OverpassElement = {
  id: number;
  lat?: number;
  lon?: number;
  center?: {
    lat: number;
    lon: number;
  };
  tags?: {
    name?: string;
  };
};

type OverpassResponse = {
  elements?: OverpassElement[];
};

type NearbyPlace = {
  id: string;
  name: string;
  lat: number;
  lng: number;
};

@Component({
  selector: 'app-project-map',
  standalone: true,
  templateUrl: './project-map.html',
  styleUrl: './project-map.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
})
export class ProjectMap implements AfterViewInit, OnDestroy {
  private static readonly DEFAULT_ADDRESS_LINE1 = 'Makkah St.,12713 المعذر الشمالي ، الرياض';
  private static readonly DEFAULT_ADDRESS_LINE2 = 'السعودية';
  private static readonly NEARBY_PLACES_LIMIT = 4;
  private static readonly OVERPASS_ENDPOINTS = [
    'https://overpass-api.de/api/interpreter',
    'https://overpass.kumi.systems/api/interpreter',
    'https://overpass.openstreetmap.fr/api/interpreter',
  ];

  private static readonly PRIMARY_MARKER_ICON = L.divIcon({
    className: 'map-main-marker-wrapper',
    html: [
      '<svg class="map-main-pin" viewBox="0 0 28 36" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">',
      '<path d="M14 1.5C8.2 1.5 3.5 6.2 3.5 12c0 7.9 8.5 20.7 10 23 .2.3.5.5.9.5s.7-.2.9-.5c1.5-2.3 10-15.1 10-23C24.5 6.2 19.8 1.5 14 1.5z" fill="#5865ad" stroke="#ffffff" stroke-width="2"/>',
      '<circle cx="14" cy="12" r="4" fill="#ffffff"/>',
      '</svg>',
    ].join(''),
    iconSize: [28, 36],
    iconAnchor: [14, 36],
  });

  private static readonly NEARBY_MARKER_ICON = L.divIcon({
    className: 'map-nearby-pin-wrapper',
    html: '<span class="map-nearby-pin"></span>',
    iconSize: [16, 16],
    iconAnchor: [8, 8],
  });

  private readonly platformId = inject(PLATFORM_ID);
  private readonly isBrowser = isPlatformBrowser(this.platformId);

  lat = input.required<number>();
  lng = input.required<number>();

  private readonly derivedAddressLine1 = signal(ProjectMap.DEFAULT_ADDRESS_LINE1);
  private readonly derivedAddressLine2 = signal(ProjectMap.DEFAULT_ADDRESS_LINE2);

  readonly infoAddressLine1 = this.derivedAddressLine1.asReadonly();
  readonly infoAddressLine2 = this.derivedAddressLine2.asReadonly();

  @ViewChild('mapRoot') mapRoot!: ElementRef<HTMLDivElement>;
  private map?: L.Map;
  private nearbyMarkers: L.Marker[] = [];

  readonly coordinateLabel = computed(() => `${this.formatCoordinate(this.lat(), 'lat')} ${this.formatCoordinate(this.lng(), 'lng')}`);
  readonly largerMapUrl = computed(() => `https://www.google.com/maps/search/?api=1&query=${this.lat()},${this.lng()}`);

  ngAfterViewInit(): void {
    if (!this.isBrowser) {
      return;
    }

    const center = L.latLng(this.lat(), this.lng());

    this.map = L.map(this.mapRoot.nativeElement, {
      zoomControl: false,
      attributionControl: false,
    }).setView(center, 16.8);

    L.control.zoom({
      position: 'bottomleft',
      zoomInTitle: 'تكبير',
      zoomOutTitle: 'تصغير',
    }).addTo(this.map);

    L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
      maxZoom: 19,
      minZoom: 3,
    }).addTo(this.map);

    L.marker(center, {
      icon: ProjectMap.PRIMARY_MARKER_ICON,
      interactive: false,
      keyboard: false,
    }).addTo(this.map);

    void this.loadAddressFromCoordinates(center);
    void this.loadNearbyPlaces(center);
  }

  ngOnDestroy(): void {
    for (const marker of this.nearbyMarkers) {
      marker.remove();
    }
    this.nearbyMarkers = [];
    this.map?.remove();
  }

  private async loadAddressFromCoordinates(center: L.LatLng): Promise<void> {
    try {
      const address = await this.fetchReverseAddress(center.lat, center.lng);
      if (!address) {
        this.useFallbackAddress();
        return;
      }

      this.derivedAddressLine1.set(this.buildAddressLine1(address));
      this.derivedAddressLine2.set(this.firstValue(address.country) ?? ProjectMap.DEFAULT_ADDRESS_LINE2);
    } catch {
      this.useFallbackAddress();
    }
  }

  private async loadNearbyPlaces(center: L.LatLng): Promise<void> {
    const radiusInMeters = 2200;
    const overpassQuery = `
[out:json][timeout:20];
(
  node["amenity"~"hospital|school|restaurant|cafe"](around:${radiusInMeters},${center.lat},${center.lng});
  node["shop"="mall"](around:${radiusInMeters},${center.lat},${center.lng});
  node["tourism"="attraction"](around:${radiusInMeters},${center.lat},${center.lng});
);
out body 50;
`.trim();

    try {
      const data = await this.fetchNearbyPlacesData(overpassQuery);
      if (!data) {
        return;
      }

      const places = this.parseNearbyPlaces(data);
      this.renderNearbyPlaces(places);
    } catch {
      // Keep map usable if nearby places API is unavailable.
    }
  }

  private async fetchNearbyPlacesData(query: string): Promise<OverpassResponse | undefined> {
    for (const endpoint of ProjectMap.OVERPASS_ENDPOINTS) {
      try {
        // const url = `${endpoint}?data=${encodeURIComponent(query)}`;
        // const response = await fetch(url);
        // if (!response.ok) {
        //   continue;
        // }

        // const bodyText = await response.text();
        // const parsed = JSON.parse(bodyText) as OverpassResponse;
        // if (!Array.isArray(parsed.elements)) {
        //   continue;
        // }

        // return parsed;
      } catch {
        // Try the next endpoint.
      }
    }

    return undefined;
  }

  private parseNearbyPlaces(data: OverpassResponse): NearbyPlace[] {
    const seen = new Set<string>();
    const places: NearbyPlace[] = [];

    for (const element of data.elements ?? []) {
      const name = this.firstValue(element.tags?.name);
      const lat = element.lat ?? element.center?.lat;
      const lng = element.lon ?? element.center?.lon;

      if (!name || lat == null || lng == null) {
        continue;
      }

      const key = `${name}-${lat.toFixed(5)}-${lng.toFixed(5)}`;
      if (seen.has(key)) {
        continue;
      }

      seen.add(key);
      places.push({
        id: String(element.id),
        name,
        lat,
        lng,
      });

      if (places.length >= ProjectMap.NEARBY_PLACES_LIMIT) {
        break;
      }
    }

    return places;
  }

  private renderNearbyPlaces(places: NearbyPlace[]): void {
    if (!this.map) {
      return;
    }

    for (const marker of this.nearbyMarkers) {
      marker.remove();
    }
    this.nearbyMarkers = [];

    for (const place of places) {
      const marker = L.marker({ lat: place.lat, lng: place.lng }, {
        icon: ProjectMap.NEARBY_MARKER_ICON,
        keyboard: false,
      });

      marker.bindTooltip(place.name, {
        direction: 'top',
        offset: [0, -12],
        className: 'map-poi-tooltip',
      });

      marker.addTo(this.map);
      this.nearbyMarkers.push(marker);
    }
  }

  private async fetchReverseAddress(lat: number, lng: number): Promise<ReverseGeocodeAddress | undefined> {
    const query = new URLSearchParams({
      format: 'jsonv2',
      lat: String(lat),
      lon: String(lng),
      addressdetails: '1',
      'accept-language': 'ar',
    });

    const response = await fetch(`https://nominatim.openstreetmap.org/reverse?${query.toString()}`, {
      headers: {
        Accept: 'application/json',
      },
    });

    if (!response.ok) {
      return undefined;
    }

    const data = (await response.json()) as ReverseGeocodeResponse;
    return data.address;
  }

  private buildAddressLine1(address: ReverseGeocodeAddress): string {
    const road = this.firstValue(address.road) ?? 'Makkah St.';
    const postcode = this.firstValue(address.postcode);

    const district = this.firstValue(
      address.suburb,
      address.city_district,
      address.neighbourhood,
    );

    const city = this.firstValue(
      address.city,
      address.town,
      address.village,
      address.state_district,
      address.state,
    );

    const districtWithCity = district && city
      ? `${district} ، ${city}`
      : district ?? city ?? '';

    if (!districtWithCity) {
      return ProjectMap.DEFAULT_ADDRESS_LINE1;
    }

    return `${road}${postcode ? `,${postcode}` : ''} ${districtWithCity}`.trim();
  }

  private normalizeAddressPart(value?: string): string | undefined {
    const term = value?.trim();
    if (!term) {
      return undefined;
    }

    return term
      .replace(/^حي\s+/, '')
      .replace(/^منطقة\s+/, '')
      .trim();
  }

  private firstValue(...values: Array<string | undefined>): string | undefined {
    for (const value of values) {
      const normalized = this.normalizeAddressPart(value);
      if (normalized) {
        return normalized;
      }
    }

    return undefined;
  }

  private useFallbackAddress(): void {
    this.derivedAddressLine1.set(ProjectMap.DEFAULT_ADDRESS_LINE1);
    this.derivedAddressLine2.set(ProjectMap.DEFAULT_ADDRESS_LINE2);
  }

  private formatCoordinate(value: number, type: 'lat' | 'lng'): string {
    const absolute = Math.abs(value);
    const degrees = Math.floor(absolute);
    const minuteBase = (absolute - degrees) * 60;
    const minutes = Math.floor(minuteBase);
    const seconds = (minuteBase - minutes) * 60;
    const direction = type === 'lat'
      ? (value >= 0 ? 'N' : 'S')
      : (value >= 0 ? 'E' : 'W');

    return `${degrees}\u00B0 ${String(minutes).padStart(2, '0')}'${seconds.toFixed(1)}"${direction}`;
  }
}
