using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Larram.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        ISizeRepository Size { get; }
        IColorRepository Color { get; }
        IProductRepository Product { get; }
        IProductAvailabilityRepository ProductAvailability { get; }
        IApplicationUserRepository ApplicationUser { get; }

        Task Save();
    }
}
