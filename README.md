# CurrencyConverterAPI

Currency Converter API
This project is a Currency Converter API built using C# and ASP.NET Core. It integrates with the Frankfurter API to fetch real-time and historical exchange rates.

# Requirements
.NET 6.0 SDK or later
Visual Studio 2022 / VSCode
Postman or any API testing tool
Access to the Frankfurter API

# Setup and Installation
Clone the repository:
bash
git clone https://github.com/lewismuwonde/CurrencyConverterAPI.git
cd CurrencyConverterAPI
Open the solution file in Visual Studio or VSCode.

Restore the NuGet packages:
bash
dotnet restore
Update the appsettings.json file with the base URL of the Frankfurter API if necessary.

Build the solution:
bash
dotnet build

Running the Application
You can run the API locally by using the following command:
bash
dotnet run
By default, the application will run on https://localhost:5001 or http://localhost:5000.

Swagger UI
To test the API, you can use the built-in Swagger UI. Once the API is running, navigate to:

bash
Copy code
https://localhost:5001/swagger
This will display all available endpoints with the ability to send requests and view responses directly from the browser.

# Endpoints
Endpoints
Here are the main endpoints exposed by the Currency Converter API:

Get Latest Rates
This endpoint returns the latest exchange rates for a given base currency, and optionally converts to specific currencies.

GET /latest/{baseCurrency}?to={commaSeparatedCurrencies}
Parameters:
baseCurrency: The currency to convert from (e.g., USD).
to: (Optional) A comma-separated list of currencies to convert to (e.g., EUR,GBP).
Example Request: /latest/USD?to=EUR,GBP

Example Response:

json
{
  "base": "USD",
  "date": "2024-09-13",
  "rates": {
    "EUR": 0.85,
    "GBP": 0.75
  }
}
Get Historical Rates
This endpoint returns historical rates for a specified base currency over a given time range.

GET /history?baseCurrency={currency}&startDate={yyyy-MM-dd}&endDate={yyyy-MM-dd}&page={page}&pageSize={pageSize}&to={commaSeparatedCurrencies}
Parameters:
baseCurrency: The currency to convert from (e.g., USD).
startDate: The start date for historical data (format: yyyy-MM-dd).
endDate: The end date for historical data (format: yyyy-MM-dd).
page: (Optional) The page number for pagination.
pageSize: (Optional) The number of records per page for pagination.
to: (Optional) A comma-separated list of currencies to convert to.
Example Request: /history?baseCurrency=USD&startDate=2023-01-01&endDate=2023-01-31&page=1&pageSize=10&to=EUR,GBP

Example Response:

json
{
  "base": "USD",
  "startDate": "2023-01-01",
  "endDate": "2023-01-31",
  "rates": {
    "2023-01-01": {
      "EUR": 0.85,
      "GBP": 0.75
    },
    "2023-01-02": {
      "EUR": 0.86,
      "GBP": 0.74
    }
  }
}
Convert Currency
This endpoint converts a specified amount from one currency to another based on the latest available exchange rate.

GET /convert?from={fromCurrency}&to={toCurrency}&amount={amount}
Parameters:
from: The currency to convert from (e.g., USD).
to: The currency to convert to (e.g., EUR).
amount: The amount to convert.
Example Request: /convert?from=USD&to=EUR&amount=100

Example Response:

json
{
  "base": "USD",
  "amount": 100,
  "convertedAmount": 85.0,
  "toCurrency": "EUR",
  "date": "2024-09-13"
}
