﻿using TeaAPI.Dtos.Products;

namespace TeaAPI.Models.Requests.Products
{
    public class CreateProductRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int CategoryId { get; set; }

        public IEnumerable<ProductSizeDTO> ProductSizes { get; set; }
        public IEnumerable<int> Options { get; set; }
    }
}
