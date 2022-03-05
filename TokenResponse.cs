namespace ProjectManagement.DataUpload;

public record TokenResponse(string Token, string RefreshToken, DateTime RefreshTokenExpiryTime);