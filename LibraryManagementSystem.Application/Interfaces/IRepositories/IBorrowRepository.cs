using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Application.Interfaces.IRepositories;

public interface IBorrowRepository : IGenericRepository<Borrow>
{
    Task<IEnumerable<Borrow>> GetByMemberIdAsync(string memberId, CancellationToken ct = default);
}
