export interface IProjectDetails {
  projectName: string;
  projectType: string;
  locationText: string;
  stats: {
    label: string;
    value: string | number;
    iconUrl?: string;
  }[];
  media: {
    panoramaImage: string;
    videoUrl: string;
    videoPoster: string;
    gallery: string[];
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
  map: {
    lat: number;
    lng: number;
  };
}
