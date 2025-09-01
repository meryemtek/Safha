using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Entities.Interfaces;

namespace Core
{
   
    public interface IRepository<T> where T : class, IEntity
    {
       
        Task<T> AddAsync(T entity);

       
        Task<T?> GetByIdAsync(int id);

       
        Task<T?> GetAsync(Expression<Func<T, bool>> filter);

       
        Task<IEnumerable<T>> GetAllAsync();

       
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter);

        
        Task<T> UpdateAsync(T entity);

       
        Task<bool> RemoveAsync(T entity);

      
        Task<bool> RemoveAsync(int id);

        
        Task<bool> SoftDeleteAsync(int id);

        Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);

        
        Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);

        
        Task<int> SaveChangesAsync();
    }
}