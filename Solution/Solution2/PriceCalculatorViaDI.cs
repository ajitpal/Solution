﻿
using Microsoft.Practices.Unity;
using Solution.Solution2.Rule;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solution.Solution2
{
    /// <summary>
    /// main Price calculator
    /// </summary>
    public class PriceCalculatorViaDI : ICalculatePrice
    {
        IList<IPriceDiscountRule> discountRules;
        public PriceCalculatorViaDI()
        {
            IUnityContainer container = Bootstrapper<IPriceDiscountRule>.Initialize(); //This can be done in application start or config file
            discountRules = container.ResolveAll<IPriceDiscountRule>().ToList();

            //var interfaceType = typeof(IPriceDiscountRule);
            //discountRules = AppDomain.CurrentDomain.GetAssemblies()
            //  .SelectMany(x => x.GetTypes())
            //  .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
            //  .Select(x => Activator.CreateInstance(x) as IPriceDiscountRule).ToList(); 
        }
        public decimal CalculateFinalPrice(StoreUser user, BillItems items)
        {
            decimal discount = 0m;
            foreach (IPriceDiscountRule rule in discountRules.Where(o => o.IsMandatory == false))
                discount = Math.Max(discount, CalculateDiscount(user, items, rule));
            foreach (IPriceDiscountRule rule in discountRules.Where(o => o.IsMandatory == true))
                discount += CalculateDiscount(user, items, rule);
            return items.BillTotal - discount;
        }
        private decimal CalculateDiscount(StoreUser user, BillItems items, IPriceDiscountRule rule)
        {
            decimal discount = 0m;
            if (rule.IsFixed) // When discount is fixed 
                discount = rule.GetApplicableDiscount(user, items);
            else // when discount is in percentage
            {
                decimal itemTotal = 0m;
                decimal discountRate = rule.GetApplicableDiscount(user, items);
                foreach (Product product in items.Products)
                {
                    if (product.ProductType != ProductType.Grocery) //Percentage discount doesn't apply on grocery items
                        itemTotal += (product.Quantity * product.Price);
                }
                discount = itemTotal * discountRate;
            }
            return discount;
        }
    }
}
