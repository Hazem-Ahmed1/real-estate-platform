/** Matches backend ProjectListDto – returned by GET /api/projects */
export interface IProject {
  projectId: number;
  name: string;
  status: string;        // 'Sale' | 'Rent' | 'Sold' | 'Rented'
  city: string;
  region: string;
  landArea: number;
  address: string;
  totalRooms: number;
  totalHalls: number;
  thumbnailUrl: string | null;
  buildingsNumber: number;
  unitsNumber: number;
}

export interface FeatureDto {
  featureId: number;
  name: string;
  isActive: boolean;
}

export interface InsuranceDto {
  insuranceId: number;
  name: string;
  duration: number;
  isActive: boolean;
}

export interface ProjectMediaDto {
  mediaId: number;
  projectId: number;
  type: string;
  mediaUrl: string;
  thumbnailUrl: string | null;
  isThumbnail: boolean;
}

export interface BuildingDto {
  buildingId: number;
  name: string;
  buildingArea: number;
}

export interface IProjectDetails {
  projectId: number;
  name: string;
  status: string;
  cityName: string;
  regionName: string | null;
  address: string | null;
  latitude: number | null;
  longitude: number | null;
  buildUpArea: number;
  buildings: BuildingDto[];
  buildingsNumber: number;
  unitsNumber: number;
  availableUnitsCount: number;
  transactedUnitsCount: number;
  totalRooms: number;
  totalHalls: number;
  minPrice: number;
  maxPrice: number;
  totalBuildingArea: number | null;
  totalFacilitiesArea: number;
  thumbnailUrl: string | null;
  panorama360Url: string | null;
  videoUrl: string | null;
  images: string[];
  media: ProjectMediaDto[];
  features: FeatureDto[];
  insurance: InsuranceDto[];
}

/** Pagination wrapper */
export interface ProjectsPage {
  items: IProject[];
  totalCount: number;
  pageSize: number;
  currentPage: number;
  totalPages: number;
}
