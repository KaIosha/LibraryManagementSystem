namespace LibraryManagementSystem.Application.DTOs.CategoryDtos
{
    public class BookDataDto
    {
        public Guid BookId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ISBN { get; set; } = string.Empty;

        public bool Availability { get; set; }
    }
}
