using HRWebApp.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using HRWebApp.Data;



namespace HRWebApp.Concrete
{

    public class EntitiesRepository<T> : IEntitiesRepository<T> where T : class, new()
    {
        protected IApplicationDbContext _context;
        protected DbSet<T> _table = null;
        private bool _disposed = false;
        public EntitiesRepository(IApplicationDbContext context)
        {
            _context = context;
            _table = _context.Set<T>();
        }
        protected EntitiesRepository(ApplicationDbContext _contex)
        {
            _context = _contex;
            _table = _contex.Set<T>();
        }

        public int Count => _table.Count();

        public virtual void Add(T entity)
        {
            _table.Add(entity);
            SaveChanges();
        }

        public virtual void AddRange(IEnumerable<T> entities)
        {
            _table.AddRange(entities);
            SaveChanges();
        }

        public virtual void Remove(T entity)
        {
            _table.Remove(entity);
            SaveChanges();
        }

        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            _table.RemoveRange(entities);
            SaveChanges();
        }

        public T Get(int? id) => _table.Find(id);
        public T Get(long? idOne, long? idTwo) => _table.Find(idOne, idTwo);
        public T Get(string id) => _table.Find(id);

        public T Get(Expression<Func<T, bool>> where)
        => _table.FirstOrDefault(where);

        public T Get<TIncludeField>(Expression<Func<T, bool>> where, Expression<Func<T, ICollection<TIncludeField>>> include)
            => _table.Where(where).Include(include).FirstOrDefault();

        public T Get<TIncludeField>(Expression<Func<T, bool>> where, Expression<Func<T, TIncludeField>> include)
            => _table.Where(where).Include(include).FirstOrDefault();

        public virtual IQueryable<T> GetAll() => _table;

        public virtual IQueryable<T> GetAllQueryable() => _table;

        public IQueryable<T> GetAll<TIncludeField>(Expression<Func<T, ICollection<TIncludeField>>> include)
            => _table.Include(include);

        public IQueryable<T> GetAll<TIncludeField>(Expression<Func<T, TIncludeField>> include)
            => _table.Include(include);

        public IQueryable<T> GetAll<TSortField>(Expression<Func<T, TSortField>> orderBy, bool ascending)
            => ascending ? _table.OrderBy(orderBy) : _table.OrderByDescending(orderBy);

        public IQueryable<T> GetAll<TIncludeField, TSortField>(
            Expression<Func<T, ICollection<TIncludeField>>> include, Expression<Func<T, TSortField>> orderBy, bool ascending)
            => ascending ? _table.Include(include).OrderBy(orderBy) : _table.Include(include).OrderByDescending(orderBy);

        public IQueryable<T> GetAll<TIncludeField, TSortField>(
            Expression<Func<T, TIncludeField>> include, Expression<Func<T, TSortField>> orderBy, bool ascending)
            => ascending ? _table.Include(include).OrderBy(orderBy) : _table.Include(include).OrderByDescending(orderBy);

        public IQueryable<T> GetSome(Expression<Func<T, bool>> where) => _table.Where(where);

        public IQueryable<T> GetSome<TIncludeField>(Expression<Func<T, bool>> where, Expression<Func<T, ICollection<TIncludeField>>> include)
            => _table.Where(where).Include(include);

        public IQueryable<T> GetSome<TIncludeField>(Expression<Func<T, bool>> where, Expression<Func<T, TIncludeField>> include)
            => _table.Where(where).Include(include);

        public IQueryable<T> GetSome<TSortField>(
            Expression<Func<T, bool>> where, Expression<Func<T, TSortField>> orderBy, bool ascending)
            => ascending ? _table.Where(where).OrderBy(orderBy) : _table.Where(where).OrderByDescending(orderBy);

        public IQueryable<T> GetSome<TIncludeField, TSortField>(
            Expression<Func<T, bool>> where, Expression<Func<T, ICollection<TIncludeField>>> include,
            Expression<Func<T, TSortField>> orderBy, bool ascending)
            => ascending ?
            _table.Where(where).OrderBy(orderBy).Include(include) :
            _table.Where(where).OrderByDescending(orderBy).Include(include);

        public IQueryable<T> GetSome<TIncludeField, TSortField>(
            Expression<Func<T, bool>> where, Expression<Func<T, TIncludeField>> include,
            Expression<Func<T, TSortField>> orderBy, bool ascending)
            => ascending ?
            _table.Where(where).OrderBy(orderBy).Include(include) :
            _table.Where(where).OrderByDescending(orderBy).Include(include);

        public int SaveChanges()
        {
            var saved = false;
            var attempt = 0;
            int result = 0;
            while (!saved && attempt <= 3)
            {
                try
                {
                    // Attempt to save changes to the database
                    result = _context.SaveChanges();
                    saved = true;
                }
                catch (Exception ex)
                {
                    //Should handle intelligently
                    attempt++;
                    throw;
                }
            }
            return result;
        }
        public virtual void Update(T entity)
        {
            _table.Update(entity);
            SaveChanges();
        }

        public T Get(long? id)
        {
            throw new NotImplementedException();
        }

        public Task GetByCondition(Func<object, bool> value)
        {
            throw new NotImplementedException();
        }

        //  Async Get methods
        public async Task<T> GetAsync(int? id) => await _table.FindAsync(id);
        public async Task<T> GetAsync(long? id) => await _table.FindAsync(id);
        public async Task<T> GetAsync(string id) => await _table.FindAsync(id);
        public async Task<T> GetAsync(Expression<Func<T, bool>> predicate) => await _table.FirstOrDefaultAsync(predicate);
        public async Task<List<T>> GetAllAsync() => await _table.ToListAsync();
        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate) => await _table.Where(predicate).ToListAsync();


        public async Task AddAsync(T entity)
        {
            await _table.AddAsync(entity);
            await SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _table.AddRangeAsync(entities);
            await SaveChangesAsync();
        }
        public async Task RemoveAsync(T entity)
        {
            _table.Remove(entity);
            await SaveChangesAsync();
        }

        public async Task RemoveRangeAsync(IEnumerable<T> entities)
        {
            _table.RemoveRange(entities);
            await SaveChangesAsync();
        }
        public async Task<int> SaveChangesAsync()
        {
            var saved = false;
            var attempt = 0;
            int result = 0;
            while (!saved && attempt <= 3)
            {
                try
                {
                    result = await _context.SaveChangesAsync();
                    saved = true;
                }
                catch (Exception)
                {
                    attempt++;
                    throw;
                }
            }
            return result;
        }
        public async Task<int> CountAsync()
        {
            return await _table.CountAsync();
        }

        public async Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _table;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }


        // For review , since we want only some entities to update 
        public virtual void Update(T existingEntity, T newValues)
        {
            _context.Entry(existingEntity).CurrentValues.SetValues(newValues);
            SaveChanges();
        }

    }
}
