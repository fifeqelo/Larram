using Larram.DataAccess.Data;
using Larram.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Larram.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
        }

        public ICategoryRepository Category {get; private set;}

        public async Task Save()
        {
            await _db.SaveChangesAsync();
        }
    }
}
