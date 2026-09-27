using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Application.Interfaces.IRepositories;

public interface IAuthorRepository : IGenericRepository<Author>
{
    Task<bool> ExistsByEmailAsync(string email);

    Task<bool> ExistsByNameAsync(string name);

    IQueryable<Author> GetQueryable();
}
