using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace ProjectManagement.DataUpload;

public class ExcelService : IExcelService
{
    private readonly ILogger<UploadDataService> _log;
    private readonly IConfiguration _config;

    public ExcelService(ILogger<UploadDataService> log, IConfiguration config)
    {
        _log = log;
        _config = config;
    }

    public List<User> GetUserDataFromExcel()
    {
        var users = new List<User>();
        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(_config["UserExcelPath"])))
            {
                var sheet = package.Workbook.Worksheets["Database"];
                int rowCount = sheet.Dimension.End.Row;
                int start = int.Parse(_config["FirstRow"]);
                int idColumn = int.Parse(_config["IdColumn"]);
                int nameColumn = int.Parse(_config["NameColumn"]);
                int phoneColumn = int.Parse(_config["PhoneColumn"]);
                for (int i = start; i < rowCount; i++)
                {
                    var id = sheet.Cells[i, idColumn].Text;
                    var name = sheet.Cells[i, nameColumn].Text;
                    var phone = sheet.Cells[i, phoneColumn].Text;
                    if (IsValid(id) && IsValid(phone))
                    {
                        users.Add(new User(name, id, phone));
                    }
                    else
                    {
                        _log.LogInformation($"name-{name} ,userid - {id}, phone - {phone}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message, ex);
        }
        return users;
    }

    private bool IsValid(string str) => !string.IsNullOrWhiteSpace(str) && long.TryParse(str, out long idNumber) && idNumber > 0 && str.Length > 5;
}