import { FeatureDto, InsuranceDto } from './IProject';

export interface NearbyFacilityDto {
  name: string;
  type: string;
  distance: string;
  latitude?: number;
  longitude?: number;
  area?: number;
}

export interface IUnitDetails {
  unitId: number;
  name: string;
  buildingId: number;
  buildingName: string | null;
  projectName: string | null;
  rooms: number | null;
  salons: number | null;
  area: number | null;
  bathrooms: number | null;
  floor: number | null;
  price: number | null;
  type: string;
  status: string;
  streetCount: number;
  city: string | null;
  region: string | null;
  street: string | null;
  address: string | null;
  latitude: number | null;
  longitude: number | null;
  thumbnailUrl: string | null;
  videoUrl: string | null;
  panoramaUrl: string | null;
  images: string[];
  designs: string[];
  nearbyFacilities: NearbyFacilityDto[];
  features: FeatureDto[];
  insurance: InsuranceDto[];
}

/** Matches backend UnitListDto – returned by GET /api/units */
export interface UnitCardModel {
  unitId: number;
  name: string;
  status: string;        // 'Sale' | 'Rent' | 'Sold' | 'Rented'
  rooms: number;
  salons: number;
  streetCount: number;
  area: number;
  price: number;
  city: string | null;
  region: string | null;
  address: string;
  type: string;
  thumbnailUrl: string | null;
  buildingName: string;
  projectName: string;
}

/** Pagination wrapper */
export interface UnitsPage {
  items: UnitCardModel[];
  totalCount: number;
  pageSize: number;
  currentPage: number;
  totalPages: number;
}

