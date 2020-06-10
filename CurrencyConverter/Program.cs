using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CurrencyConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            CurrencyManager currencyManager = new CurrencyManager();

            PrePopulateDB(currencyManager);

            // Some examples using ConvertCurrency
            Console.WriteLine("100 USD = " + currencyManager.ConvertCurrency("USD", "EUR", 100, true) + " EUR (API)");
            Console.WriteLine("7000 ARS = " + currencyManager.ConvertCurrency("ARS", "USD", 7000) + " USD (DB)");
            Console.WriteLine("1 USD = " + currencyManager.ConvertCurrency("USD", "PHP", 1, true) + " PHP (API)");
            Console.WriteLine("1 USD = " + currencyManager.ConvertCurrency("USD", "PHP", 1) + " PHP (DB)");

            // CRUD example
            currencyManager.RemoveCurrency("ARS");
            currencyManager.AddCurrency("ARS", 69.17);
            currencyManager.UpdateCurrency("ARS", 69.20);

            // Some examples using GetCurrencyExchangeRate
            Console.WriteLine("USD to EUR (Exchange Rate): " + currencyManager.GetCurrencyExchangeRate("EUR") + " (DB)");
            Console.WriteLine("USD to EUR (Exchange Rate): " + currencyManager.GetCurrencyExchangeRate("EUR", true) + " (API)");

            //Display all the currencies
            currencyManager.GetAllCurrencies().ForEach(c => Console.WriteLine(c.ToString()));

            Console.ReadKey();
        }

        // Add some initial currencies to database
        public static void PrePopulateDB(CurrencyManager currencyManager)
        {
            using (var context = new BankContext())
            {
                context.Database.EnsureCreated();

                if (!context.Currency.Any())
                {
                    currencyManager.AddCurrency("USD", 1);
                    currencyManager.AddCurrency("ARS", 69.50);
                    currencyManager.AddCurrency("EUR", 0.89);
                    currencyManager.AddCurrency("PHP", 43.1232);
                    currencyManager.AddCurrency("BRL", 4.82);
                }
            }
        }
    }
}
