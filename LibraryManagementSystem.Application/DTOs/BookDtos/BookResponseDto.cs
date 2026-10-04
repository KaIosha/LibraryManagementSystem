namespace LibraryManagementSystem.Application.DTOs.BookDtos
{
    public class BookResponseDto
    {
        public Guid BookId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ISBN { get; set; } = string.Empty;

        public string? Genre { get; set; }

        public string? Language { get; set; }

        public bool Availability { get; set; }

        public Guid AuthorId { get; set; }

        public string AuthorName { get; set; } = string.Empty;

        public Guid CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public bool IsSuccess { get; set; } = false;

        public string Message { get; set; } = string.Empty;
    }
}
