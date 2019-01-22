namespace Student.Models
{
    public class UserModel
    {
        public string Username { get; set; }
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
        public long AccessToken_ExpDate { get; set; }
    }
}