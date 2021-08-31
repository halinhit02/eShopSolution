using eShopSolution.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShopSolution.ViewModels.Catalog.Products
{
    public class GetPublicProductPagingResquest : PagingRequestBase
    {
        public int? CategoryId { get; set; }
    }
}
