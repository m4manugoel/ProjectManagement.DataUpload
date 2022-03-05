namespace ProjectManagement.DataUpload;

public interface ITokenService
{
    Task<Result<TokenResponse>> GetToken();

    Task<Result<TokenResponse>> GetTokenUsingRefreshToken(string token, string refreshToken);
}