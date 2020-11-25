using Conneckt.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Coneckt.Web
{
    //This class gets all the authorizations for tracfone's API
    //For efficiency purposes all the authorizations are stored in the appsetting.json with exp time
    //If expired it should be replaced...
    public class TracfoneAuthorizations
    {

        private string _accessToken;
        private string _username;
        private string _password;
        private string _jwtAccessToken;
        private IConfiguration _configuration;

        public TracfoneAuthorizations(IConfiguration configuration)
        {
            _accessToken = configuration["Credentials:accessToken"];
            _username = configuration["Credentials:username"];
            _password = configuration["Credentials:password"];
            _jwtAccessToken = configuration["Credentials:jwtAccessToken"];
            _configuration = configuration;
        }

        public async Task<Authorization> GetServiceQualificationMgmt()
        {
            var path = "Authorizations:ServiceQualificationMgmt";
            var url = "api/service-qualification-mgmt/oauth/token?grant_type=client_credentials&scope=/service-qualification-mgmt"; ;

            return await GetOrAddAuth(path, url);
        }

        public async Task<Authorization> GetResourceMgmt()
        {
            var path = "Authorizations:ResourceMgmt";
            var url = "api/resource-mgmt/oauth/token?grant_type=client_credentials&scope=/resource-mgmt";

            return await GetOrAddAuth(path, url);
        }

        public async Task<Authorization> GetOrderMgmt()
        {
            var path = "Authorizations:GetOrderMgmt";
            var url = "api/order-mgmt/oauth/token?grant_type=client_credentials&scope=/order-mgmt";

            return await GetOrAddAuth(path, url);
        }

        public async Task<Authorization> GetCustomerMgmtJWT()
        {
            var auth = _configuration.GetSection("Authorizations:CustomerMgmtJWTt").Get<Authorization>();
            if (auth.exp_dateTime < DateTime.Now)
            {
                var client = new HttpClient();
                var url = "https://apigateway.tracfone.com/api/customer-mgmt/oauth/token/ro";

                client.DefaultRequestHeaders.Add("username", _username);
                client.DefaultRequestHeaders.Add("password", _password);
                client.DefaultRequestHeaders.Add("Authorization", _accessToken);

                HttpResponseMessage response = await client.PostAsync(url, null);
                var responseData = response.Content.ReadAsStringAsync().Result;
                dynamic responseJson = JObject.Parse(responseData);
                auth = new Authorization
                {
                    token_type = responseJson.token_type,
                    access_token = responseJson.access_token,
                    exp_dateTime = DateTime.Now.AddSeconds(double.Parse(responseJson.expires_in))
                };
                //set new auth
            }
            return auth;
        }

        private async Task<Authorization> GetOrAddAuth(string path, string url)
        {
            var auth = _configuration.GetSection("path").Get<Authorization>();
            if (auth.exp_dateTime < DateTime.Now)
            {
                var response = await Tracfone.PostAPIResponse(url, _accessToken);
                auth = new Authorization
                {
                    token_type = response.token_type,
                    access_token = response.access_token,
                    exp_dateTime = DateTime.Now.AddSeconds(double.Parse(response.expires_in))
                };
                //set new auth
            }
            return auth;
        }

    }
}
