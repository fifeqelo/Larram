using Larram.DataAccess.Data;
using Larram.DataAccess.Repository.IRepository;
using Larram.Models;
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
            Size = new SizeRepository(_db);
            Color = new ColorRepository(_db);
            Product = new ProductRepository(_db);
            ProductAvailability = new ProductAvailabilityRepository(_db);
        }

        public ICategoryRepository Category {get; private set;}
        public ISizeRepository Size { get; private set; }
        public IColorRepository Color { get; private set; }
        public IProductRepository Product { get; private set; }
        public IProductAvailabilityRepository ProductAvailability { get; private set; }

        public async Task Save()
        {
            await _db.SaveChangesAsync();
        }
    }
}
