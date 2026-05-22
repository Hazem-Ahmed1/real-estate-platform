using APILayer.Dtos.Units;
using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.UnitModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers.Admin;

[ApiController]
[Route("api/admin/units")]
[Authorize(Roles = "Admin")]
public class UnitsController(IUnitService unitService) : ControllerBase
{
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
            City = form.City,
            Region = form.Region,
            Street = form.Street,
            Address = form.Address,
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
        return CreatedAtAction(nameof(CreateUnit), new { id = created.UnitId }, created);
    }

    [HttpGet]
    public async Task<ActionResult> GetAllUnits([FromQuery] BusinessLogicLayer.Specifications.Units.UnitSpecParams @params)
    {
        var result = await unitService.GetAdminUnitsAsync(@params);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetUnit(int id)
    {
        var unit = await unitService.GetUnitByIdAsync(id);
        if (unit is null)
        {
            return NotFound();
        }

        return Ok(unit);
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
            City = form.City,
            Region = form.Region,
            Street = form.Street,
            Address = form.Address,
            Latitude = form.Latitude,
            Longitude = form.Longitude,
            FeatureIds = form.FeatureIds,
            InsuranceIds = form.InsuranceIds,
            DeletedMediaIds = form.DeletedMediaIds,
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
