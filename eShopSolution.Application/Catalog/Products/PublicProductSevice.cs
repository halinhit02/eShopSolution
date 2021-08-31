
using eShopSolution.Data.EF;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using eShopSolution.ViewModels.Catalog.Products;
using eShopSolution.ViewModels.Common;

namespace eShopSolution.Application.Catalog.Products
{
    public class PublicProductSevice : IPublicProductService
    {
        EShopDbContext _context;
        public PublicProductSevice(EShopDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductViewModel>> GetAll(string languageId)
        {
            var query = from product in _context.Products
                        join productTrans in _context.ProductTranslations on product.Id equals productTrans.ProductId
                        where productTrans.LanguageId == languageId
                        join productIncate in _context.ProductInCategories on product.Id equals productIncate.ProductId
                        join category in _context.Categories on productIncate.CategoryId equals category.Id
                        
                        select new { product, productTrans, productIncate };

            var data = await query.Select(x => new ProductViewModel()
                {
                    Id = x.product.Id,
                    Name = x.productTrans.Name,
                    DateCreated = x.product.DateCreated,
                    Description = x.productTrans.Description,
                    Details = x.productTrans.Details,
                    LanguageId = x.productTrans.LanguageId,
                    OriginalPrice = x.product.OriginalPrice,
                    Price = x.product.Price,
                    SeoAlias = x.productTrans.SeoAlias,
                    SeoDescription = x.productTrans.SeoDescription,
                    SeoTitle = x.productTrans.SeoTitle,
                    Stock = x.product.Stock,
                    ViewCount = x.product.ViewCount
                }).ToListAsync();
            return data;
        }

        public async Task<PagedResult<ProductViewModel>> GetAllByCategoryId(GetPublicProductPagingResquest request)
        {
            //1. Select join
            var query = from product in _context.Products
                        join productTrans in _context.ProductTranslations on product.Id equals productTrans.ProductId
                        where productTrans.LanguageId == request.languageId
                        join productIncate in _context.ProductInCategories on product.Id equals productIncate.ProductId
                        join category in _context.Categories on productIncate.CategoryId equals category.Id
                        select new { product, productTrans, productIncate };
            //2. Filter
            if (request.CategoryId.HasValue && request.CategoryId.Value > 0)
            {
                query = query.Where(p => p.productIncate.CategoryId == request.CategoryId);
            }
            //3. Paging
            int totalRow = await query.CountAsync();
            var data = await query.Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new ProductViewModel()
                {
                    Id = x.product.Id,
                    Name = x.productTrans.Name,
                    DateCreated = x.product.DateCreated,
                     Description = x.productTrans.Description,
                     Details = x.productTrans.Details,
                     LanguageId = x.productTrans.LanguageId,
                     OriginalPrice = x.product.OriginalPrice,
                     Price = x.product.Price,
                     SeoAlias = x.productTrans.SeoAlias, 
                     SeoDescription = x.productTrans.SeoDescription,
                     SeoTitle = x.productTrans.SeoTitle,
                     Stock = x.product.Stock,
                     ViewCount = x.product.ViewCount
                }).ToListAsync();
            //4. Select and Projecion
            var pagedResult = new PagedResult<ProductViewModel>()
            {
                TotalRecord = totalRow,
                Items = data
            };
            return pagedResult;
        }
    }
}
