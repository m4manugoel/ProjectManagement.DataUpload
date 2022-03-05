namespace ProjectManagement.DataUpload;

public interface IExcelService
{
    List<User> GetUserDataFromExcel();
}