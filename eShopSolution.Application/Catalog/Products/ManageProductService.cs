using eShopSolution.Data.EF;
using eShopSolution.Data.Entities;
using eShopSolution.Utilities.Exceptions;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using eShopSolution.ViewModels.Catalog.Products;
using eShopSolution.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.IO;
using eShopSolution.Application.Common;
using eShopSolution.ViewModels.Catalog.ProductImages;

namespace eShopSolution.Application.Catalog.Products
{
    public class ManageProductService : IManageProductService
    {
        private readonly EShopDbContext _context;
        private readonly IStorageService _storageService;

        public ManageProductService(EShopDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        public async Task<int> AddImage(int productId, ProductImageCreateRequest request)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new EShopException($"Cannot find a product with id: {productId}");

            ProductImage productImage = new ProductImage()
            {
                Caption = request.Caption,
                DateCreated = DateTime.Now,
                IsDefault = request.IsDefault,
                ProductId = productId,
                SortOrder = request.SortOrder
            };
            if (request.IsDefault)
            {
                var images = _context.ProductImages.Where(x => x.ProductId == productId);
                foreach (var image in images)
                {
                    image.IsDefault = false;
                    _context.ProductImages.Update(image);
                }
            }
            if (request.ImageFile != null)
            {
                productImage.ImagePath = await this.SaveFile(request.ImageFile);
                productImage.FileSize = request.ImageFile.Length;
            }
            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();
            return productImage.Id;
        }

        public async Task<bool> AddViewCount(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            product.ViewCount += 1;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> Create(ProductCreateRequest request)
        {
            var product = new Product()
            {
                Price = request.Price,
                OriginalPrice = request.OriginalPrice,
                Stock = request.Stock,
                ViewCount = 0,
                DateCreated = DateTime.Now,
                ProductTranslations = new List<ProductTranslation>()
                {
                    new ProductTranslation()
                    {
                        Name = request.Name,
                        Description = request.Description,
                        Details = request.Details,
                        SeoDescription = request.SeoDescription,
                        SeoAlias = request.SeoAlias,
                        SeoTitle = request.SeoTitle,
                        LanguageId = request.LanguageId,
                    }
                }
            };
            //Save Image
            if (request.ThumbnailImage != null)
            {
                product.ProductImages = new List<ProductImage>()
                {
                    new ProductImage()
                    {
                        Caption = "Thumbnail image",
                        DateCreated = DateTime.Now,
                        FileSize = request.ThumbnailImage.Length,
                        ImagePath = await this.SaveFile(request.ThumbnailImage),
                        IsDefault = true,
                        SortOrder = 1
                    }
                };
            }
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product.Id;
        }

        public async Task<int> Delete(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new EShopException($"Cannot find a product with id: {productId}");

            var images = _context.ProductImages
                    .Where(i => i.ProductId == productId);
            foreach (var image in images)
            {
                await _storageService.DeleteFileAsync(image.ImagePath);
            }
            _context.Products.Remove(product);

            return await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<ProductViewModel>> GetAllPaging(GetManageProductPagingRequest request)
        {
            //1. select join
            var query = from product in _context.Products
                        join productTrans in _context.ProductTranslations on product.Id equals productTrans.ProductId
                        join productInCate in _context.ProductInCategories on product.Id equals productInCate.ProductId
                        join category in _context.Categories on productInCate.CategoryId equals category.Id
                        select new { product, productTrans, productInCate };
            //2.filter

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => x.productTrans.Name.Contains(request.Keyword));
            }
            if (request.CategoryIds.Count > 0)
            {
                query = query.Where(p => request.CategoryIds.Contains(p.productInCate.CategoryId));
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
                    OriginalPrice = x.product.Price,
                    SeoAlias = x.productTrans.SeoAlias,
                    SeoDescription = x.productTrans.SeoDescription,
                    SeoTitle = x.productTrans.SeoTitle,
                    Stock = x.product.Stock,
                    ViewCount = x.product.ViewCount
                })
                .ToListAsync();
            //4. Select and projection
            var pagedResult = new PagedResult<ProductViewModel>()
            {
                TotalRecord = totalRow,
                Items = data
            };
            return pagedResult;
        }

        public async Task<ProductViewModel> GetById(int productId, string languageId)
        {
            var product = await _context.Products.FindAsync(productId);
            var productTrans = await _context.ProductTranslations
                .FirstOrDefaultAsync(x => x.ProductId == productId && x.LanguageId == languageId);
            var productViewModel = new ProductViewModel()
            {
                Id = product.Id,
                DateCreated = product.DateCreated,
                Price = product.Price,
                Stock = product.Stock,
                ViewCount = product.ViewCount,
                OriginalPrice = product.OriginalPrice,
                Description = productTrans != null ? productTrans.Description : "",
                Details = productTrans != null ? productTrans.Details : "",
                Name = productTrans != null ? productTrans.Name : "",
                LanguageId = productTrans != null ? productTrans.LanguageId : "",
                SeoAlias = productTrans != null ? productTrans.SeoAlias : "",
                SeoDescription = productTrans != null ? productTrans.SeoDescription : "",
                SeoTitle = productTrans != null ? productTrans.SeoTitle : "",
            };
            return productViewModel;
        }

        public async Task<ProductImageViewModel> GetImageById(int productId, int imageId)
        {
            var image = await _context.ProductImages.FirstOrDefaultAsync(x => x.ProductId == productId && x.Id == imageId);
            if (image == null) throw new EShopException($"Cannot find a image with id: {imageId}");
            return new ProductImageViewModel()
            {
                Id = image.Id,
                Caption = image.Caption,
                DateCreated = image.DateCreated,
                ProductId = image.ProductId,
                SortOrder = image.SortOrder,
                ImagePath = image.ImagePath,
                FileSize = image.FileSize,
                IsDefault = image.IsDefault
            };
        }

        public async Task<List<ProductImageViewModel>> GetListImages(int productId)
        {
            var images = _context.ProductImages.Where(x => x.ProductId == productId);
            if (images == null) throw new EShopException($"Cannot find a image with productId: {productId}");
            return await images.Select(x => new ProductImageViewModel()
            {
                Id = x.Id,
                Caption = x.Caption,
                DateCreated = x.DateCreated,
                ProductId = x.ProductId,
                SortOrder = x.SortOrder,
                ImagePath = x.ImagePath,
                FileSize = x.FileSize,
                IsDefault = x.IsDefault
            }).ToListAsync();
        }

        public async Task<bool> RemoveImage(int productId, int imageId)
        {
            var productImage = await _context.ProductImages.FindAsync(imageId);
            if (productImage == null) throw new EShopException($"Cannot find a image with id: {imageId}");
            _context.ProductImages.Remove(productImage);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> Update(ProductUpdateRequest request)
        {
            var product = await _context.Products.FindAsync(request.Id);
            var productTrans = await _context.ProductTranslations
                .FirstOrDefaultAsync(x => x.ProductId == request.Id && x.LanguageId == request.LanguageId);
            if (product == null || productTrans == null)
            {
                throw new EShopException($"Cannot find a product with id: {request.Id}");
            }
            productTrans.Name = request.Name;
            productTrans.SeoAlias = request.SeoAlias;
            productTrans.SeoDescription = request.SeoDescription;
            productTrans.SeoTitle = request.SeoTitle;
            productTrans.Description = request.Description;
            productTrans.Details = request.Details;
            _context.ProductTranslations.Update(productTrans);

            //Update Image
            if (request.ThumbnailImage != null)
            {
                var thumbnailImage = await _context.ProductImages
                    .FirstOrDefaultAsync(i => i.IsDefault == true && i.ProductId == request.Id);
                if (thumbnailImage != null)
                {
                    thumbnailImage.FileSize = request.ThumbnailImage.Length;
                    thumbnailImage.ImagePath = await this.SaveFile(request.ThumbnailImage);
                    _context.ProductImages.Update(thumbnailImage);
                }
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateImage(int productId, int imageId, ProductImageUpdateRequest request)
        {
            ProductImage image = await _context.ProductImages.FindAsync(imageId);
            if (image == null)
            {
                throw new EShopException($"Cannot find a image with id: {imageId}");
            }
            image.Caption = request.Caption;
            image.IsDefault = request.IsDefault;
            image.SortOrder = request.SortOrder;

            if (request.IsDefault)
            {
                var productImages = _context.ProductImages.Where(x => x.ProductId == image.ProductId && x.Id != imageId);
                foreach (var productImage in productImages)
                {
                    productImage.IsDefault = false;
                    _context.ProductImages.Update(productImage);
                }
            }

            if (request.ImageFile != null)
            {
                image.ImagePath = await this.SaveFile(request.ImageFile);
                image.FileSize = request.ImageFile.Length;
            }

            _context.ProductImages.Update(image);
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdatePrice(int productId, decimal newPrice)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                throw new EShopException($"Cannot find a product with id: {productId}");
            }
            product.Price = newPrice;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateStock(int productId, int addedQuantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                throw new EShopException($"Cannot find a product with id: {productId}");
            }
            product.Stock += addedQuantity;
            return await _context.SaveChangesAsync() > 0;
        }

        //private
        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            return fileName;
        }
    }
}