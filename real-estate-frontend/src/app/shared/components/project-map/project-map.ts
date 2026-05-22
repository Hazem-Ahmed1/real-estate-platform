import { isPlatformBrowser } from '@angular/common';
import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, EventEmitter, Input, OnDestroy, Output, PLATFORM_ID, ViewChild, ViewEncapsulation, computed, inject, input, signal } from '@angular/core';
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
  tags?: Record<string, string>;
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
  private static readonly DEFAULT_ADDRESS_LINE1 = 'العنوان غير متوفر';
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

  private static readonly UNIT_MARKER_ICON = L.divIcon({
    className: 'map-main-marker-wrapper',
    html: [
      '<svg class="map-main-pin map-unit-circle" viewBox="0 0 28 28" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">',
      '<defs>',
      '<filter id="unitShadow">',
      '<feDropShadow dx="0" dy="1" stdDeviation="3" flood-color="#ffffff" flood-opacity="0.95"/>',
      '</filter>',
      '</defs>',
      // single colored circle without outer white halo
      '<circle cx="14" cy="14" r="10" fill="#5865ad" stroke="#ffffff" stroke-width="2"/>',
      '</svg>',
    ].join(''),
    iconSize: [28, 28],
    iconAnchor: [14, 14],
  });

  private static readonly PROJECT_ANCHOR_ICON = L.divIcon({
    className: 'map-main-marker-wrapper',
    html: [
      '<svg class="map-main-pin" viewBox="0 0 28 36" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">',
      '<path d="M14 1.5C8.2 1.5 3.5 6.2 3.5 12c0 7.9 8.5 20.7 10 23 .2.3.5.9.5s.7-.2.9-.5c1.5-2.3 10-15.1 10-23C24.5 6.2 19.8 1.5 14 1.5z" fill="#5865ad" stroke="#ffffff" stroke-width="2"/>',
      '<circle cx="14" cy="12" r="4" fill="#ffffff"/>',
      '</svg>',
    ].join(''),
    iconSize: [28, 36],
    iconAnchor: [14, 36],
  });

  private readonly platformId = inject(PLATFORM_ID);
  private readonly isBrowser = isPlatformBrowser(this.platformId);

  lat = input.required<number>();
  lng = input.required<number>();
  nearbyFacilities = input<any[]>([]);
  query = input('');
  circleRadiusMeters = input<number | null>(null);
  /** projectPicker: move pin, no nearby. unitPicker: fixed project pin + circle, movable unit pin. */
  mapMode = input<'default' | 'projectPicker' | 'unitPicker'>('default');
  anchorLat = input<number | null>(null);
  anchorLng = input<number | null>(null);
  /** When true, do not add the initial main marker if it would be located at the project's anchor. Useful for unit picker so project anchor isn't shown as a marker. */
  suppressInitialMarker = input(false);
  showNearbyPlaces = input(true);
  @Input() allowSelect = false;
  @Output() outOfBounds = new EventEmitter<void>();
  @Output() selection = new EventEmitter<{ lat: number; lng: number; addressLine1: string; addressLine2: string; places: NearbyPlace[] }>();

  private readonly derivedAddressLine1 = signal(ProjectMap.DEFAULT_ADDRESS_LINE1);
  private readonly derivedAddressLine2 = signal(ProjectMap.DEFAULT_ADDRESS_LINE2);

  readonly infoAddressLine1 = this.derivedAddressLine1.asReadonly();
  readonly infoAddressLine2 = this.derivedAddressLine2.asReadonly();

  @ViewChild('mapRoot') mapRoot!: ElementRef<HTMLDivElement>;
  private map?: L.Map;
  private nearbyMarkers: L.Marker[] = [];
  private mainMarker?: L.Marker;
  private anchorMarker?: L.Marker;
  private selectionCircle?: L.Circle;

  readonly coordinateLabel = computed(() => `${this.formatCoordinate(this.lat(), 'lat')} ${this.formatCoordinate(this.lng(), 'lng')}`);
  readonly largerMapUrl = computed(() => `https://www.google.com/maps/search/?api=1&query=${this.lat()},${this.lng()}`);

  ngAfterViewInit(): void {
    if (!this.isBrowser) {
      return;
    }

    const selectionCenter = L.latLng(this.lat(), this.lng());
    const anchorCenter = this.getAnchorCenter(selectionCenter);
    const mapCenter = this.mapMode() === 'unitPicker' ? anchorCenter : selectionCenter;

    this.map = L.map(this.mapRoot.nativeElement, {
      zoomControl: false,
      attributionControl: false,
    }).setView(mapCenter, 16.8);

    L.control.zoom({
      position: 'bottomleft',
      zoomInTitle: 'تكبير',
      zoomOutTitle: 'تصغير',
    }).addTo(this.map!);

    L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
      maxZoom: 19,
      minZoom: 3,
    }).addTo(this.map!);

    if (this.mapMode() === 'unitPicker') {
      const isSameAsAnchor = selectionCenter.lat === anchorCenter.lat && selectionCenter.lng === anchorCenter.lng;
      if (!this.suppressInitialMarker() || !isSameAsAnchor) {
        this.mainMarker = L.marker(selectionCenter, {
          icon: ProjectMap.UNIT_MARKER_ICON,
          interactive: false,
          keyboard: false,
        }).addTo(this.map!);
      }
      this.updateSelectionCircle(anchorCenter);
    } else {
      const isUnitDisplay = (this.nearbyFacilities()?.length ?? 0) > 0;
      const mainIcon = isUnitDisplay ? ProjectMap.UNIT_MARKER_ICON : ProjectMap.PRIMARY_MARKER_ICON;
      this.mainMarker = L.marker(selectionCenter, { icon: mainIcon, interactive: false, keyboard: false }).addTo(this.map!);
      this.updateSelectionCircle(selectionCenter);
    }

    void this.loadAddressFromCoordinates(selectionCenter);
    if (this.shouldShowNearby()) {
      void this.loadNearbyPlaces(selectionCenter);
    }
    void this.applyQueryFocus(selectionCenter);

    if (this.allowSelect) {
      this.map.on('click', async (e: L.LeafletMouseEvent) => {
        const clicked = e.latlng;
        const target = this.resolveSelectionPoint(clicked);
        if (!target) {
          // inform parent that user clicked outside the allowed circle
          this.outOfBounds.emit();
          return;
        }

        if (this.mapMode() !== 'unitPicker') {
          this.updateSelectionCircle(target);
        }

        if (this.mainMarker) {
          this.mainMarker.setLatLng(target);
        } else {
          this.mainMarker = L.marker(target, {
            icon: ProjectMap.UNIT_MARKER_ICON,
            interactive: false,
            keyboard: false,
          }).addTo(this.map!);
        }

        await this.loadAddressFromCoordinates(target);
        // If reverse-geocoding falls back, keep the clicked point rather than a hardcoded neighborhood.
        if (this.infoAddressLine1() === ProjectMap.DEFAULT_ADDRESS_LINE1) {
          this.derivedAddressLine1.set('العنوان المحدد');
        }
        const places = this.shouldShowNearby() ? await this.getNearbyPlacesFor(target) : [];
        if (this.shouldShowNearby()) {
          this.renderNearbyPlaces(places, target);
        } else {
          this.clearNearbyMarkers();
        }

        this.selection.emit({
          lat: target.lat,
          lng: target.lng,
          addressLine1: this.infoAddressLine1(),
          addressLine2: this.infoAddressLine2(),
          places,
        });
      });
    }
  }

  private getAnchorCenter(fallback: L.LatLng): L.LatLng {
    const lat = this.anchorLat();
    const lng = this.anchorLng();
    if (lat != null && lng != null && Number.isFinite(lat) && Number.isFinite(lng)) {
      return L.latLng(lat, lng);
    }
    return fallback;
  }

  private shouldShowNearby(): boolean {
    if (this.mapMode() === 'projectPicker') {
      return false;
    }
    return this.showNearbyPlaces();
  }

  private resolveSelectionPoint(clicked: L.LatLng): L.LatLng | null {
    if (this.mapMode() !== 'unitPicker') {
      return clicked;
    }

    const anchor = this.getAnchorCenter(clicked);
    const radius = this.circleRadiusMeters();
    if (radius == null || radius <= 0) {
      return clicked;
    }
    const distance = this.distanceMeters(anchor, clicked);
    if (distance <= radius) {
      return clicked;
    }

    // Outside the allowed circle: return null to indicate out-of-bounds selection
    return null;
  }

  private distanceMeters(a: L.LatLng, b: L.LatLng): number {
    const R = 6371000;
    const toRad = (v: number) => (v * Math.PI) / 180;
    const dLat = toRad(b.lat - a.lat);
    const dLon = toRad(b.lng - a.lng);
    const x = Math.sin(dLat / 2) ** 2
      + Math.cos(toRad(a.lat)) * Math.cos(toRad(b.lat)) * Math.sin(dLon / 2) ** 2;
    return R * 2 * Math.atan2(Math.sqrt(x), Math.sqrt(1 - x));
  }

  private clearNearbyMarkers(): void {
    for (const marker of this.nearbyMarkers) {
      marker.remove();
    }
    this.nearbyMarkers = [];
  }

  private dedupeNearbyFacilities<T extends { name?: string; type?: string; latitude?: number | null; longitude?: number | null; distance?: string }>(items: T[]): T[] {
    const seen = new Set<string>();
    const result: T[] = [];

    for (const item of items ?? []) {
      if (!item) {
        continue;
      }

      const key = [
        String(item.name ?? '').trim().toLowerCase(),
        String(item.type ?? '').trim().toLowerCase(),
        item.latitude != null ? Number(item.latitude).toFixed(6) : '',
        item.longitude != null ? Number(item.longitude).toFixed(6) : '',
        String(item.distance ?? '').trim().toLowerCase(),
      ].join('|');

      if (seen.has(key)) {
        continue;
      }

      seen.add(key);
      result.push(item);
    }

    return result;
  }

  private async applyQueryFocus(fallbackCenter: L.LatLng): Promise<void> {
    const query = this.query().trim();
    const circleCenter = this.mapMode() === 'unitPicker'
      ? this.getAnchorCenter(fallbackCenter)
      : fallbackCenter;

    if (!query || !this.map) {
      this.updateSelectionCircle(circleCenter);
      return;
    }

    try {
      const location = await this.fetchCityLocation(query);
      if (!location) {
        this.updateSelectionCircle(circleCenter);
        return;
      }

      const focusCenter = L.latLng(location.lat, location.lng);
      this.map.setView(focusCenter, 15.2);

      if (this.mapMode() === 'unitPicker') {
        this.updateSelectionCircle(circleCenter);
        return;
      }

      if (this.mainMarker && this.mapMode() !== 'unitPicker') {
        this.mainMarker.setLatLng(focusCenter);
      }

      this.updateSelectionCircle(focusCenter);
      void this.loadAddressFromCoordinates(focusCenter);
    } catch {
      this.updateSelectionCircle(circleCenter);
    }
  }

  private updateSelectionCircle(center: L.LatLng): void {
    if (!this.map) {
      return;
    }

    const radiusMeters = this.circleRadiusMeters();
    if (radiusMeters == null || radiusMeters <= 0) {
      this.selectionCircle?.remove();
      this.selectionCircle = undefined;
      return;
    }

    if (!this.selectionCircle) {
      this.selectionCircle = L.circle(center, {
        radius: radiusMeters,
        color: '#5865ad',
        weight: 2,
        fillColor: '#5865ad',
        fillOpacity: 0.12,
      }).addTo(this.map!);
      return;
    }

    this.selectionCircle.setLatLng(center);
    this.selectionCircle.setRadius(radiusMeters);
  }

  private async fetchCityLocation(query: string): Promise<{ lat: number; lng: number } | undefined> {
    const params = new URLSearchParams({
      format: 'jsonv2',
      q: query,
      limit: '1',
      'accept-language': 'ar',
    });

    const response = await fetch(`https://nominatim.openstreetmap.org/search?${params.toString()}`, {
      headers: {
        Accept: 'application/json',
      },
    });

    if (!response.ok) {
      return undefined;
    }

    const results = (await response.json()) as Array<{ lat: string; lon: string }>;
    const first = results[0];
    if (!first) {
      return undefined;
    }

    return {
      lat: Number(first.lat),
      lng: Number(first.lon),
    };
  }

  ngOnDestroy(): void {
    this.clearNearbyMarkers();
    this.anchorMarker?.remove();
    this.selectionCircle?.remove();
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

  private getFacilityIcon(type: string): string {
    const t = String(type).toLowerCase();
    if (t.includes('mosque') || t.includes('مسجد')) return 'fa-mosque';
    if (t.includes('school') || t.includes('مدرسة') || t.includes('جامعة')) return 'fa-graduation-cap';
    if (t.includes('hospital') || t.includes('مستشفى') || t.includes('عيادة')) return 'fa-hospital';
    if (t.includes('restaurant') || t.includes('مطعم') || t.includes('اكل')) return 'fa-utensils';
    if (t.includes('park') || t.includes('حديقة') || t.includes('منتزه')) return 'fa-tree';
    if (t.includes('bank') || t.includes('بنك') || t.includes('صراف')) return 'fa-building-columns';
    if (t.includes('pharmacy') || t.includes('صيدلية')) return 'fa-prescription-bottle-medical';
    if (t.includes('supermarket') || t.includes('سوبر ماركت') || t.includes('بقال')) return 'fa-cart-shopping';
    if (t.includes('club') || t.includes('نادي') || t.includes('رياضة')) return 'fa-dumbbell';
    return 'fa-circle-dot';
  }

  private renderUnitFacilities(places: any[]): void {
    if (!this.map) return;

    for (const marker of this.nearbyMarkers) {
      marker.remove();
    }
    this.nearbyMarkers = [];

    for (const place of places) {
      const iconClass = this.getFacilityIcon(place.type);

      const customIcon = L.divIcon({
        className: 'map-facility-marker-wrapper',
        html: `<div class="map-facility-circle"><i class="fa-solid ${iconClass}"></i></div>`,
        iconSize: [32, 32],
        iconAnchor: [16, 16],
      });

      const marker = L.marker({ lat: place.lat, lng: place.lng }, {
        icon: customIcon,
        keyboard: false,
      });

      marker.bindTooltip(`
        <div style="text-align: right; direction: rtl; font-family: 'Cairo', sans-serif; margin: 0; padding: 0;">
          <strong style="color: #212529; font-size: 0.85rem; display: block; line-height: 1.2;">${place.name}</strong>
          <span style="color: #15A94B; font-size: 0.75rem; font-weight: 700; display: inline-flex; align-items: center; margin-top: 2px; gap: 3px; line-height: 1.2;">
            <i class="fa-solid fa-route"></i> المسافة: ${place.distance}
          </span>
        </div>
      `, {
        direction: 'top',
        offset: [0, -2],
        className: 'map-facility-tooltip',
      });

      try {
        // ensure map pane exists before appending DOM elements
        const hasPane = !!(this.map && (this.map as any).getPane && (this.map as any).getPane('markerPane'));
        if (hasPane) {
          marker.addTo(this.map!);
        }
        this.nearbyMarkers.push(marker);
      } catch (e) {
        // Prevent map marker DOM errors from crashing the map UI.
        // Keep marker in array for potential cleanup, but skip adding to DOM.
        this.nearbyMarkers.push(marker);
      }
    }
  }

  private async loadNearbyPlaces(center: L.LatLng): Promise<void> {
    const facilities = this.dedupeNearbyFacilities(this.nearbyFacilities() ?? []);
    if (facilities.length > 0) {
      const places: any[] = [];
      const angleStep = (2 * Math.PI) / facilities.length;

      facilities.forEach((facility, index) => {
        let distanceMeters = 300;
        const distStr = facility.distance ? String(facility.distance).trim() : '';
        const matchNum = distStr.match(/[\d.]+/);
        if (matchNum) {
          const num = parseFloat(matchNum[0]);
          if (distStr.includes('كم') || distStr.includes('km')) {
            distanceMeters = num * 1000;
          } else {
            distanceMeters = num;
          }
        }

        const meters = Math.max(150, Math.min(800, distanceMeters));
        const latDelta = (meters * Math.sin(index * angleStep)) / 111000;
        const lngDelta = (meters * Math.cos(index * angleStep)) / (111000 * Math.cos((center.lat * Math.PI) / 180));

        places.push({
          id: `facility-${index}`,
          name: facility.name,
          lat: center.lat + latDelta,
          lng: center.lng + lngDelta,
          type: facility.type,
          distance: facility.distance,
        });
      });

      this.renderUnitFacilities(places);
      return;
    }

    try {
      const places = await this.fetchOverpassPlaces(center);
      this.renderNearbyPlaces(places, center);
    } catch {
      // Keep map usable if nearby places API is unavailable.
    }
  }

  async getNearbyPlacesFor(center: L.LatLng): Promise<NearbyPlace[]> {
    const facilities = this.dedupeNearbyFacilities(this.nearbyFacilities() ?? []);
    if (facilities.length > 0) {
      const places: NearbyPlace[] = [];
      const angleStep = (2 * Math.PI) / facilities.length;

      facilities.forEach((facility, index) => {
        let distanceMeters = 300;
        const distStr = facility.distance ? String(facility.distance).trim() : '';
        const matchNum = distStr.match(/[\d.]+/);
        if (matchNum) {
          const num = parseFloat(matchNum[0]);
          if (distStr.includes('كم') || distStr.includes('km')) {
            distanceMeters = num * 1000;
          } else {
            distanceMeters = num;
          }
        }

        const meters = Math.max(150, Math.min(800, distanceMeters));
        const latDelta = (meters * Math.sin(index * angleStep)) / 111000;
        const lngDelta = (meters * Math.cos(index * angleStep)) / (111000 * Math.cos((center.lat * Math.PI) / 180));

        places.push({
          id: `facility-${index}`,
          name: facility.name,
          lat: center.lat + latDelta,
          lng: center.lng + lngDelta,
        });
      });

      return places;
    }

    try {
      return await this.fetchOverpassPlaces(center);
    } catch {
      return [];
    }
  }

  private async fetchOverpassPlaces(center: L.LatLng): Promise<NearbyPlace[]> {
    const radiusInMeters = 2200;
    const overpassQuery = `
[out:json][timeout:25];
(
  node["amenity"~"hospital|clinic|school|university|restaurant|cafe|fast_food|bank|pharmacy|place_of_worship|mosque"](around:${radiusInMeters},${center.lat},${center.lng});
  node["shop"~"supermarket|mall|convenience"](around:${radiusInMeters},${center.lat},${center.lng});
  node["leisure"~"park|garden|sports_centre|fitness_centre|stadium"](around:${radiusInMeters},${center.lat},${center.lng});
  node["tourism"="attraction"](around:${radiusInMeters},${center.lat},${center.lng});
);
out body 80;
`.trim();

    const data = await this.fetchNearbyPlacesData(overpassQuery);
    if (!data) {
      return [];
    }

    return this.parseNearbyPlaces(data, center);
  }

  private async fetchNearbyPlacesData(query: string): Promise<OverpassResponse | undefined> {
    for (const endpoint of ProjectMap.OVERPASS_ENDPOINTS) {
      try {
        const body = `data=${encodeURIComponent(query)}`;
        const response = await fetch(endpoint, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8',
            Accept: 'application/json',
          },
          body,
        });

        if (!response.ok) {
          continue;
        }

        const parsed = (await response.json()) as OverpassResponse;
        if (!Array.isArray(parsed.elements)) {
          continue;
        }

        return parsed;
      } catch {
        // Try the next endpoint.
      }
    }

    return undefined;
  }

  private parseNearbyPlaces(data: OverpassResponse, center?: L.LatLng): NearbyPlace[] {
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
      const place: NearbyPlace = {
        id: String(element.id),
        name,
        lat,
        lng,
        tags: element.tags ?? undefined,
      };

      if (center) {
        const meters = this.distanceMeters(center, L.latLng(lat, lng));
        place.tags = { ...(place.tags ?? {}), distanceMeters: String(Math.round(meters)) };
      }

      places.push(place);

      if (places.length >= ProjectMap.NEARBY_PLACES_LIMIT) {
        break;
      }
    }

    if (center) {
      places.sort((a, b) => {
        const da = Number(a.tags?.['distanceMeters'] ?? 0);
        const db = Number(b.tags?.['distanceMeters'] ?? 0);
        return da - db;
      });
    }

    return places;
  }

  private formatDistanceLabel(meters: number): string {
    if (meters >= 1000) {
      return `${(meters / 1000).toFixed(1)} كم`;
    }
    return `${Math.round(meters)} م`;
  }

  private renderNearbyPlaces(places: NearbyPlace[], center?: L.LatLng): void {
    if (!this.map) {
      return;
    }

    this.clearNearbyMarkers();

    for (const place of places) {
      const iconClass = this.getNearbyIconClass(place);
      const customIcon = L.divIcon({
        className: 'map-facility-marker-wrapper',
        html: `<div class="map-facility-circle"><i class="fa-solid ${iconClass}"></i></div>`,
        iconSize: [32, 32],
        iconAnchor: [16, 16],
      });

      const marker = L.marker({ lat: place.lat, lng: place.lng }, {
        icon: customIcon,
        keyboard: false,
      });

      const distanceMeters = place.tags?.['distanceMeters']
        ? Number(place.tags['distanceMeters'])
        : center
          ? this.distanceMeters(center, L.latLng(place.lat, place.lng))
          : null;

      const distanceHtml = distanceMeters != null
        ? `<span style="color: #15A94B; font-size: 0.75rem; font-weight: 700; display: inline-flex; align-items: center; margin-top: 2px; gap: 3px; line-height: 1.2;">
            <i class="fa-solid fa-route"></i> المسافة: ${this.formatDistanceLabel(distanceMeters)}
          </span>`
        : '';

      marker.bindTooltip(`
        <div style="text-align: right; direction: rtl; font-family: 'Cairo', sans-serif; margin: 0; padding: 0;">
          <strong style="color: #212529; font-size: 0.85rem; display: block; line-height: 1.2;">${place.name}</strong>
          ${distanceHtml}
        </div>
      `, {
        direction: 'top',
        offset: [0, -2],
        className: 'map-facility-tooltip',
      });

      try {
        const hasPane = !!(this.map && (this.map as any).getPane && (this.map as any).getPane('markerPane'));
        if (hasPane) {
          marker.addTo(this.map!);
        }
        this.nearbyMarkers.push(marker);
      } catch (e) {
        this.nearbyMarkers.push(marker);
      }
    }
  }

  private getNearbyIconClass(place: NearbyPlace): string {
    const amenity = String(place.tags?.['amenity'] ?? '').toLowerCase();
    const shop = String(place.tags?.['shop'] ?? '').toLowerCase();
    const tourism = String(place.tags?.['tourism'] ?? '').toLowerCase();
    const leisure = String(place.tags?.['leisure'] ?? '').toLowerCase();

    if (/mosque|place_of_worship/.test(amenity)) return 'fa-mosque';
    if (/school|university/.test(amenity)) return 'fa-graduation-cap';
    if (/hospital|clinic/.test(amenity)) return 'fa-hospital';
    if (/restaurant|cafe/.test(amenity)) return 'fa-utensils';
    if (/park|garden/.test(leisure)) return 'fa-tree';
    if (/bank/.test(amenity)) return 'fa-building-columns';
    if (/pharmacy/.test(amenity)) return 'fa-prescription-bottle-medical';
    if (/supermarket|mall/.test(shop) || /attraction/.test(tourism)) return 'fa-cart-shopping';

    return this.getFacilityIcon(place.name);
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

    if (districtWithCity) {
      return `${road}${postcode ? `,${postcode}` : ''} ${districtWithCity}`.trim();
    }

    if (road || postcode) {
      return `${road}${postcode ? `,${postcode}` : ''}`.trim();
    }

    return ProjectMap.DEFAULT_ADDRESS_LINE1;
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
