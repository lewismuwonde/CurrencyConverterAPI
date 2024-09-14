using CurrencyConverterAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyConverter : ControllerBase
    {

        private readonly ICurrencyService _currencyService;
        private readonly ILogger<CurrencyConverter> _logger;
    
        public CurrencyConverter(ICurrencyService currencyService, ILogger<CurrencyConverter> logger)
        {
            _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("latest/{baseCurrency}")]
        public async Task<IActionResult> GetLatestRates(string baseCurrency, [FromQuery] IEnumerable<string> to = null)
        {           
            if (string.IsNullOrEmpty(baseCurrency))
                return BadRequest("Base currency is required.");
           
            var rates = await _currencyService.GetLatestRatesAsync(baseCurrency, to);

            if (rates == null)
                return NotFound("Rates not found.");

            return Ok(rates);
        }

        [HttpGet("convert")]
        public async Task<IActionResult> ConvertCurrency(decimal amount, string from, string to)
        {
            var excludedCurrencies = new[] { "TRY", "PLN", "THB", "MXN" };
            if (excludedCurrencies.Contains(to))
                return BadRequest("Currency conversion not supported for TRY, PLN, THB, MXN.");

            var result = await _currencyService.ConvertCurrencyAsync(amount, from, to);
            if (result == null)
                return BadRequest("Conversion failed.");

            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistoricalRates(string baseCurrency, DateTime startDate, DateTime? endDate = null, int page = 1, int pageSize = 10, string toCurrencies = "")
        {
            var historicalRates = await _currencyService.GetHistoricalRatesAsync(baseCurrency, startDate, endDate, page, pageSize, toCurrencies);
            if (historicalRates == null)
                return BadRequest("Invalid request or no data found.");

            return Ok(historicalRates);
        }

    }
}
