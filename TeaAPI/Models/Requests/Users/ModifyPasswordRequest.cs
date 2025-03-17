namespace TeaAPI.Models.Requests.Users
{
    public class ModifyPasswordRequest
    {
        public int Id { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
