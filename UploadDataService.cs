using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProjectManagement.DataUpload;
using System.Net.Http.Headers;
using System.Text;

public interface IUploadDataService
{
    Task UploadData();
}

public class UploadDataService : IUploadDataService
{
    private readonly ILogger<UploadDataService> _log;
    private readonly IConfiguration _config;
    private readonly IExcelService _excelService;
    private readonly ITokenService _tokenService;
    private readonly HttpClient _httpClient;
    private int counter = 0;

    private TokenResponse _token;
    public UploadDataService(ILogger<UploadDataService> log, IConfiguration config, ITokenService tokenService, HttpClient httpClient, IExcelService excelService)
    {
        _log = log;
        _config = config;
        _httpClient = httpClient;
        _tokenService = tokenService;
        _excelService = excelService;
    }

    public async Task UploadData()
    {
        var tokenResponse = await _tokenService.GetToken();
        _token = tokenResponse.Data;
        _log.LogInformation($"Token generated {_token.Token}");
        var users = _excelService.GetUserDataFromExcel();
        if (users.Any())
        {
            await UploadUsers(users);
        }
    }

    private async Task UploadUsers(List<User> users)
    {
        List<Task> tasks = new List<Task>();
        SetHttpClient();
        foreach (var user in users)
        {
            async Task UploadUserRequest()
            {
                await UploadUser(user);
            }

            tasks.Add(UploadUserRequest());
            counter++;
            if (counter == 50)
            {
                await Task.WhenAll(tasks);
                tasks = new List<Task>();
                _log.LogInformation("Batch Completed!!");
                counter = 0;
            }
        }

        await Task.WhenAll(tasks);
        _log.LogInformation("Task Completed!!");
        Console.ReadLine();
    }

    private async Task UploadUser(User user)
    {
        var userDetails = new UserDetail(user.Name, string.Empty, $"{user.Id}@test.com", user.Id, user.Phone, user.Phone, user.Phone);
        var json = JsonConvert.SerializeObject(userDetails, new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                IgnoreSerializableAttribute = false
            }
        });
        HttpContent content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var url = $"{_config["ApiUrl"]}/api/identity/register";
        try
        {
            var response = await _httpClient.PostAsync(url, content);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var _content = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(_content))
                {
                    var responseContent = JsonConvert.DeserializeObject<Result<string>>(_content);
                    if (responseContent.Succeeded)
                    {
                        _log.LogInformation(string.Join(",", responseContent.Messages));
                    }
                    else
                    {
                        _log.LogError(string.Join(",", responseContent.Messages));
                    }
                }
                else
                {
                    _log.LogError($"user not created: {json}");
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _log.LogError("Unauthorized...trying to refresh token");
                await RefreshToken();
                await UploadUser(user);
            }
            else
            {
                try
                {
                    var _content = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(_content))
                    {
                        var errorResponse = JsonConvert.DeserializeObject<ErrorResult<string>>(_content);
                        _log.LogError(errorResponse.Exception);
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError(ex.Message, ex);
                }
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message, ex);
        }
    }

    private void SetHttpClient()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("tenant", _config["Tenant"]);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token.Token}");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private async Task RefreshToken()
    {
        var tokenResponse = await _tokenService.GetTokenUsingRefreshToken(_token.Token, _token.RefreshToken);
        _token = tokenResponse.Data;
        _log.LogInformation($"New token generated {_token.Token}");
    }
}