using Newtonsoft.Json;
using NMSCD.BaitBox.Contracts;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NMSCD.BaitBox.Repository
{
    public class GoogleSheetRepository : BaseExternalApiRepository
    {
        private const string _restApiBase = "https://sheets.googleapis.com/v4/spreadsheets/";
        private const string _getValuesUrl = _restApiBase + "{0}/values/{1}?valueRenderOption=UNFORMATTED_VALUE";
        private const string _clearValuesUrl = _restApiBase + "{0}/values/{1}:clear";
        private const string _appendValuesUrl = _restApiBase + "{0}/values/{1}:append";
        private string _googleApiKey;

        public GoogleSheetRepository(HttpClient httpClient, string googleAuthApiKey) : base(httpClient)
        {
            _googleApiKey = googleAuthApiKey;
        }

        /// <summary>
        /// Get Sheet as 2 Dimensional Array
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="sheetRange">Format can be found here: https://developers.google.com/sheets/api/guides/concepts</param>
        /// <returns>A list of string lists</returns>
        public async Task<ResultWithValue<List<List<string>>>> GetSheetContents(string sheetName, string sheetRange)
        {
            string url = _getValuesUrl.Replace("{0}", sheetName).Replace("{1}", sheetRange);
            ResultWithValue<GoogleSheet> sheetResult = await Get<GoogleSheet>(url, headers =>
            {
                headers.Add("X-goog-api-key", _googleApiKey);
            });

            return new ResultWithValue<List<List<string>>>(
                sheetResult.IsSuccess,
                sheetResult.Value.Values,
                sheetResult.ExceptionMessage
            );
        }

        /// <summary>
        /// Clear sheet content
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="sheetRange">Format can be found here: https://developers.google.com/sheets/api/guides/concepts</param>
        /// <returns>A result (isSuccess & ExceptionMessage)</returns>
        public async Task<Result> ClearSheetContents(string sheetName, string sheetRange)
        {
            string url = _clearValuesUrl.Replace("{0}", sheetName).Replace("{1}", sheetRange);
            ResultWithValue<string> sheetResult = await Post<string>(url, string.Empty, headers =>
            {
                headers.Add("X-goog-api-key", _googleApiKey);
            });

            return new Result(
                sheetResult.IsSuccess,
                sheetResult.ExceptionMessage
            );
        }

        /// <summary>
        /// Add content to sheet
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="sheetRange">Format can be found here: https://developers.google.com/sheets/api/guides/concepts</param>
        /// <returns>A result (isSuccess & ExceptionMessage)</returns>
        public async Task<Result> AddSheetContents(string sheetName, string sheetRange, List<List<string>> data)
        {
            string url = _appendValuesUrl.Replace("{0}", sheetName).Replace("{1}", sheetRange);
            string stringBody = JsonConvert.SerializeObject(new GoogleSheet
            {
                MajorDimension = "ROWS",
                Range = sheetRange,
                Values = data
            });
            ResultWithValue<string> sheetResult = await Post<string>(url, stringBody, headers =>
            {
                headers.Add("X-goog-api-key", _googleApiKey);
            });

            return new Result(
                sheetResult.IsSuccess,
                sheetResult.ExceptionMessage
            );
        }
    }
}
