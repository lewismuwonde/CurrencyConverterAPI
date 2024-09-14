namespace CurrencyConverterAPI.ViewModels
{

    public class HistoricalRatesVM
    {
        public decimal Amount { get; set; }
        public string BaseCurrency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; }

        public HistoricalRatesVM(decimal amount, string baseCurrency, DateTime startDate, DateTime endDate, Dictionary<string, Dictionary<string, decimal>> rates)
        {
            Amount = amount;
            BaseCurrency = baseCurrency;
            StartDate = startDate;
            EndDate = endDate;
            Rates = rates;
        }
    }

}
