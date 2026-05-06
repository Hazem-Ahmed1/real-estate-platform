using APILayer.Dtos.Auth;
using APILayer.Options;
using APILayer.Services;
using DataAccessLayer.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace APILayer.Controllers;

public class AuthController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    JwtTokenService tokenService,
    IOptions<JwtSettings> jwtOptions
) : ApiController
{
    private const string AdminRoleName = "Admin";

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and password are required.");
        }

        var user = await userManager.FindByNameAsync(request.UserName);
        if (user is null)
        {
            return Unauthorized("Invalid credentials.");
        }

        var isValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isValid)
        {
            return Unauthorized("Invalid credentials.");
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = tokenService.CreateToken(user, roles);

        var expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpiresMinutes);
        return Ok(new LoginResponseDto(token, expiresAt));
    }

    [HttpPost("admin")]
    public async Task<ActionResult<LoginResponseDto>> CreateAdmin([FromBody] CreateAdminRequestDto request)
    {
        var hasAnyUsers = await userManager.Users.AnyAsync();

        // Bootstrap: if there are no users yet, allow creating the first admin without authentication.
        // After bootstrap: only authenticated Admins can create additional admin accounts.
        if (hasAnyUsers)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Unauthorized("Authentication is required.");
            }

            if (!User.IsInRole(AdminRoleName))
            {
                return Forbid();
            }
        }

        if (string.IsNullOrWhiteSpace(request.UserName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("UserName, Email, and Password are required.");
        }

        if (!await roleManager.RoleExistsAsync(AdminRoleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(AdminRoleName));
            if (!roleResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create Admin role.");
            }
        }

        var existingByName = await userManager.FindByNameAsync(request.UserName);
        if (existingByName is not null)
        {
            return Conflict("Username already exists.");
        }

        var existingByEmail = await userManager.FindByEmailAsync(request.Email);
        if (existingByEmail is not null)
        {
            return Conflict("Email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            EmailConfirmed = true
        };

        var created = await userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
        {
            var errors = string.Join(" | ", created.Errors.Select(e => e.Description));
            return BadRequest(errors);
        }

        var addedToRole = await userManager.AddToRoleAsync(user, AdminRoleName);
        if (!addedToRole.Succeeded)
        {
            var errors = string.Join(" | ", addedToRole.Errors.Select(e => e.Description));
            return StatusCode(StatusCodes.Status500InternalServerError, errors);
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = tokenService.CreateToken(user, roles);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpiresMinutes);

        return Ok(new LoginResponseDto(token, expiresAt));
    }
}
