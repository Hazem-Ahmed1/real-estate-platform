export interface AdminBuildingDto {
  buildingId: number;
  projectId: number;
  projectName: string;
  name: string;
  type: string;
  status?: string | null;
  maxArea: number;
  buildingArea: number;
  floorCount?: number | null;
  units?: Array<{ unitId: number }>;
}

export interface AdminBuildingUpsertDto {
  name: string;
  projectId: number;
  maxArea: number;
  buildingArea: number;
  type: string;
  floorCount?: number | null;
}
