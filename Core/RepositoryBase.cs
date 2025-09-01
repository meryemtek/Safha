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
    /// Generic repository implementasyonu
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    public class RepositoryBase<T> : IRepository<T> where T : class, IEntity
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public RepositoryBase(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// Yeni bir entity ekler
        /// </summary>
        public virtual async Task<T> AddAsync(T entity)
        {
            if (entity is ITrackable trackable)
            {
                trackable.CreatedAt = DateTime.UtcNow;
                trackable.UpdatedAt = DateTime.UtcNow;
                trackable.IsActive = true;
            }

            await _dbSet.AddAsync(entity);
            await SaveChangesAsync();
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
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Belirtilen filtreye göre entity'leri getirir
        /// </summary>
        public virtual async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.Where(filter).ToListAsync();
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
            await SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Entity'yi siler (hard delete)
        /// </summary>
        public virtual async Task<bool> RemoveAsync(T entity)
        {
            _dbSet.Remove(entity);
            return await SaveChangesAsync() > 0;
        }

        /// <summary>
        /// ID'ye göre entity'yi siler (hard delete)
        /// </summary>
        public virtual async Task<bool> RemoveAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            return await RemoveAsync(entity);
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
            return await RemoveAsync(entity);
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

        /// <summary>
        /// Değişiklikleri kaydeder
        /// </summary>
        public virtual async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}