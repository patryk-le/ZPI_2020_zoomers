﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zpi_aspnet_test.Models;

namespace zpi_aspnet_test_algorithms_library
{
    class Algorithms
    {
        public static void CalculateFinalPrice(ProductModel product, StateOfAmericaModel state)
        {
            var tax = state.Rates[product.Category];
            product.FinalPrice = product.PreferredPrice - (product.PreferredPrice * tax);
        }

        public static void SetFinalPrices(ProductModel product, List<StateOfAmericaModel> states)
        {
            foreach (var state in states)
            {
                CalculateFinalPrice(product, state);
            }
        }

        public static double CalculateMargin(ProductModel product)
        {
            return product.FinalPrice - product.PurchasePrice;
        }

        public static void ChangeMargin(ProductModel product, double margin)
        {
            product.FinalPrice = margin + product.PurchasePrice;
        }
    }
}