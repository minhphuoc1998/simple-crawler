namespace WebApi.Models;

public class AuthSettings {
    public required string JwtSecret { get; set; }
}

public class LoginDto {
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class AuthenticatedUser {
    public required string Username { get; set; }
    public required string Token { get; set; }
}
