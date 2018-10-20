// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using SampleSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
    [Title("LINQ Module")]
    [Prefix("Linq")]
    public class LinqSamples : SampleHarness
    {
        private DataSource dataSource = new DataSource();

        [Category("Restriction Operators(example)")]
        [Title("Where - Task 1(example)")]
        [Description("This sample uses the where clause to find all elements of an array with a value less than 5.")]
        public void Linq1()
        {
            int[] numbers = { 5, 4, 1, 3, 9, 8, 6, 7, 2, 0 };

            var lowNums =
                from num in numbers
                where num < 5
                select num;

            Console.WriteLine("Numbers < 5:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x);
            }
        }

        [Category("Restriction Operators(example)")]
        [Title("Where - Task 2(example)")]
        [Description("This sample return return all presented in market products")]
        public void Linq2()
        {
            var products =
                from p in dataSource.Products
                where p.UnitsInStock > 0
                select p;

            foreach (var p in products)
            {
                ObjectDumper.Write(p);
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 1")]
        [Description("This sample return list of customers with the sum of the orders greate then 'x'.")]
        public void Linq001()
        {
            int x = 1000;

            var customers = dataSource.Customers
            .Where(c => c.Orders.Select(o => o.Total).Sum() > x)
            .Select(c => new
            {
                CustomerId = c.CustomerID,
                TotalSum = c.Orders.Select(o => o.Total).Sum()
            });

            ObjectDumper.Write($"List of customers with the sum of the orders greate then {x}:");
            foreach (var c in customers)
            {
                ObjectDumper.Write($"Customer id: {c.CustomerId} Total sum of orders: {c.TotalSum}.");
            }

            x = 10000;
            ObjectDumper.Write($"List of customers with the sum of the orders greate then {x}:");
            foreach (var c in customers)
            {
                ObjectDumper.Write($"Customer id: {c.CustomerId} Total sum of orders: {c.TotalSum}.");
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 2")]
        [Description("This sample return list of customers with list of suppliers who are from the same country and town.")]
        public void Linq002()
        {
            ObjectDumper.Write($"Using grouping:");

            var customers = dataSource.Customers.Join(
                dataSource.Suppliers,
                c => new { c.Country, c.City },
                s => new { s.Country, s.City },
                (c, s) => new
                {
                    CustomerInfo = $"{c.CustomerID}; Country {c.Country}, City {c.City}",
                    SuppliersInfo = $"{s.SupplierName}; Country {s.Country}; City {s.City}",
                }).GroupBy(x => x.CustomerInfo).ToDictionary(
                g => g.Key,
                g => g.Select(x => x.SuppliersInfo)
                );

            foreach (var c in customers)
            {
                ObjectDumper.Write($"Customer {c.Key}");
                ObjectDumper.Write("List of suppliers from the same country and city:");
                foreach (var v in c.Value)
                {
                    ObjectDumper.Write(v);
                }
            }

            ObjectDumper.Write($"Without grouping:");

            var customersWithoutGrouping = dataSource.Customers.Select(c => new
            {
                CustomerInfo = $"{c.CustomerID}; Country {c.Country}, City {c.City}",
                SuppliersList = dataSource.Suppliers.Where(s => s.Country == c.Country && s.City == c.City)
            }).Where(c => c.SuppliersList.Count() > 0);

            foreach (var c in customersWithoutGrouping)
            {
                ObjectDumper.Write($"Customer {c.CustomerInfo}");
                ObjectDumper.Write("List of suppliers from the same country and city:");
                foreach (var s in c.SuppliersList)
                {
                    ObjectDumper.Write($"{s.SupplierName}; Country {s.Country}; City {s.City}");
                }
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 3")]
        [Description("This sample return list of customers with orders bigger than x.")]
        public void Linq003()
        {
            int x = 8000;
            var result = dataSource.Customers.Where(c => c.Orders.Any(o => o.Total > x));

            foreach (var r in result)
            {
                ObjectDumper.Write($"{r.CustomerID}. List of orders:");
                foreach (var o in r.Orders)
                {
                    ObjectDumper.Write($"{o.OrderID}, Total: {o.Total}");
                }
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 4")]
        [Description("This sample return list of customers with date of registration.")]
        public void Linq004()
        {
            var customers = dataSource.Customers.Where(x => x.Orders.Count() > 0).Select(c => new
            {
                Customer = c.CustomerID,
                Date = c.Orders.Select(x => x.OrderDate).Min()
            });

            foreach (var c in customers)
            {
                ObjectDumper.Write($"Customer:{c.Customer}; Date of registration: Month {c.Date.Month} Year {c.Date.Year}");
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 5")]
        [Description("This sample return sorted list of customers with date of registration.")]
        public void Linq005()
        {
            var customers = dataSource.Customers.Where(x => x.Orders.Count() > 0).Select(c => new
            {
                Customer = c.CustomerID,
                Sum = c.Orders.Select(o => o.Total).Sum(),
                Date = c.Orders.Select(x => x.OrderDate).Min()
            }).OrderByDescending(c => c.Date.Year).ThenByDescending(c => c.Date.Month).ThenByDescending(c => c.Sum).ThenBy(c => c.Customer);

            foreach (var c in customers)
            {
                ObjectDumper.Write($"Date of registration: Year {c.Date.Year} Month {c.Date.Month}; Total sum {c.Sum}; Customer:{c.Customer};");
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 6")]
        [Description("This sample return sorted list of customers with non digital post code or without region or phone doesn't have an operator code.")]
        public void Linq006()
        {
            long postcode;
            var phoneCodeRegex = new Regex(@"\(\d+\).*", RegexOptions.Compiled);

            var customers = dataSource.Customers.Where(x => String.IsNullOrEmpty(x.Region)
            || !long.TryParse(x.PostalCode, out postcode)
            || !phoneCodeRegex.Match(x.Phone).Success);

            ObjectDumper.Write("----//----");
            foreach (var c in customers)
            {
                ObjectDumper.Write($"Customer {c.CustomerID}, Region: {c.Region}");
                ObjectDumper.Write($"PostalCode: { c.PostalCode}, Phone { c.Phone}");
                ObjectDumper.Write("----//----");
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 7")]
        [Description("This sample return grouped list of products by category, then by availability and sorted by price.")]
        public void Linq007()
        {
            var products = dataSource.Products.GroupBy(x => x.Category).
                 Select(c => new
                 {
                     Category = c.Key,
                     ProductsAvailable = c.GroupBy(x => x.UnitsInStock > 0)
                     .Select(p => new
                     {
                         IsAvailable = p.Key,
                         Products = p.OrderBy(o => o.UnitPrice)
                     })
                 });

            foreach (var p in products)
            {
                ObjectDumper.Write($"Product category: {p.Category}");

                foreach (var pr in p.ProductsAvailable)
                {
                    ObjectDumper.Write($"Is Product Available: {pr.IsAvailable}");
                    foreach (var product in pr.Products)
                    {
                        ObjectDumper.Write($"Name: {product.ProductName} Price {product.UnitPrice}.");
                    }
                }
                ObjectDumper.Write("------------//------------");
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 8")]
        [Description("This sample return grouped list of products by 3 categories: cheep, middle, expensive.")]
        public void Linq008()
        {
            int cheepBoundary = 20;
            int middleBoundary = 40;

            var groups = dataSource.Products.GroupBy(x => x.UnitPrice < cheepBoundary ? "Cheep"
            : x.UnitPrice < middleBoundary ? "Middle" : "Expensive");

            foreach (var g in groups)
            {
                ObjectDumper.Write(g.Key);
                foreach (var p in g)
                {
                    ObjectDumper.Write($"Product {p.ProductName} Price {p.UnitPrice}.");
                }
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 9")]
        [Description("This sample return average income and intensity by town.")]
        public void Linq009()
        {
            var statistics = dataSource.Customers.GroupBy(x => x.City)
                .Select(c => new
                {
                    City = c.Key,
                    AverageIncome = c.Where(i => i.Orders.Count() > 0)
                    .Select(i => i.Orders
                    .Select(p => p.Total).Average()).Average(),
                    AverageIntensity = c.Where(i => i.Orders.Count() > 0)
                    .Select(i => i.Orders.Count()).Average()
                });

            foreach (var g in statistics)
            {
                ObjectDumper.Write($"City: {g.City}; Average income: {String.Format("{0:0.000}", g.AverageIncome)};" +
                    $" Average intensity: {String.Format("{0:0.00}", g.AverageIntensity)}");
            }
        }

        [Category("Restriction Operators")]
        [Title("Task 10")]
        [Description("This sample return statistics of client activities(count of orders) by Month, by Year, by Month and Year.")]
        public void Linq010()
        {
            var statistics = dataSource.Customers
                .Select(c => new
                {
                    c.CustomerID,
                    ByMonth = c.Orders.GroupBy(x => x.OrderDate.Month).Select(x => new
                    {
                        Month = x.Key,
                        CountOfOrders = x.Count()
                    }),
                    ByYear = c.Orders.GroupBy(x => x.OrderDate.Year).Select(x => new
                    {
                        Year = x.Key,
                        CountOfOrders = x.Count()
                    }),
                    ByMonthAndYear = c.Orders.GroupBy(x => new { x.OrderDate.Month, x.OrderDate.Year }).Select(x => new
                    {
                        x.Key.Month,
                        x.Key.Year,
                        CountOfOrders = x.Count()
                    })
                });

            foreach (var s in statistics)
            {
                ObjectDumper.Write($"Customer {s.CustomerID}");
                ObjectDumper.Write($"Statistics by Month:");
                foreach (var m in s.ByMonth)
                {
                    ObjectDumper.Write($"Month: {m.Month}; Activity: {m.CountOfOrders}");
                }
                ObjectDumper.Write($"Statistics by Year:");
                foreach (var y in s.ByYear)
                {
                    ObjectDumper.Write($"Year: {y.Year}; Activity: {y.CountOfOrders}");
                }
                ObjectDumper.Write($"Statistics by Month and Year:");
                foreach (var my in s.ByMonthAndYear)
                {
                    ObjectDumper.Write($"Month: {my.Month}; Year {my.Year}; Activity: {my.CountOfOrders}");
                }
                ObjectDumper.Write($"------------//------------");
            }
        }
    }
}
