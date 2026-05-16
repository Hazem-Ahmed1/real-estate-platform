# Real Estate Platform — System Business Logic Document
**التاريخ:** 2026-05-15
**الحالة:** مكتمل

---

## المحتويات
1. [Entities Overview](#entities)
2. [Projects](#projects)
3. [Buildings](#buildings)
4. [Units](#units)
5. [Features](#features)
6. [Insurance](#insurance)
7. [Nearby Facilities](#nearby)
8. [Media](#media)
9. [Dashboard](#dashboard)

---

## 1. Entities Overview {#entities}

### Project Entity
| Field | Type | Source | Notes |
|---|---|---|---|
| ProjectId | int | DB Auto | PK |
| Name | string | Input | Unique globally |
| Status | ProjectStatus enum | Derived | Sale / Rent / Sold / Rented |
| City | string | Input | Free text from map |
| Area | string? | Input | Optional sub-area |
| Address | string? | Input | Optional street address |
| Latitude | double | Map input | -90 to 90 |
| Longitude | double | Map input | -180 to 180 |
| LandArea | double | Input | Total land footprint (m²) |
| BuildUpArea | double | Input | Built structures area (m²) |
| TotalBuildingArea | double? | Computed | Σ(BuildingArea_i + Max(Unit.Area) per building) |
| VideoUrl | string? | Input | Direct URL (no upload) |
| IsStatusChanged | bool | Computed | true if status changed since last save |
| AvailableUnitsCount | int | Computed | Count of Sale or Rent units |
| TransactedUnitsCount | int | Computed | Count of Sold or Rented units |
| CreatedAt | DateTime | System | Auto UTC |
| UpdatedAt | DateTime? | System | Auto on update |

### Building Entity
| Field | Type | Source | Notes |
|---|---|---|---|
| BuildingId | int | DB Auto | PK |
| ProjectId | int | Input | FK → Project |
| Name | string | Input | Unique per project |
| MaxArea | double | Input | Max allowed unit area per floor |
| BuildingArea | double | Input | Footprint of building (m²) |
| FloorCount | int? | Computed | Max(Unit.Floor) in building |

### Unit Entity
| Field | Type | Source | Notes |
|---|---|---|---|
| UnitId | int | DB Auto | PK |
| BuildingId | int | Input | FK → Building |
| Name | string | Input | Unique per building |
| Rooms | int | Input | 1–100 |
| Salons | int | Input | 1–100 |
| Bathrooms | int | Input | 1–100 |
| Area | double | Input | 100–1,000,000 m² |
| Floor | int | Input | 1–1000 |
| Price | decimal | Input | 50–∞ |
| Type | UnitType enum | Input | Villa / Apartment / etc. |
| Status | UnitStatus enum | Input | Sale / Rent / Sold / Rented |
| Street | string | Input | Street address |
| StreetCount | int | Input | 1–10 |
| Latitude | double | Map input | -90 to 90 |
| Longitude | double | Map input | -180 to 180 |
| VideoUrl | string? | Input | Direct URL |
| IsStatusChanged | bool | Computed | true if status changed |

### Feature Entity
| Field | Type | Notes |
|---|---|---|
| FeatureId | int | PK |
| Name | string | Unique |
| IsActive | bool | Default true |

### Insurance Entity
| Field | Type | Notes |
|---|---|---|
| InsuranceId | int | PK |
| Name | string | Unique |
| IsActive | bool | Default true |

### NearbyFacility Entity
| Field | Type | Notes |
|---|---|---|
| FacilityId | int | PK |
| ProjectId | int? | FK → Project (nullable) |
| UnitId | int? | FK → Unit (nullable) |
| Type | FacilityType enum | Mosque, Hospital, School, etc. |
| Name | string | Facility name |
| Area | double | Footprint m² (counted toward LandArea) |
| Distance | string? | Manual text if no coords |
| Latitude | double? | For auto distance calc |
| Longitude | double? | For auto distance calc |

---

## 2. Projects {#projects}

### Status Logic (Derived Automatically)
```
Project.Status is set at creation (Sale or Rent).
When units exist:
  - Sale-type project → Sold when ALL units are Sold
  - Rent-type project → Rented when ALL units are Rented
  - Otherwise stays Sale / Rent
When no units:
  - Returns to Sale or Rent (base type)
```
Status is recalculated on:
- Create/Update/Delete Unit
- Move Unit to another Building
- Update/Delete Building

### Area Validation Rule
```
BuildUpArea <= LandArea                                  ✅ Required
TotalBuildingArea <= BuildUpArea                         ✅ If provided
Sum(NearbyFacility.Area) + BuildUpArea <= LandArea       ✅ Checked on Create/Update
```

---

### 📌 POST /api/admin/projects
**Auth:** Bearer Token (Admin)
**Content-Type:** multipart/form-data

**Request Fields:**
| Field | Type | Required | Validation |
|---|---|---|---|
| Name | string | ✅ | Unique globally |
| City | string | ✅ | 2–100 chars |
| Area | string | ❌ | max 200 chars |
| Address | string | ❌ | max 500 chars |
| Latitude | double | ✅ | -90 to 90 |
| Longitude | double | ✅ | -180 to 180 |
| LandArea | double | ✅ | >= 0 |
| BuildUpArea | double | ✅ | >= 0, <= LandArea |
| TotalBuildingArea | double | ❌ | <= BuildUpArea |
| Status | ProjectStatus | ❌ | Default: Sale |
| FeatureIds | int[] | ❌ | Must be Active |
| InsuranceIds | int[] | ❌ | Must be Active |
| NearbyFacilities | NearbyFacilityDto[] | ❌ | Each has Name,Type,Area,Lat,Long |
| ThumbnailImage | IFormFile | ❌ | Uploaded to Cloudinary |
| Images | IFormFile[] | ❌ | Uploaded to Cloudinary |
| Panorama360 | IFormFile | ❌ | Uploaded to Cloudinary |
| VideoUrl | string | ❌ | Direct URL string |

**Business Logic:**
1. Validate Name uniqueness → 409 if duplicate
2. Validate BuildUpArea <= LandArea → 400
3. Validate Sum(NearbyFacilities.Area) + BuildUpArea <= LandArea → 400
4. Validate all FeatureIds are Active → 400 if inactive
5. Validate all InsuranceIds are Active → 400 if inactive
6. Map to Project entity, save
7. Save NearbyFacilities linked to ProjectId
8. Upload media to Cloudinary, save ProjectMedia records
9. Return 201 with full ProjectDetailsDto

**Response:** `201 Created`
```json
{
  "projectId": 1,
  "name": "Nile Towers",
  "status": "Sale",
  "isStatusChanged": false,
  "cityName": "Cairo",
  "areaName": "Maadi",
  "address": "23 Corniche St",
  "latitude": 30.0444,
  "longitude": 31.2357,
  "buildingsCount": 0,
  "unitsCount": 0,
  "availableUnitsCount": 0,
  "transactedUnitsCount": 0,
  "totalRooms": 0,
  "totalHalls": 0,
  "minPrice": 0,
  "maxPrice": 0,
  "totalBuildingArea": null,
  "totalFacilitiesArea": 500.0,
  "panorama360Url": null,
  "videoUrl": "https://youtube.com/...",
  "images": [],
  "features": [],
  "insurance": [],
  "nearbyFacilities": [
    { "name": "Al Nour Mosque", "type": "Mosque", "area": 500, "latitude": 30.045, "longitude": 31.236, "distance": "120 m" }
  ]
}
```

---

### 📌 GET /api/projects (Public)
**Auth:** None
**Query Params:**
| Param | Type | Notes |
|---|---|---|
| page | int | Default 1 |
| pageSize | int | Default 10, Max 50 |
| status | ProjectStatus | Sale / Rent only |
| city | string | Contains filter |
| area | string | Contains filter |
| search | string | Name contains |

**Business Logic:**
- `PublicOnly = true` → filters only Sale/Rent projects
- Hides `IsStatusChanged` (returned as null)
- Cannot filter by Sold/Rented unit status → 400

**Response:** `200 OK` — Paginated list of ProjectListDto

---

### 📌 GET /api/admin/projects (Admin)
**Auth:** Bearer Token (Admin)
**Query Params:** Same as public + can filter by any Status including Sold/Rented

**Response:** `200 OK` — Full paginated list including all statuses, `IsStatusChanged` included

---

### 📌 GET /api/projects/{id} (Public)
**Auth:** None

**Business Logic:**
- If Status is Sold or Rented → 404 (hidden from public)
- `IsStatusChanged` → returned as null

**Response:** `200 OK` — Full ProjectDetailsDto

---

### 📌 GET /api/admin/projects/{id} (Admin)
**Auth:** Bearer Token (Admin)

**Business Logic:**
- Returns regardless of status
- Includes `IsStatusChanged` flag

---

### 📌 PUT /api/admin/projects/{id}
**Auth:** Bearer Token (Admin)
**Content-Type:** multipart/form-data

**Request Fields:** Same as Create, plus:
| Field | Type | Notes |
|---|---|---|
| DeletedMediaIds | int[] | Media IDs to delete |
| IsStatusChanged | bool | Manual override flag |
| NearbyFacilities | NearbyFacilityDto[] | Replaces all existing |

**Business Logic:**
1. Validate Name unique (excluding current ID)
2. Validate Area constraints
3. Calculate: `buildingsBuildUpArea + totalFacilitiesArea <= LandArea`
4. Map updates to existing entity
5. Clear and re-add NearbyFacilities
6. Delete removed media from Cloudinary by PublicId
7. Prevent duplicate thumbnail/video/panorama (auto-replace)
8. Recalculate Project Status and unit counts
9. Recalculate Building FloorCounts
10. Recalculate TotalBuildingArea

**Response:** `200 OK` — Updated ProjectDetailsDto

---

### 📌 DELETE /api/admin/projects/{id}
**Auth:** Bearer Token (Admin)

**Business Logic:**
- Has Buildings → 400 (must delete buildings first)
- Status is Sold/Rented → 400 (cannot delete transacted project)
- Deletes all media from Cloudinary
- Hard deletes Project (cascades Media, Features, Insurance links)

**Response:** `204 No Content`

---

## 3. Buildings {#buildings}

### 📌 GET /api/admin/buildings?projectId={id}
**Auth:** Bearer Token (Admin)

**Response:** `200 OK`
```json
[{ "buildingId": 1, "projectId": 1, "name": "Tower A", "maxArea": 200.0, "buildingArea": 500.0, "floorCount": 10 }]
```

---

### 📌 GET /api/admin/buildings/{id}
**Auth:** Bearer Token (Admin)
**Response:** `200 OK` — Single BuildingDto

---

### 📌 POST /api/admin/buildings
**Auth:** Bearer Token (Admin)
**Content-Type:** application/json

**Request:**
```json
{
  "name": "Tower A",
  "projectId": 1,
  "maxArea": 200.0,
  "buildingArea": 500.0,
  "floorCount": null
}
```

**Business Logic:**
1. Validate ProjectId exists → 404
2. Validate Name unique per project → 409
3. Save Building

**Response:** `200 OK` — BuildingDto

---

### 📌 PUT /api/admin/buildings/{id}
**Auth:** Bearer Token (Admin)

**Business Logic:**
1. If ProjectId changes → validate new project exists
2. Validate Name unique in target project
3. Update building
4. If ProjectId changed → recalculate both old and new project status

**Response:** `200 OK` — BuildingDto

---

### 📌 DELETE /api/admin/buildings/{id}
**Auth:** Bearer Token (Admin)

**Business Logic:**
- Has Sold/Rented units → 400 (cannot delete)
- Has Sale/Rent units → deletes them (cleanup Cloudinary media)
- Recalculates parent Project status after deletion

**Response:** `204 No Content`

---

## 4. Units {#units}

### 📌 POST /api/admin/units
**Auth:** Bearer Token (Admin)
**Content-Type:** multipart/form-data

**Request Fields:**
| Field | Type | Required | Validation |
|---|---|---|---|
| Name | string | ✅ | Unique per building |
| BuildingId | int | ✅ | Must exist |
| Rooms | int | ✅ | 1–100 |
| Salons | int | ✅ | 1–100 |
| Area | double | ✅ | 100–1,000,000, <= Building.MaxArea |
| Bathrooms | int | ✅ | 1–100 |
| Floor | int | ✅ | 1–1000 |
| Price | decimal | ✅ | >= 50 |
| Type | UnitType | ✅ | |
| Status | UnitStatus | ✅ | Must match project type |
| Street | string | ✅ | max 500 chars |
| StreetCount | int | ✅ | 1–10 |
| Latitude | double | ✅ | -90 to 90 |
| Longitude | double | ✅ | -180 to 180 |
| FeatureIds | int[] | ❌ | Must be Active |
| InsuranceIds | int[] | ❌ | Must be Active |
| NearbyFacilities | NearbyFacilityDto[] | ❌ | Unit-specific nearby |
| ThumbnailImage | IFormFile | ❌ | |
| Images | IFormFile[] | ❌ | |
| Designs | IFormFile[] | ❌ | |
| Panorama360 | IFormFile | ❌ | |
| VideoUrl | string | ❌ | Direct URL |

**Business Logic:**
1. Validate Building exists → 404
2. Validate Project exists → 404
3. Status consistency: Sale-project → only Sale/Sold units allowed → 400
4. Validate Villa uniqueness: only one building per project can have Villa units → 400
5. Validate Area <= Building.MaxArea → 400
6. Validate Name unique per building → 409
7. Save Unit with Features, Insurance, NearbyFacilities
8. Upload media to Cloudinary
9. Recalculate Project: Status, AvailableCount, TransactedCount, FloorCount, TotalBuildingArea

**Response:** `201 Created` — Full GetUnitDto

---

### 📌 GET /api/units (Public)
**Auth:** None
**Query Params:**
| Param | Type | Notes |
|---|---|---|
| page | int | Default 1 |
| pageSize | int | Max 50 |
| city | string | Project.City contains |
| area | string | Project.Area contains |
| type | UnitType | Filter by type |
| status | UnitStatus | Sale / Rent only |
| rooms | int | Exact match |
| minPrice | decimal | |
| maxPrice | decimal | |
| sort | string | priceAsc/priceDesc/newest/areaAsc/areaDesc |
| search | string | Street contains |
| buildingId | int | Filter by building |

**Business Logic:**
- `PublicOnly = true` auto-set → hides Sold/Rented units
- `IsStatusChanged` hidden (null)

**Response:** `200 OK` — Paginated UnitListDto

---

### 📌 GET /api/units/{id} (Public)
**Auth:** None

**Business Logic:**
- Status is Sold/Rented → 404
- Returns combined NearbyFacilities (Project-level + Unit-level)
- Distance auto-calculated: Haversine formula between Unit coords and each facility's coords
- Format: "120 m" or "1.5 km"

**Response:** `200 OK`
```json
{
  "unitId": 1,
  "name": "Unit 101",
  "buildingId": 1,
  "buildingName": "Tower A",
  "projectName": "Nile Towers",
  "rooms": 3, "salons": 1, "area": 150.0,
  "bathrooms": 2, "floor": 3, "price": 1500000,
  "type": "Apartment", "status": "Sale",
  "streetCount": 3, "street": "Main St",
  "latitude": 30.045, "longitude": 31.235,
  "videoUrl": null,
  "media": [{"mediaId":1,"mediaUrl":"https://...","type":"Image","isThumbnail":true}],
  "nearbyFacilities": [
    {"name": "Al Nour Mosque", "type": "Mosque", "area": 500, "latitude": 30.046, "longitude": 31.236, "distance": "120 m"}
  ],
  "features": [{"featureId":1,"name":"Pool","isActive":true}],
  "insurance": [{"insuranceId":1,"name":"Fire Insurance","isActive":true}]
}
```

---

### 📌 GET /api/admin/units
**Auth:** Bearer Token (Admin)
**Query Params:** Same as public, NO PublicOnly restriction — can see Sold/Rented
**Response:** Includes `IsStatusChanged`

---

### 📌 PUT /api/admin/units/{id}
**Auth:** Bearer Token (Admin)

**Business Logic:**
1. Validate building/project constraints same as Create
2. Cannot move Sold/Rented unit to another building → 400
3. If status changed → `IsStatusChanged = true`
4. Clear and re-add Features, Insurance, NearbyFacilities
5. Handle media: delete old by PublicId, upload new
6. Auto-replace if duplicate thumbnail/video/panorama
7. Recalculate old project + new project if building changed

**Response:** `200 OK` — Updated GetUnitDto

---

### 📌 DELETE /api/admin/units/{id}
**Auth:** Bearer Token (Admin)

**Business Logic:**
- Status Sold/Rented → 400
- Cleanup all media from Cloudinary
- Hard delete unit
- Recalculate parent project

**Response:** `204 No Content`

---

## 5. Features {#features}

### Endpoints
| Method | Path | Auth | Description |
|---|---|---|---|
| GET | /api/lookups/features/public | Public | Active features only |
| GET | /api/admin/lookups/features | Admin | All (Active + Inactive) |
| GET | /api/admin/lookups/features/{id} | Admin | Single feature |
| POST | /api/admin/lookups/features | Admin | Create feature |
| PUT | /api/admin/lookups/features/{id} | Admin | Update name/status |
| DELETE | /api/admin/lookups/features/{id} | Admin | Smart delete |

### POST /api/admin/lookups/features
**Request:**
```json
{ "name": "Swimming Pool" }
```
**Logic:** Unique name check → 409 if exists. Created with `IsActive = true`.

### DELETE Logic (Smart Delete)
```
Feature used in Projects or Units → IsActive = false (soft disable)
Feature NOT used anywhere         → Hard delete from DB
```

---

## 6. Insurance {#insurance}

Same structure as Features:

| Method | Path | Auth |
|---|---|---|
| GET | /api/lookups/insurance/public | Public |
| GET | /api/admin/lookups/insurance | Admin |
| POST | /api/admin/lookups/insurance | Admin |
| PUT | /api/admin/lookups/insurance/{id} | Admin |
| DELETE | /api/admin/lookups/insurance/{id} | Admin |

**DELETE Logic:** Same smart delete as Features.

---

## 7. Nearby Facilities {#nearby}

Linked to either a **Project** (ProjectId) or a **Unit** (UnitId).

### On Unit Detail Response
- System merges: `Project.NearbyFacilities + Unit.NearbyFacilities`
- For each facility with Lat/Long → distance computed via **Haversine**:
  ```
  R = 6,371,000 m
  distance = R × 2 × atan2(√a, √(1-a))
  ```
- Output: "120 m" if < 1000m, else "1.2 km"

### Managed via
- **Project Create/Update**: pass in `NearbyFacilities[]` array → replaces all project-level facilities
- **Unit Create/Update**: pass in `NearbyFacilities[]` array → replaces all unit-level facilities

---

## 8. Media {#media}

### Types Supported
| Type | Enum Value | Notes |
|---|---|---|
| Image | Image | Thumbnail + Gallery |
| Panorama 360 | Panorama360 | VR viewer |

### Storage
- **Images & Panorama**: Uploaded via Cloudinary. Stored: `MediaUrl` + `PublicId`.
- **Video**: Stored as `VideoUrl` (string) — no upload, just external link.

### Thumbnail Rule
- `IsThumbnail = true` → one per Project/Unit. Auto-replaced on update.

### Cloudinary Cleanup
- On delete of project/unit/media → `PublicId` used to call Cloudinary delete API
- Ensures no orphaned cloud files

---

## 9. Dashboard {#dashboard}

### 📌 GET /api/admin/dashboard/stats
**Auth:** Bearer Token (Admin)

**Response Structure:**
```json
{
  "summary": {
    "totalProjects": 12,
    "totalBuildings": 45,
    "totalUnits": 320,
    "totalBlogs": 8,
    "totalMessages": 152,
    "totalFeatures": 10,
    "totalInsurance": 5
  },
  "inventoryStatus": {
    "forSale": 120,
    "forRent": 80,
    "sold": 100,
    "rented": 20,
    "occupancyRate": 37.5
  },
  "projectHealth": {
    "sale": 7,
    "rent": 3,
    "sold": 1,
    "rented": 1
  },
  "insights": {
    "typeDistribution": [
      { "type": "Apartment", "count": 200 },
      { "type": "Villa", "count": 50 }
    ],
    "cityInsights": [
      { "city": "Cairo", "projects": 8, "totalUnits": 230 },
      { "city": "Alexandria", "projects": 4, "totalUnits": 90 }
    ],
    "topPerformingProjects": [
      { "projectId": 1, "name": "Nile Towers", "salesProgress": 85.5, "totalUnits": 40 }
    ],
    "financials": {
      "totalInventoryValue": 450000000,
      "avgPrice": 1406250.0
    }
  }
}
```

### How Each Value is Calculated
| Value | Calculation |
|---|---|
| occupancyRate | `(sold + rented) / totalUnits × 100` |
| totalInventoryValue | `Sum(Unit.Price) WHERE Status = Sale` |
| avgPrice | `Average(Unit.Price)` across all units |
| salesProgress | `TransactedUnitsCount / (Transacted+Available) × 100` |
| cityInsights | Group Projects by `Project.City` string |
| typeDistribution | Group Units by `Unit.Type` enum |

---

## Security Summary

| Endpoint Group | Token Required | Role |
|---|---|---|
| GET /api/projects | ❌ | Public |
| GET /api/projects/{id} | ❌ | Public |
| GET /api/units | ❌ | Public |
| GET /api/units/{id} | ❌ | Public |
| GET /api/lookups/*/public | ❌ | Public |
| GET /api/buildings (public) | ❌ | Public |
| ALL /api/admin/* | ✅ JWT Bearer | Admin role |
| GET /api/admin/dashboard/* | ✅ JWT Bearer | Admin role |

**Public Restrictions:**
- Cannot see Sold/Rented units/projects (404)
- `IsStatusChanged` hidden (null)
- Cannot filter by Sold/Rented unit status

**Admin Privileges:**
- Full CRUD on all entities
- See all statuses including Sold/Rented
- Access Dashboard analytics
- Manage Features/Insurance active state
