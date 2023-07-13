using Bulcky.DataAccess.Data;
using Bulcky.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace Bulcky.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _dp;
        internal DbSet<T> dpset;
        public Repository(ApplicationDbContext dp)
        {
            _dp = dp;
            this.dpset = _dp.Set<T>();
            _dp.Products.Include(u => u.Category).Include(u => u.CategoryId);
            
        }
        public void Add(T entity)
        {
            dpset.Add(entity);
        }

        public T Get (Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
        {
            IQueryable<T> query;
            if (tracked)
            {
                query = dpset;
            }
            else
            {
                query = dpset.AsNoTracking();
            }
            query = query.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var property in includeProperties.Split(new char[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
                
            }
            return query.FirstOrDefault();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter, string? includeProperties = null)
        {
            IQueryable<T> query = dpset;
            if (filter!=null)
            { 
            query = query.Where(filter);
            }
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach(var  property in includeProperties.Split(new char[] {','},
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }
            return query.ToList();
        }

        public void Remove(T entity)
        {
            dpset.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dpset.RemoveRange(entity);
        }
    }
}
