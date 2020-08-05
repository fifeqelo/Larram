using Larram.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Larram.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        void Update(ShoppingCart shoppingCart);
    }
}
