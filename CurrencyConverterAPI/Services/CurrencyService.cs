using CurrencyConverterAPI.ViewModels;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CurrencyConverterAPI.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CurrencyService> _logger;
        private const string BASE_URL = "https://api.frankfurter.app/";

        public CurrencyService(IHttpClientFactory httpClientFactory, ILogger<CurrencyService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<RatesVM> GetLatestRatesAsync(string baseCurrency, IEnumerable<string> targetCurrencies = null)
        {            
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(baseCurrency))
            {
                queryParams.Add($"from={baseCurrency}");
            }

            if (targetCurrencies != null && targetCurrencies.Any())
            {
                queryParams.Add($"to={string.Join(",", targetCurrencies)}");
            }

            var queryString = string.Join("&", queryParams);
            var endpoint = string.IsNullOrEmpty(queryString) ? "latest" : $"latest?{queryString}";

            var response = await GetFromApiAsync(endpoint);
            var ratesVM = MapToRatesVM(response);

            return ratesVM;
        }

        public async Task<RatesVM> ConvertCurrencyAsync(decimal amount, string from, string to)
        {
            var endpoint = $"latest?amount={amount}&from={from}&to={to}";
            var response = await GetFromApiAsync(endpoint);
            return MapToRatesVM(response);
        }
      
        public async Task<HistoricalRatesVM> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime? endDate, int page, int pageSize, string toCurrencies)
        {
            string endpoint;

            if (endDate.HasValue)
            {
                // Time series endpoint if both start and end dates are provided
                endpoint = $"{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}?base={baseCurrency}";
            }
            else
            {
                // Historical endpoint for a single date if no end date is provided
                endpoint = $"{startDate:yyyy-MM-dd}?base={baseCurrency}";
            }

            // Append "to" parameter if specific currencies are required (optional)
            if (!string.IsNullOrWhiteSpace(toCurrencies))
            {
                endpoint += $"&to={toCurrencies}";
            }

            var response = await GetFromApiAsync(endpoint);
            var allRates = ProcessHistoricalResponse(response.ToString());
            var pagedRates = PaginateRates(allRates, page, pageSize);

            return new HistoricalRatesVM(
                amount: 1, 
                baseCurrency: baseCurrency,
                startDate: startDate,
                endDate: endDate ?? startDate, 
                rates: pagedRates
            );
        }


        private Dictionary<string, Dictionary<string, decimal>> ProcessHistoricalResponse(string response)
        {
            var rates = new Dictionary<string, Dictionary<string, decimal>>();

            // Parse the JSON response
            var jsonResponse = JObject.Parse(response);

            // Extract the "rates" object, which contains the dates and rates
            var ratesObject = jsonResponse["rates"] as JObject;

            if (ratesObject == null)
            {
                throw new Exception("Rates object not found in response.");
            }

            // Extract the rates for each date
            foreach (var dateProperty in ratesObject.Properties())
            {
                var date = dateProperty.Name;
                var dateRatesObject = dateProperty.Value as JObject;

                if (dateRatesObject != null)
                {
                    var currencyRates = new Dictionary<string, decimal>();

                    // Iterate over each currency in the "rates" object for this date
                    foreach (var rate in dateRatesObject.Properties())
                    {
                        var currency = rate.Name;
                        var valueToken = rate.Value;

                        // Check if the value is a decimal
                        if (valueToken.Type == JTokenType.Float || valueToken.Type == JTokenType.Integer)
                        {
                            currencyRates[currency] = valueToken.Value<decimal>();
                        }
                        else
                        {
                            var x = valueToken.Value<decimal>();
                            // Handle the case where the rate is not a simple decimal value
                            throw new Exception($"Unexpected rate format for currency: {currency} on {date}");
                        }
                    }

                    rates[date] = currencyRates;
                }
            }

            return rates;
        }




        private async Task<JObject> GetFromApiAsync(string endpoint)
        {
            var client = _httpClientFactory.CreateClient();
            var retries = 3;

            for (int i = 0; i < retries; i++)
            {
                try
                {
                    var response = await client.GetAsync($"{BASE_URL}{endpoint}");
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();
                    return JObject.Parse(content);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Request failed.");
                    if (i == retries - 1)
                        throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred.");
                    if (i == retries - 1)
                        throw;
                }
            }

            return null;
        }


        private Dictionary<string, Dictionary<string, decimal>> PaginateRates(Dictionary<string, Dictionary<string, decimal>> rates, int page, int pageSize)
        {         
            var pagedRates = rates.Skip((page - 1) * pageSize).Take(pageSize).ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value
            );

            return pagedRates;
        }

        private RatesVM MapToRatesVM(JObject response)
        {         
            var amount = response["amount"]?.Value<decimal>() ?? 0;
            var baseCurrency = response["base"]?.ToString();
            var date = response["date"]?.ToString();

            // Extract rates and convert them to a dictionary
            var ratesObject = response["rates"] as JObject;
            var rates = new Dictionary<string, decimal>();

            if (ratesObject != null)
            {
                foreach (var rate in ratesObject)
                {
                    var currency = rate.Key;
                    var value = rate.Value.Value<decimal>();

                    // Add the rate to the dictionary
                    rates[currency] = value;
                }
            }

            // Create and return the RatesVM object
            return new RatesVM
            {
                Amount = amount,
                Base = baseCurrency,
                Date = date,
                Rates = rates
            };
        }

       
    }
}
