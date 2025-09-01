using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Entities.Interfaces;

namespace Core
{
    
    public interface ICrudService<T> where T : class, IEntity
    {
        
        Microsoft.EntityFrameworkCore.DbContext GetContext();

       
        Task<T> CreateAsync(T entity);

        
        Task<T?> GetByIdAsync(int id);

      
        /// <param name="filter">Lambda expression filtresi</param>
        
        Task<T?> GetAsync(Expression<Func<T, bool>> filter);

       
        Task<List<T>> GetAllAsync();

        /// <summary>
        /// Belirtilen filtreye göre entity'leri getirir
        /// </summary>
        /// <param name="filter">Lambda expression filtresi</param>
        /// <returns>Filtrelenmiş entity listesi</returns>
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter);

        /// <summary>
        /// Belirtilen filtreye göre ve ilişkili entity'leri dahil ederek entity'leri getirir
        /// </summary>
        /// <param name="filter">Lambda expression filtresi</param>
        /// <param name="includeProperties">Dahil edilecek ilişkili entity'ler (virgülle ayrılmış)</param>
        /// <returns>Filtrelenmiş entity listesi</returns>
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter, string includeProperties);

        /// <summary>
        /// Entity'yi günceller
        /// </summary>
        /// <param name="entity">Güncellenecek entity</param>
        /// <returns>Güncellenen entity</returns>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Entity'yi siler (hard delete)
        /// </summary>
        /// <param name="entity">Silinecek entity</param>
        /// <returns>İşlem başarılı ise true</returns>
        Task<bool> DeleteAsync(T entity);

        /// <summary>
        /// ID'ye göre entity'yi siler (hard delete)
        /// </summary>
        /// <param name="id">Silinecek entity ID</param>
        /// <returns>İşlem başarılı ise true</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Entity'yi soft delete yapar (IsActive = false)
        /// </summary>
        /// <param name="id">Soft delete yapılacak entity ID</param>
        /// <returns>İşlem başarılı ise true</returns>
        Task<bool> SoftDeleteAsync(int id);

        /// <summary>
        /// Entity sayısını döndürür
        /// </summary>
        /// <param name="filter">Opsiyonel filtre</param>
        /// <returns>Entity sayısı</returns>
        Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);

        /// <summary>
        /// Entity'nin var olup olmadığını kontrol eder
        /// </summary>
        /// <param name="filter">Kontrol edilecek koşul</param>
        /// <returns>Varsa true, yoksa false</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
    }
}