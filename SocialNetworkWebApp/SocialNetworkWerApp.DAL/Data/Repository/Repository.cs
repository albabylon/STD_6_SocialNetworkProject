using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialNetworkWebApp.Data.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected DbContext _context;

        public DbSet<T> Set => _context.Set<T>();

        public Repository(ApplicationDbContext db)
        {
            _context = db;
        }

        public async Task Create(T item)
        {
            Set.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(T item)
        {
            Set.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task<T> Get(int id)
        {
            var result = await Set.FindAsync(id);

            return result ?? throw new Exception($"{id} не найден");
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await Set.ToListAsync();
        }

        public async Task Update(T item)
        {
            Set.Update(item);
            await _context.SaveChangesAsync();
        }
    }
}
