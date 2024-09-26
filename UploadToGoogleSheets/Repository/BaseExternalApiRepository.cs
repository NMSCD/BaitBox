using Newtonsoft.Json;
using NMSCD.BaitBox.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NMSCD.BaitBox.Repository
{
    public class BaseExternalApiRepository
    {
        private readonly HttpClient _httpClient;

        public BaseExternalApiRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected async Task<ResultWithValue<T>> Get<T>(string url, Action<HttpRequestHeaders>? manipulateHeaders = null)
        {
            ResultWithValue<string> webGetResult = await Get(url, manipulateHeaders);
            if (webGetResult.HasFailed) return new ResultWithValue<T>(false, default!, webGetResult.ExceptionMessage);

            try
            {
                T? result = JsonConvert.DeserializeObject<T>(webGetResult.Value);
                if (result == null) throw new Exception("Json deserialisation returned an empty object");
                return new ResultWithValue<T>(true, result, string.Empty);
            }
            catch (Exception ex)
            {
                return new ResultWithValue<T>(false, default!, ex.Message);
            }
        }

        protected async Task<ResultWithValue<string>> Get(string url, Action<HttpRequestHeaders>? manipulateHeaders = null)
        {
            try
            {
                manipulateHeaders?.Invoke(_httpClient.DefaultRequestHeaders);
                HttpResponseMessage httpResponse = await _httpClient.GetAsync(url);
                string content = await httpResponse.Content.ReadAsStringAsync();
                return new ResultWithValue<string>(true, content, string.Empty);
            }
            catch (Exception ex)
            {
                return new ResultWithValue<string>(false, default!, ex.Message);
            }
        }

        public async Task<ResultWithValue<T>> Post<T>(
            string url,
            string postContentString,
            Action<HttpRequestHeaders>? manipulateHeaders = null,
            string mediaType = "application/json"
        )
        {
            HttpClient client = new HttpClient();
            try
            {
                manipulateHeaders?.Invoke(client.DefaultRequestHeaders);
                StringContent postContent = new StringContent(postContentString, Encoding.UTF8, mediaType);
                HttpResponseMessage httpResponse = await client.PostAsync(url, postContent);
                httpResponse.EnsureSuccessStatusCode();
                string content = await httpResponse.Content.ReadAsStringAsync();

                T? result = JsonConvert.DeserializeObject<T>(content);
                if (result == null) throw new Exception("Json deserialisation returned an empty object");
                return new ResultWithValue<T>(true, result, string.Empty);
            }
            catch (Exception ex)
            {
                return new ResultWithValue<T>(false, default!, ex.Message);
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}