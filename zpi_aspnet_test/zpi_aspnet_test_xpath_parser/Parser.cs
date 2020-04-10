﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using zpi_aspnet_test.Extensions;
using HtmlAgilityPack;
using zpi_aspnet_test.Enumerators;
using zpi_aspnet_test.Models;

namespace zpi_aspnet_test_xpath_parser
{
   public class Parser
   {
      public List<StateOfAmericaModel> GetStatesModelsFromWikipedia()
      {
         var web = new HtmlWeb();
         var html = web.Load(@"https://en.wikipedia.org/wiki/Sales_taxes_in_the_United_States");
         var node = html.DocumentNode.SelectNodes("//table")
            .FirstOrDefault(htmlNode => htmlNode.Attributes["class"].Value == "wikitable sortable");
         var nodes = node?.SelectSingleNode("./tbody").ChildNodes
            .Where(htmlNode => htmlNode.ChildNodes.Any(n => n.Name == "td"));
         var states = new List<StateOfAmericaModel>();

         var dict = new Dictionary<int, ProductCategoryEnum>
         {
            {7, ProductCategoryEnum.Groceries},
            {9, ProductCategoryEnum.PreparedFood},
            {11, ProductCategoryEnum.PrescriptionDrug},
            {13, ProductCategoryEnum.NonPrescriptionDrug},
            {15, ProductCategoryEnum.Clothing},
            {17, ProductCategoryEnum.Intangibles}
         };

         foreach (var n in nodes ?? new HtmlNode[1])
         {
            var state = new StateOfAmericaModel {Rates = new Dictionary<ProductCategoryEnum, double>()};
            for (var index = 1; index < n.ChildNodes.Count; index += 2)
            {
               var childNode = n.ChildNodes[index];
               switch (index)
               {
                  case 1:
                     state.Name = childNode.InnerText.Trim();
                     break;
                  case 3:
                     var percent = childNode.InnerText.Trim();
                     state.BaseSalesTax = double.Parse(percent.Replace('.', ',').Substring(0, percent.Length - 1)) /
                                          100.0;
                     break;
                  case 5:
                     var totalPercent = childNode.InnerText.Trim();
                     state.TotalTax =
                        double.Parse(totalPercent.Replace('.', ',').Substring(0, totalPercent.Length - 1)) / 100.0;
                     break;
                  case 7:
                  case 9:
                  case 11:
                  case 13:
                  case 15:
                  case 17:
                     var type = dict[index];
                     var style = childNode.Attributes["style"].Value;
                     var kV = ParseStyle(style);
                     var innerText = childNode.InnerText.Trim();
                     var background = kV.GetValueOrDefault("background", "");
                     var foreground = kV.GetValueOrDefault("color", "black");
                     switch (background)
                     {
                        case "#f62b0f":
                           state.Rates.Add(type, 0.0);
                           break;
                        case "#4ee04e":
                           state.Rates.Add(type, 0.0);
                           if (innerText.Length > 0)
                           {
                              var id = innerText.IndexOf('$') + 1;
                              innerText = innerText.Substring(id > 0 ? id : 0, id > 0 ? innerText.Length - id : 0);
                           }

                           var condition = innerText.Length > 0
                              ? double.Parse(innerText.Trim().Replace('.', ','))
                              : 0;
                           Console.WriteLine(condition > 0
                              ? $"Amount of money should be less than ${condition}"
                              : "There is no conditions for not exempting product from sales tax");
                           break;
                        case "#7788ff":
                           if (innerText.Length > 0)
                           {
                              var idx = innerText.IndexOf('%');
                              innerText = innerText.Substring(0, idx < 0 ? 0 : idx);
                           }

                           var taxRate = innerText.Length == 0
                              ? state.BaseSalesTax
                              : double.Parse(innerText.Trim().Replace('.', ',')) / 100.0;
                           state.Rates.Add(type, taxRate);
                           break;
                        default:
                           break;
                     }

                     break;
               }
            }

            states.Add(state);
         }

         return states;
      }

      public static Dictionary<string, string> ParseStyle(string cssString)
      {
         var dict = new Dictionary<string, string>();
         var strings = cssString.Split(' ');
         foreach (var s in strings)
         {
            var arr = s.Split(':');
            if (arr[1].Contains(';'))
            {
               arr[1] = arr[1].Substring(0, arr[1].Length - 1);
            }

            dict.Add(arr[0], arr[1]);
         }

         return dict;
      }

   }
}
