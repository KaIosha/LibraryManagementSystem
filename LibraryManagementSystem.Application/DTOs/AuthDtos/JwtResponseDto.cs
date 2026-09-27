namespace LibraryManagementSystem.Application.DTOs.AuthDtos
{
    public class JwtResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpireAt { get; set; }
    }
}
