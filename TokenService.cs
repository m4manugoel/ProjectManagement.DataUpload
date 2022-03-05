using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;
using System.Text;

namespace ProjectManagement.DataUpload;

public class TokenService : ITokenService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TokenService> _log;
    private readonly IConfiguration _config;

    public TokenService(ILogger<TokenService> log, IConfiguration config, HttpClient httpClient)
    {
        _log = log;
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<Result<TokenResponse>> GetToken()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("tenant", _config["Tenant"]);
        var credentials = new Credentials(_config["User"], _config["Password"]);
        var json = JsonConvert.SerializeObject(credentials, new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                IgnoreSerializableAttribute = false
            }
        });

        HttpContent content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        try
        {
            var response = await _httpClient.PostAsync($"{_config["ApiUrl"]}/api/tokens", content);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var _content = await response.Content.ReadAsStringAsync();
                return !string.IsNullOrWhiteSpace(_content) ? JsonConvert.DeserializeObject<Result<TokenResponse>>(_content) : new Result<TokenResponse>();
            }
        }
        catch (Exception e)
        {
            _log.LogError(e.Message, e);
        }
        return new Result<TokenResponse>();
    }

    public async Task<Result<TokenResponse>> GetTokenUsingRefreshToken(string token, string refreshToken)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("tenant", _config["Tenant"]);
        var request = new { Token = token, RefreshToken = refreshToken };
        var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                IgnoreSerializableAttribute = false
            }
        });
        HttpContent content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        try
        {
            var response = await _httpClient.PostAsync($"{_config["ApiUrl"]}/api/tokens/refresh", content);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var _content = await response.Content.ReadAsStringAsync();
                return !string.IsNullOrWhiteSpace(_content) ? JsonConvert.DeserializeObject<Result<TokenResponse>>(_content) : new Result<TokenResponse>();
            }
        }
        catch (Exception e)
        {
            _log.LogError(e.Message, e);
        }
        return new Result<TokenResponse>();
    }
}