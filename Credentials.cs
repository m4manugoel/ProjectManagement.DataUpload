namespace ProjectManagement.DataUpload
{
    public class Credentials
    {
        public Credentials()
        { 
        }

        public Credentials(string email, string password)
        {
            Email = email;
            Password = password;
        }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
