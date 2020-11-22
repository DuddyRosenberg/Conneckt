using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Coneckt.Web
{
    public class Tracfone
    {
        public async Task<dynamic> PostAPIResponse(string url, string auth)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://apigateway.tracfone.com");
            client.DefaultRequestHeaders.Add("Authorization", auth);
            var response = await client.PostAsync(url, null);
            var responseData = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(responseData);
        }

        //Overload for request with data
        public async Task<dynamic> PostAPIResponse(string url, string auth, object data)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://apigateway.tracfone.com");
            client.DefaultRequestHeaders.Add("Authorization", auth);
            //convert to json
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            var jsonString = JsonConvert.SerializeObject(data, settings);
            var sendingData = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, sendingData);
            var responseData = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(responseData);
        }

        public async Task<dynamic> GetBearerAuthorization(string accessToken)
        {
            var url = "api/order-mgmt/oauth/token?grant_type=client_credentials&scope=/order-mgmt";
            return await PostAPIResponse(url, accessToken);
        }

        public async Task<dynamic> GetJWTAuthorization
            (string username, string password, string accessToken)
        {
            var client = new HttpClient();
            var url = "https://apigateway.tracfone.com/api/customer-mgmt/oauth/token/ro";

            client.DefaultRequestHeaders.Add("username", username);
            client.DefaultRequestHeaders.Add("password", password);
            client.DefaultRequestHeaders.Add("Authorization", accessToken);

            HttpResponseMessage response = await client.PostAsync(url, null);
            if (response.IsSuccessStatusCode)
            {
                var responseData = response.Content.ReadAsStringAsync().Result;
                return JObject.Parse(responseData);
            }
            return "Error";
        }
    }
}
