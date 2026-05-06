namespace APILayer.Dtos.Auth;

public sealed record LoginRequestDto(
    string UserName,
    string Password
);
