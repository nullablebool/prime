﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prime.Plugins.Services.Coinbase;
using RestEase;

namespace plugins
{
    [Header("User-Agent", "RestEase")]
    internal interface IGdaxApi
    {
        [Get("/products")]
        Task<GdaxSchema.ProductsResponse> GetProducts();

        [Get("/products/{productId}/book?level={level}")]
        Task<GdaxSchema.OrderBookResponse> GetProductOrderBook([Path] string productId, [Path(Format = "D")] OrderBookDepthLevel level);

        [Get("/products/{curencyPair}/candles")]
        Task<decimal[][]> GetCandles(
            [Path] string curencyPair, 
            [Query("start", Format = "o")] DateTime? start = null, 
            [Query("end", Format = "o")] DateTime? end = null, 
            [Query("granularity")] int? granularity = null);
    }
}
