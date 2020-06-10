using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;

namespace CurrencyConverter
{
    public class CurrencyManager
    {
        // Currency API
        private const string Currency_API_Endpoint = "https://api.exchangeratesapi.io/";
        private const string Currency_API_Base_USD_Specific_Codes = "latest?base=USD&symbols=";

        // Convert from one currency to another using the exchange rate from DB or from the API
        public decimal ConvertCurrency(string currencyFrom, string currencyTo, decimal amount, bool useAPI = false)
        {
            if (amount < 0)
            {
                throw new ArgumentException("Amount cannot be smaller than 0");
            }

            // Retrieve currencies' exchange rate
            double currencyFromExchangeRate = GetCurrencyExchangeRate(currencyFrom, useAPI);
            double currencyToExchangeRate = GetCurrencyExchangeRate(currencyTo, useAPI);

            // Calculate the conversion rate
            decimal conversionRate = Convert.ToDecimal(currencyToExchangeRate / currencyFromExchangeRate);

            // Return the new currency with two decimals
            return Math.Round(conversionRate * amount, 2);
        }

        // Get currency exchange rate from DB or from API
        public double GetCurrencyExchangeRate(string currencyCode, bool useAPI = false)
        {
            if (useAPI)
            {
                dynamic result = GetRequest(Currency_API_Base_USD_Specific_Codes + currencyCode);

                return result.rates[currencyCode];
            }
            else
            {
                return GetCurrencyByCurrencyCode(currencyCode).ExchangeRate;
            }
        }

        // CRUD - Read Section
        public Currency GetCurrencyById(int currencyId)
        {
            using (var context = new BankContext())
            {
                return context.Currency.Find(currencyId);
            }
        }
        public Currency GetCurrencyByCurrencyCode(string currencyCode)
        {
            using (var context = new BankContext())
            {
                return context.Currency.Where(c => c.CurrencyCode == currencyCode).FirstOrDefault();
            }
        }
        public List<Currency> GetAllCurrencies()
        {
            using (var context = new BankContext())
            {
                return context.Currency.ToList();
            }
        }

        // CRUD - Create Section
        public void AddCurrency(string currencyCode, double exchangeRate)
        {
            if (currencyCode.Length != 3)
            {
                throw new ArgumentException("CurrencyId must be 3 characters long");
            }

            if (exchangeRate < 0)
            {
                throw new ArgumentException("ExchangeRate must be greater than 0");
            }

            using (var context = new BankContext())
            {
                if (!context.Currency.Any(c => c.CurrencyCode.Equals(currencyCode.ToUpper())))
                {
                    context.Currency.Add(new Currency { CurrencyCode = currencyCode.ToUpper(), ExchangeRate = exchangeRate });

                    context.SaveChanges();
                }
                else
                {
                    throw new ArgumentException("Cannot add a duplicate currencyCode");
                }
            }
        }

        // CRUD - Delete Section
        public void RemoveCurrency(string currencyCode)
        {
            using (var context = new BankContext())
            {
                Currency currency = GetCurrencyByCurrencyCode(currencyCode);

                if (currency != null)
                {
                    context.Currency.Remove(GetCurrencyByCurrencyCode(currencyCode));

                    context.SaveChanges();
                }
                else
                {
                    throw new ArgumentException("Cannot remove a null");
                }
            }
        }

        // CRUD - Update Section
        public void UpdateCurrency(string currencyCode, double exchangeRate)
        {
            if (exchangeRate < 0)
            {
                throw new ArgumentException("ExchangeRate must be greater than 0");
            }

            using (var context = new BankContext())
            {
                Currency currency = GetCurrencyByCurrencyCode(currencyCode);

                if (currency != null)
                {
                    currency.ExchangeRate = exchangeRate;

                    context.Currency.Update(currency);

                    context.SaveChanges();
                }
                else
                {
                    throw new ArgumentException("Cannot update a null");
                }
            }
        }

        // Http Client GET Request Helper
        private dynamic GetRequest(string requestUri)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Currency_API_Endpoint);

                var responseTask = client.GetStringAsync(requestUri);

                responseTask.Wait();

                return JsonConvert.DeserializeObject<dynamic>(responseTask.Result);
            }
        }
    }

    // Currency Model
    public class Currency
    {
        public int CurrencyId { get; set; }

        [StringLength(3, MinimumLength = 3, ErrorMessage = "CurrencyCode must be 3 characters long.")]
        public string CurrencyCode { get; set; }

        [Range(0.0, Double.MaxValue, ErrorMessage = "ExchangeRate must be greater than 0.")]
        public double ExchangeRate { get; set; }

        public override string ToString()
        {
            return $"CurrencyId: {CurrencyId} - CurrencyCode: {CurrencyCode} - ExchangeRate: {ExchangeRate}";
        }
    }

    // Entity Framework Core Context
    public class BankContext : DbContext
    {
        // Create Currency table
        public DbSet<Currency> Currency { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Use Local SQL DB
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BankDB;Trusted_Connection=True;");
        }
    }
}
