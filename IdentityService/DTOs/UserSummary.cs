namespace IdentityService.DTOs
{
    public class UserSummary
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
