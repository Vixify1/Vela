using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HRWebApp.Abstract
{

    /// <summary>
    /// Generic repository interface for all entities
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEntitiesRepository<T> where T : class
    {
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        void Update(T entity);
        int SaveChanges();
        T Get(int? id);
        T Get(long? id);
        T Get(long? idOne, long? idTwo);
        T Get(string id);
        T Get(Expression<Func<T, bool>> where);
        T Get<TIncludeField>(Expression<Func<T, bool>> where,
            Expression<Func<T, ICollection<TIncludeField>>> include);
        T Get<TIncludeField>(Expression<Func<T, bool>> where,
            Expression<Func<T, TIncludeField>> include);
        IQueryable<T> GetAll();
        IQueryable<T> GetAllQueryable();
        IQueryable<T> GetAll<TIncludeField>(Expression<Func<T, ICollection<TIncludeField>>> include);
        IQueryable<T> GetAll<TIncludeField>(Expression<Func<T, TIncludeField>> include);
        IQueryable<T> GetAll<TSortField>(Expression<Func<T, TSortField>> orderBy, bool ascending);
        IQueryable<T> GetAll<TIncludeField, TSortField>(
            Expression<Func<T, ICollection<TIncludeField>>> include,
            Expression<Func<T, TSortField>> orderBy, bool ascending);
        IQueryable<T> GetAll<TIncludeField, TSortField>(
            Expression<Func<T, TIncludeField>> include,
            Expression<Func<T, TSortField>> orderBy, bool ascending);

        IQueryable<T> GetSome<TIncludeField>(Expression<Func<T, bool>> where,
            Expression<Func<T, ICollection<TIncludeField>>> include);
        IQueryable<T> GetSome<TIncludeField>(Expression<Func<T, bool>> where,
            Expression<Func<T, TIncludeField>> include);
        IQueryable<T> GetSome<TSortField>(
            Expression<Func<T, bool>> where, Expression<Func<T, TSortField>> orderBy, bool ascending);
        IQueryable<T> GetSome<TIncludeField, TSortField>(
            Expression<Func<T, bool>> where, Expression<Func<T, ICollection<TIncludeField>>> include,
            Expression<Func<T, TSortField>> orderBy, bool ascending);
        IQueryable<T> GetSome<TIncludeField, TSortField>(
            Expression<Func<T, bool>> where, Expression<Func<T, TIncludeField>> include,
            Expression<Func<T, TSortField>> orderBy, bool ascending);
        Task GetByCondition(Func<object, bool> value);
        // TTarget Map<TSource, TTarget>(TSource source);


        // Async CRUD Support
        Task<T> GetAsync(int? id);
        Task<T> GetAsync(long? id);
        Task<T> GetAsync(string id);
        Task<T> GetAsync(Expression<Func<T, bool>> predicate);
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);

        Task RemoveAsync(T entity);
        Task RemoveRangeAsync(IEnumerable<T> entities);

        Task<int> SaveChangesAsync();
        Task<int> CountAsync();
        Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes);

        // Review

        void Update(T existingEntity, T newValues);

    }
}
