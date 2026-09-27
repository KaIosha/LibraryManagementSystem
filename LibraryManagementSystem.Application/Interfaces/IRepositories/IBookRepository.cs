using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Application.Interfaces.IRepositories;

public interface IBookRepository : IGenericRepository<Book>
{
    Task<IEnumerable<Book>> GetBooksByAuthorAsync(string author, CancellationToken ct = default);

    Task<bool> ExistsByCategoryAsync(Guid categoryId, CancellationToken ct = default);

    Task<Dictionary<Guid, int>> CountGroupedByCategoryAsync(CancellationToken ct = default);

    Task<bool> ExistsByAuthorAsync(Guid authorId, CancellationToken ct = default);

    Task<Dictionary<Guid, int>> CountGroupedByAuthorAsync(CancellationToken ct = default);

    Task<(IEnumerable<Book> Items, int TotalCount)> GetPagedByCategoryAsync(Guid categoryId, string? searchTerm, int skip, int take, CancellationToken ct = default);

    IQueryable<Book> GetQueryable();
}
