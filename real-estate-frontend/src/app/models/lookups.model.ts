export interface FeatureDto {
  featureId: number;
  name: string;
  isActive: boolean;
  unitCount: number;
  projectCount: number;
}

export interface InsuranceDto {
  insuranceId: number;
  name: string;
  duration: number;
  isActive: boolean;
  unitCount: number;
  projectCount: number;
}

export interface LookupUpsertDto {
  name: string;
  duration?: number;
}

export interface UpdateFeatureDto {
  name: string;
  isActive: boolean;
}

export interface UpdateInsuranceDto {
  name: string;
  duration: number;
  isActive: boolean;
}

export interface LookupDeleteResultDto<T> {
  message: string;
  item: T;
}
