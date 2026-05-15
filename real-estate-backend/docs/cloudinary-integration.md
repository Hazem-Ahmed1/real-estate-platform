# Cloudinary Integration (3‑Tier) — Documentation

## 1) Project structure (3 tiers)
This backend follows a **3‑Tier** architecture:

- **APILayer**
  - Exposes HTTP endpoints (Controllers)
  - Validates request shapes (e.g., model state, file exists)
  - Calls Business Logic interfaces (services)

- **BusinessLogicLayer (BLL)**
  - Contains business services + contracts (interfaces)
  - Holds integrations with external providers (Cloudinary in this case)
  - Returns DTOs to the API layer

- **DataAccessLayer (DAL)**
  - Entity Framework Core DbContext + Entities + Repositories
  - Responsible for persisting data (e.g., saving the uploaded image URL in tables)

The **API** should not talk directly to Cloudinary or EF Core; it should go through the **BLL** contracts.

---

## 2) What existed already
Cloudinary integration was already present in the BLL:

- `CloudinaryDotNet` NuGet package is referenced in **BusinessLogicLayer**.
- A settings class exists: `BusinessLogicLayer/Helpers/CloudinarySettings.cs`.
- A Cloudinary upload/delete implementation exists: `BusinessLogicLayer/Implementation/MediaService.cs`.
- Dependency injection (DI) + configuration binding existed in `APILayer/Program.cs`:
  - `builder.Services.AddScoped<IMediaService, MediaService>();`
  - `builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));`

Also, `APILayer/appsettings.json` already contains the `CloudinarySettings` section with placeholders.

---

## 3) What was added/changed
### 3.1 New DTO to return upload results
Created:
- `BusinessLogicLayer/Dtos/MediaModule/MediaUploadResultDto.cs`

Purpose:
- Return both:
  - `Url` (the Cloudinary **secure URL** to store/display)
  - `PublicId` (Cloudinary identifier used later for **delete/update**)

### 3.2 Updated media contract + implementation
Changed:
- `BusinessLogicLayer/Contracts/IMediaService.cs`
- `BusinessLogicLayer/Implementation/MediaService.cs`

Before:
- `UploadImageAsync(IFormFile file)` returned only a `string` URL.

After:
- `UploadImageAsync(IFormFile file)` returns `MediaUploadResultDto` containing `{ Url, PublicId }`.

This change is important because:
- You can store `Url` in your DB.
- You can store `PublicId` to later delete the asset using `DeleteImageAsync(publicId)`.

### 3.3 New API endpoint
Created:
- `APILayer/Controllers/MediaController.cs`

Endpoint:
- `POST /api/media/upload`

Consumes:
- `multipart/form-data`

Form field name:
- `file` (type: `IFormFile`)

Response:
- `{ "url": "https://...", "publicId": "..." }`

Basic validation in the endpoint:
- File is required (not null and `Length > 0`)
- File must be an image (`ContentType` starts with `image/`)

---

## 4) Configuration: set your Cloudinary credentials
Update:
- `APILayer/appsettings.json`

```json
"CloudinarySettings": {
  "CloudName": "YOUR_CLOUD_NAME",
  "ApiKey": "YOUR_API_KEY",
  "ApiSecret": "YOUR_API_SECRET"
}
```

Notes:
- Do **not** commit real credentials.
- Prefer environment variables / user-secrets for local dev.

---

## 5) How the upload works (flow)
1. Client sends `multipart/form-data` with an image in `file`.
2. `APILayer` receives it in `MediaController.Upload`.
3. Controller calls BLL: `IMediaService.UploadImageAsync(file)`.
4. `MediaService`:
   - Creates a `Cloudinary` client from the configured credentials.
   - Uploads the stream using `Cloudinary.UploadAsync(...)`.
  - No resizing/cropping transformation is applied (uploads the original image as-is).
   - Returns `SecureUrl` + `PublicId`.
5. API returns the result to the client.

---

## 6) How to call the endpoint
### 6.1 Swagger (recommended)
- Run the API and open: `https://localhost:<PORT>/swagger`
- Use: `Media` → `POST /api/media/upload`
- Upload an image file

### 6.2 cURL (Windows)
```bash
curl.exe -X POST "https://localhost:<PORT>/api/media/upload" -F "File=@C:\path\to\image.jpg"
```

Example response:
```json
{
  "url": "https://res.cloudinary.com/<cloud>/image/upload/v123/...jpg",
  "publicId": "some-folder/abc123"
}
```

---

## 6.3 Post a blog with image files (Cloudinary upload happens inside the request)
The blog create endpoint is **Admin-only** and accepts **multipart/form-data** so you send image files (not URLs).

Endpoint:
- `POST /api/blogs`

Authorization header:
- `Authorization: Bearer <JWT>`

Form fields:
- `Title` (string, required)
- `Description` (string, optional)
- `PublishDate` (datetime, optional)
- `Thumbnail` (file, optional)
- `Images` (file, optional, repeatable: send multiple parts named `Images`)

Example (Windows curl):
```bash
curl.exe -X POST "https://localhost:<PORT>/api/blogs" ^
  -H "Authorization: Bearer <JWT>" ^
  -F "Title=My first blog" ^
  -F "Description=Hello from the dashboard" ^
  -F "Thumbnail=@C:\path\to\thumb.jpg" ^
  -F "Images=@C:\path\to\img1.jpg" ^
  -F "Images=@C:\path\to\img2.jpg"
```

What happens on the server:
- API uploads the received files to Cloudinary.
- API stores the returned `secure_url` values into `BlogImage.ImageUrl`.
- Response returns the created blog with its images.

---

## 7) How to store the returned URL in your database
Your DAL entities already support URL storage:

- `DataAccessLayer/Entities/ProjectModule/ProjectMedia.cs` has `MediaUrl` and `ThumbnailUrl`
- `DataAccessLayer/Entities/UnitModule/UnitMedia.cs` has `MediaUrl` and `ThumbnailUrl`

Typical persistence approach:
1. Upload image → get `{ Url, PublicId }`.
2. Create a `ProjectMedia` (or `UnitMedia`) record.
3. Set `MediaUrl = Url`.
4. (Recommended) Add a new DB column like `CloudinaryPublicId` to store `PublicId` for later deletion.

Why store `PublicId`?
- Cloudinary delete uses `publicId`, not the URL.

---

## 8) How to add a new endpoint (repeatable checklist)
Use this pattern for any new feature endpoint (not only media):

### Step A — Define the contract (BLL)
- Add a method to an interface inside `BusinessLogicLayer/Contracts/`.

Example:
```csharp
public interface IFooService
{
    Task<FooDto> DoSomethingAsync(...);
}
```

### Step B — Implement the service (BLL)
- Add implementation inside `BusinessLogicLayer/Implementation/`.
- Keep external integrations here (Cloudinary, payment gateway, etc.).

### Step C — Register the service in DI (API)
In `APILayer/Program.cs`:
```csharp
builder.Services.AddScoped<IFooService, FooService>();
```

### Step D — Create the controller endpoint (API)
- Create a controller in `APILayer/Controllers/`.
- Inherit from `ApiController` (your base controller).
- Add an action with `[HttpGet]`, `[HttpPost]`, etc.

Example:
```csharp
public class FooController(IFooService fooService) : ApiController
{
    [HttpPost]
    public async Task<ActionResult<FooDto>> Create([FromBody] FooCreateDto dto)
    {
        var result = await fooService.CreateAsync(dto);
        return Ok(result);
    }
}
```

### Step E — (Optional) Add DAL persistence
- Use existing repositories/unit-of-work to create/update entities.
- Keep EF Core logic out of the controller.

### Step F — Validate
- `dotnet build`
- Call endpoint via Swagger or cURL

---

## 9) Files changed/added (for reference)
Added:
- `APILayer/Controllers/MediaController.cs`
- `BusinessLogicLayer/Dtos/MediaModule/MediaUploadResultDto.cs`

Modified:
- `BusinessLogicLayer/Contracts/IMediaService.cs`
- `BusinessLogicLayer/Implementation/MediaService.cs`

---

## 10) Next recommended step
If you want **upload + save to DB** in one call (e.g., upload a media item to a specific project):
- Add an endpoint like `POST /api/projects/{projectId}/media` that:
  1) uploads to Cloudinary
  2) inserts a `ProjectMedia` row with `MediaUrl` (and optionally `PublicId`)

This keeps the API convenient and ensures the DB stays consistent.
