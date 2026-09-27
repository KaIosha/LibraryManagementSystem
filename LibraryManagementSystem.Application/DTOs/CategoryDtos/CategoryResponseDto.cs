namespace LibraryManagementSystem.Application.DTOs.CategoryDtos
{
    public class CategoryResponseDto
    {
        public Guid CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int BooksCount { get; set; }

        public bool IsSuccess { get; set; } = false;

        public string Message { get; set; } = string.Empty;
    }
}
