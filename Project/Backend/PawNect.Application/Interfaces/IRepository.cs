using System.Linq.Expressions;
using PawNect.Domain.Entities;

namespace PawNect.Application.Interfaces;

/// <summary>
/// Generic repository interface for CRUD operations
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task SaveChangesAsync();
}
