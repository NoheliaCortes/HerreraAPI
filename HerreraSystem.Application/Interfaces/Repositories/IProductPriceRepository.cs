using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IProductPriceRepository
    {
        // Retorna el precio vigente de tipo "Detalle" para un producto
        Task<decimal?> GetActivePriceAsync(int productId, string priceTypeName);
    }
}
