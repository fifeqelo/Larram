using Larram.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Larram.DataAccess.Repository.IRepository
{
    public interface IColorRepository : IRepository<Color>
    {
        void Update(Color color);
    }
}
