namespace LibraryManagementSystem.Application.DTOs.AuthorDtos
{
    public class AuthorResponseDto
    {
        public Guid AuthorId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Nationality { get; set; }

        public int BooksCount { get; set; }

        public bool IsSuccess { get; set; } = false;

        public string Message { get; set; } = string.Empty;
    }
}
