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
    public class ColorRepository : Repository<Color>, IColorRepository
    {
        private readonly ApplicationDbContext _db;
        public ColorRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Color Color)
        {
            var objFromDb = _db.Colors.FirstOrDefault(s => s.Id == Color.Id);
            if(objFromDb != null)
            {
                objFromDb.Name = Color.Name;
                objFromDb.HexValue = Color.HexValue;
                _db.SaveChanges();
            }
        }
    }
}
