namespace IdentityService.DTOs
{
    public class AuthResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public TokenDTO Tokens { get; set; }
        public UserDTO User { get; set; }
    }
}
