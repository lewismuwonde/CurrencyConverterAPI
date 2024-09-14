using CurrencyConverterAPI.ViewModels;

namespace CurrencyConverterAPI.Services
{
    public interface ICurrencyService
    {
        Task<RatesVM> GetLatestRatesAsync(string baseCurrency, IEnumerable<string> targetCurrencies);
        Task<RatesVM> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency);
        Task<HistoricalRatesVM> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime? endDate, int page, int pageSize, string toCurrencies);
    }
}
