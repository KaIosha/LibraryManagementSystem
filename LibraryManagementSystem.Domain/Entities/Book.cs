using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Domain.Entities
{
    public class Book
    {
        public Guid BookId { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public bool Availability { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        
        public Guid AuthorId { get; set; }
        public Author Author { get; set; } = null!;

        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<Borrow> Borrows { get; set; } = new List<Borrow>();
    }
}
