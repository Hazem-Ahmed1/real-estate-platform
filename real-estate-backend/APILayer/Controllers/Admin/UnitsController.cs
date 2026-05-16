using APILayer.Dtos.Units;
using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.UnitModule;
using BusinessLogicLayer.Specifications.Units;
using DataAccessLayer.Common;
using DataAccessLayer.Entities.ProjectModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Entities.UnitModule;

namespace APILayer.Controllers.Admin;

[ApiController]
[Route("api/admin/units")]
[Authorize(Roles = "Admin")]
public class UnitsController(IUnitService unitService, IMediaService mediaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetUnits([FromQuery] UnitSpecParams @params)
    {
        var result = await unitService.GetUnitsAsync(@params);
        return Ok(result);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> CreateUnit([FromForm] UnitCreateFormDto form)
    {
        var dto = new UnitCreateDto
        {
            Name = form.Name,
            BuildingId = form.BuildingId,
            Rooms = form.Rooms,
            Salons = form.Salons,
            Area = form.Area,
            Bathrooms = form.Bathrooms,
            Floor = form.Floor,
            Price = form.Price,
            Type = form.Type,
            Status = form.Status,
            StreetCount = form.StreetCount,
            Street = form.Street,
            Latitude = form.Latitude,
            Longitude = form.Longitude,
            FeatureIds = form.FeatureIds,
            InsuranceIds = form.InsuranceIds,
            NearbyFacilities = form.NearbyFacilities,
            ThumbnailImage = form.ThumbnailImage,
            Images = form.Images,
            Designs = form.Designs,
            Panorama360 = form.Panorama360,
            VideoFile = form.VideoFile
        };

        var created = await unitService.CreateUnitAsync(dto);
        return CreatedAtAction("GetUnit", "Units", new { id = created.UnitId }, created);
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> UpdateUnit(int id, [FromForm] UnitUpdateFormDto form)
    {
        var dto = new UnitUpdateDto
        {
            Name = form.Name,
            BuildingId = form.BuildingId,
            Rooms = form.Rooms,
            Salons = form.Salons,
            Area = form.Area,
            Bathrooms = form.Bathrooms,
            Floor = form.Floor,
            Price = form.Price,
            Type = form.Type,
            Status = form.Status,
            StreetCount = form.StreetCount,
            Street = form.Street,
            Latitude = form.Latitude,
            Longitude = form.Longitude,
            FeatureIds = form.FeatureIds,
            InsuranceIds = form.InsuranceIds,
            DeletedMediaIds = form.DeletedMediaIds,
            IsStatusChanged = form.IsStatusChanged,
            NearbyFacilities = form.NearbyFacilities,
            ThumbnailImage = form.ThumbnailImage,
            Images = form.Images,
            Designs = form.Designs,
            Panorama360 = form.Panorama360,
            VideoFile = form.VideoFile
        };

        var result = await unitService.UpdateUnitAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUnit(int id)
    {
        await unitService.DeleteUnitAsync(id);
        return NoContent();
    }
}
