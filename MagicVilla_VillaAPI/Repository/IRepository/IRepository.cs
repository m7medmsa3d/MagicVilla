using MagicVilla_VillaAPI.Models;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> GetAllAsync(
          Expression<Func<T, bool>>? filter = null,
          string? includeproperties = null,
          int pagesize = 0,
          int pagenumber = 1);
        Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true, string? includeproperties = null);
        Task CreateAsync(T entity);
     
        Task RemoveAsync(T entity);
        Task SaveAsync();
    }
}
