export interface IUnitDetails {
  unitName: string;
  unitType: string;
  locationText: string;
  priceText: string;
  metadata: {
    label: string;
    value: string;
    iconClass: string;
  }[];
  media: {
    panoramaImage: string;
    videoUrl: string;
    videoPoster: string;
    gallery: string[];
    designGallery: string[];
  };
  features: {
    label: string;
    iconClass: string;
  }[];
  warranties: {
    label: string;
    durationText: string;
    iconClass: string;
  }[];
  nearbyPlaces: {
    name: string;
    distanceText: string;
  }[];
  map: {
    lat: number;
    lng: number;
  };
}
