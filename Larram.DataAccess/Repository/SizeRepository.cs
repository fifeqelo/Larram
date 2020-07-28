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
    public class SizeRepository : Repository<Size>, ISizeRepository
    {
        private readonly ApplicationDbContext _db;
        public SizeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Size size)
        {
            var objFromDb = _db.Sizes.FirstOrDefault(s => s.Id == size.Id);
            if(objFromDb != null)
            {
                objFromDb.Name = size.Name;
                _db.SaveChanges();
            }
        }
    }
}
