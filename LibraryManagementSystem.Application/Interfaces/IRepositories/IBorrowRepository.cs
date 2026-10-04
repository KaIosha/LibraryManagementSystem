using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Application.Interfaces.IRepositories;

public interface IBorrowRepository : IGenericRepository<Borrow>
{
    Task<IEnumerable<Borrow>> GetByMemberIdAsync(string memberId, CancellationToken ct = default);

    Task<bool> ExistsActiveBorrowByBookAsync(Guid bookId, CancellationToken ct = default);

    Task<int> CountActiveByMemberAsync(string memberId, CancellationToken ct = default);

    IQueryable<Borrow> GetQueryable();
}
