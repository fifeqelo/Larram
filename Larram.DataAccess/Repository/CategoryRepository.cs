using Larram.DataAccess.Data;
using Larram.DataAccess.Repository.IRepository;
using Larram.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Larram.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Category category)
        {
            var objFromDb = _db.Categories.FirstOrDefault(s => s.Id == category.Id);
            if(objFromDb != null)
            {
                objFromDb.Name = category.Name;
                _db.SaveChanges();
            }
        }
    }
}
