namespace APILayer.Dtos.Auth;

public sealed record CreateAdminRequestDto(
    string UserName,
    string Email,
    string Password
);
