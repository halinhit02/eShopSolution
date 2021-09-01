using eShopSolution.Application.Catalog.Products;
using eShopSolution.ViewModels.Catalog.ProductImages;
using eShopSolution.ViewModels.Catalog.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopSolution.BackendApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IPublicProductService _publicProductService;
        private readonly IManageProductService _manageProductService;

        public ProductsController(IPublicProductService publicProductService,
            IManageProductService manageProductService)
        {
            _publicProductService = publicProductService;
            _manageProductService = manageProductService;
        }

        // http://localhost:port/products?pageIndex=1&pageSize=10
        [HttpGet("{languageId}")]
        public async Task<IActionResult> GetAllPaging(string languageId, [FromQuery] GetPublicProductPagingResquest request)
        {
            var products = await _publicProductService.GetAllByCategoryId(languageId, request);
            return Ok(products);
        }

        // http://localhost:port/product/1
        [HttpGet("{productId}/{languageId}")]
        public async Task<IActionResult> GetById(int productId, string languageId)
        {
            var product = await _manageProductService.GetById(productId, languageId);
            if (product == null)
            {
                return BadRequest("Cannot find product"); //NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var productId = await _manageProductService.Create(request);
            if (productId == 0)
            {
                return BadRequest();
            }
            var product = await _manageProductService.GetById(productId, request.LanguageId);
            //return Created(nameof(GetById), product);
            return CreatedAtAction(nameof(GetById), new { productId = productId, languageId = request.LanguageId }, product);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromForm] ProductUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var affectedResult = await _manageProductService.Update(request);
            if (affectedResult == 0)
            {
                return BadRequest();
            }
            return Ok();
        }

        [HttpPatch("{productId}/price/{newPrice}")]
        public async Task<IActionResult> UpdatePrice(int productId, decimal newPrice)
        {
            var isSuccessful = await _manageProductService.UpdatePrice(productId, newPrice);
            if (!isSuccessful)
            {
                return BadRequest();
            }
            return Ok();
        }

        [HttpPatch("{productId}/stock/{addedQuantity}")]
        public async Task<IActionResult> UpdateStock(int productId, int addedQuantity)
        {
            var isSuccessful = await _manageProductService.UpdateStock(productId, addedQuantity);
            if (!isSuccessful)
            {
                return BadRequest();
            }
            return Ok();
        }

        [HttpPatch("{productId}")]
        public async Task<IActionResult> AddViewCount(int productId)
        {
            var isSuccessful = await _manageProductService.AddViewCount(productId);
            if (!isSuccessful)
            {
                return BadRequest();
            }
            return Ok();
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> Delete(int productId)
        {
            var affectedResult = await _manageProductService.Delete(productId);
            if (affectedResult == 0)
            {
                return BadRequest();
            }
            return Ok();
        }

        //Images

        [HttpPost("{productId}/images/{imageId}")]
        public async Task<IActionResult> GetImageById(int productId, int imageId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _manageProductService.GetImageById(productId, imageId);
            if (result == null)
            {
                return BadRequest("Cannot find image");
            }
            //return Created(nameof(GetById), product);
            return Ok(result);
        }

        [HttpPost("{productId}/images")]
        public async Task<IActionResult> Create(int productId, [FromForm] ProductImageCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var imageId = await _manageProductService.AddImage(productId, request);
            if (imageId == 0)
            {
                return BadRequest();
            }
            var image = await _manageProductService.GetImageById(productId, imageId);
            //return Created(nameof(GetById), product);
            return CreatedAtAction(nameof(GetImageById), new { productId = productId, imageId = imageId }, image);
        }

        [HttpPut("{productId}/images/{imageId}")]
        public async Task<IActionResult> UpdateImage(int productId, int imageId, [FromForm] ProductImageUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _manageProductService.UpdateImage(productId, imageId, request);
            if (result == 0)
            {
                return BadRequest();
            }
            return Ok();
        }

        [HttpDelete("{productId}/images/{imageId}")]
        public async Task<IActionResult> RemoveImage(int productId, int imageId)
        {
            var isSuccessful = await _manageProductService.RemoveImage(productId, imageId);
            if (!isSuccessful)
            {
                return BadRequest();
            }
            return Ok();
        }
    }
}