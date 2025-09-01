using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Entities.Interfaces;

namespace Core
{
    /// <summary>
    /// Generic CRUD operasyonları için temel servis sınıfı
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    public class CrudServiceBase<T> : ICrudService<T> where T : class, IEntity
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public CrudServiceBase(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

       
        public virtual DbContext GetContext()
        {
            return _context;
        }

       
        public virtual async Task<T> CreateAsync(T entity)
        {
            if (entity is ITrackable trackable)
            {
                trackable.CreatedAt = DateTime.UtcNow;
                trackable.UpdatedAt = DateTime.UtcNow;
                trackable.IsActive = true;
            }

            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// ID'ye göre entity getirir
        /// </summary>
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Belirtilen filtreye göre entity getirir
        /// </summary>
        public virtual async Task<T?> GetAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.FirstOrDefaultAsync(filter);
        }

        /// <summary>
        /// Tüm entity'leri getirir
        /// </summary>
        public virtual async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Belirtilen filtreye göre entity'leri getirir
        /// </summary>
        public virtual async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.Where(filter).ToListAsync();
        }

        /// <summary>
        /// Belirtilen filtreye göre ve ilişkili entity'leri dahil ederek entity'leri getirir
        /// </summary>
        public virtual async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter, string includeProperties)
        {
            IQueryable<T> query = _dbSet.Where(filter);
            
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }
            
            return await query.ToListAsync();
        }

        /// <summary>
        /// Entity'yi günceller
        /// </summary>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            if (entity is ITrackable trackable)
            {
                trackable.UpdatedAt = DateTime.UtcNow;
            }

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Entity'yi siler (hard delete)
        /// </summary>
        public virtual async Task<bool> DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// ID'ye göre entity'yi siler (hard delete)
        /// </summary>
        public virtual async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            return await DeleteAsync(entity);
        }

        /// <summary>
        /// Entity'yi soft delete yapar (IsActive = false)
        /// </summary>
        public virtual async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;

            if (entity is ITrackable trackable)
            {
                trackable.IsActive = false;
                trackable.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(entity);
                return true;
            }

            // Entity ITrackable değilse normal silme işlemi yap
            return await DeleteAsync(entity);
        }

        /// <summary>
        /// Entity sayısını döndürür
        /// </summary>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
        {
            return filter == null 
                ? await _dbSet.CountAsync() 
                : await _dbSet.CountAsync(filter);
        }

        /// <summary>
        /// Entity'nin var olup olmadığını kontrol eder
        /// </summary>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.AnyAsync(filter);
        }
    }
}