using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maham.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    IQueryable<T> Query();
}
