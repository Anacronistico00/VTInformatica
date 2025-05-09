﻿namespace VTInformatica.DTOs.Cart
{
    public class CartDto
    {
        public string? Email { get; set; }
        public ICollection<CartItemDto> Items { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}
