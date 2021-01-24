using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Conneckt.Data;

namespace Coneckt.Web
{
    //These functions is where the APIs actully get called
    public static class TracfoneAPI
    {
        //Post to Tracfone API
        public static async Task<dynamic> PostAPIResponse(string url, string auth)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://apigateway.tracfone.com");
            client.DefaultRequestHeaders.Add("Authorization", auth);
            return await client.PostAsync(url, null);
        }

        //Overload post for request with data
        public static async Task<dynamic> PostAPIResponse(string url, string auth, object data)
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

            return await client.PostAsync(url, sendingData);
        }

        //Overload for requstes with username and password
        public static async Task<dynamic> PostAPIResponse(string url, string auth, string username, string password)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://apigateway.tracfone.com");

            client.DefaultRequestHeaders.Add("username", username);
            client.DefaultRequestHeaders.Add("password", password);
            client.DefaultRequestHeaders.Add("Authorization", auth);

            return await client.PostAsync(url, null);
        }

        //Overload for calls with coockie
        public static async Task<dynamic> PostAPIResponse(string url, string auth,object data, string cookie)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://apigateway.tracfone.com");

            client.DefaultRequestHeaders.Add("Authorization", auth);
            client.DefaultRequestHeaders.Add("Set-Cookie", cookie);

            return await client.PostAsync(url, null);
        }

        public static async Task<dynamic> GetAPIResponse(string url, string auth)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://apigateway.tracfone.com");
            client.DefaultRequestHeaders.Add("Authorization", auth);
            var response = await client.GetAsync(url);
            var responseData = response.Content.ReadAsStringAsync().Result;

            var json = JObject.Parse(responseData);
            return json;
        }

        public static async Task<dynamic> GetPaymentAPIResponse(string url, string auth)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://apigateway.tracfone.com");
            client.DefaultRequestHeaders.Add("Authorization", auth);
            var response = await client.GetAsync(url);
            var responseData = response.Content.ReadAsStringAsync().Result;

            var parsed = JObject.Parse(responseData);
            List<int> responseArr = new List<int>();
            
            if ((string) parsed["status"]["code"] == "0")
            {
                JsonTextReader reader = new JsonTextReader(new System.IO.StringReader(responseData));
                var readNext = false;
                while (reader.Read())
                {
                    if (reader.Value != null)
                    {
                        Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
                        if (readNext)
                        {
                            responseArr.Add((int)(long) reader.Value);
                            readNext = false;
                        }
                        if (reader.TokenType == JsonToken.PropertyName && (string) reader.Value == "paymentSourceId")
                        {
                            Console.WriteLine("here");
                            readNext = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Token: {0}", reader.TokenType);
                    }
                }
            }

            return responseArr;
        }

        //overload for cookies
        public static async Task<dynamic> GetAPIResponse(string url, string auth, string cookie)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://apigateway.tracfone.com");
            client.DefaultRequestHeaders.Add("Authorization", auth);
            client.DefaultRequestHeaders.Add("Set-Cookie", cookie);
            var response = await client.GetAsync(url);
            var responseData = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(responseData);
        }
    }
}
