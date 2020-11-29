using Conneckt.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

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

        public TracfoneAuthorizations(IConfiguration configuration)
        {
            _accessToken = configuration["Credentials:accessToken"];
            _username = configuration["Credentials:username"];
            _password = configuration["Credentials:password"];
            _jwtAccessToken = configuration["Credentials:jwtAccessToken"];
        }

        public async Task<Authorization> GetServiceQualificationMgmt()
        {
            var path = "ServiceQualificationMgmt";
            var url = "api/service-qualification-mgmt/oauth/token?grant_type=client_credentials&scope=/service-qualification-mgmt"; ;

            return await GetOrAddAuth(path, url);
        }

        public async Task<Authorization> GetResourceMgmt()
        {
            var path = "ResourceMgmt";
            var url = "api/resource-mgmt/oauth/token?grant_type=client_credentials&scope=/resource-mgmt";

            return await GetOrAddAuth(path, url);
        }

        public async Task<Authorization> GetOrderMgmt()
        {
            var path = "OrderMgmt";
            var url = "api/order-mgmt/oauth/token?grant_type=client_credentials&scope=/order-mgmt";

            return await GetOrAddAuth(path, url);
        }

        public async Task<Authorization> GetServiceMgmtJWT()
        {
            var path = "ServiceMgmtJWT";
            var url = "api/service-mgmt/oauth/token/ro";

            return await GetOrAddJWTAuth(path, url);
        }
        public async Task<Authorization> GetCustomerMgmtJWT()
        {
            var path = "CustomerMgmtJWT";
            var url = "api/customer-mgmt/oauth/token/ro";

            return await GetOrAddJWTAuth(path, url);
        }

        private async Task<Authorization> GetOrAddAuth(string path, string url)
        {
            var jsonString = File.ReadAllText("Authorizations.json");
            var authorizations = JsonConvert.DeserializeObject<Dictionary<string, Authorization>>(jsonString);
            var auth = authorizations[path];

            if (auth.Expires < DateTime.Now)
            {
                var response = await TracfoneAPI.PostAPIResponse(url, _accessToken);
                auth = new Authorization
                {
                    TokenType = response.token_type,
                    AccessToken = response.access_token,
                    Expires = DateTime.Now.AddSeconds((double) response.expires_in)
                };
                authorizations[path] = auth;
                var newJsonString = JsonConvert.SerializeObject(authorizations);
                File.WriteAllText("Authorizations.json", newJsonString);
            }

            return auth;
        }

        private async Task<Authorization> GetOrAddJWTAuth(string path, string url)
        {
            var jsonString = File.ReadAllText("Authorizations.json");
            var authorizations = JsonConvert.DeserializeObject<Dictionary<string, Authorization>>(jsonString);
            var auth = authorizations[path];
            if (auth.Expires < DateTime.Now)
            {
                var response = await TracfoneAPI.PostAPIResponse(url, _jwtAccessToken, _username, _password);
                double expires = response.expires_in;

                auth = new Authorization
                {
                    TokenType = response.token_type,
                    AccessToken = response.access_token,
                    Expires = DateTime.Now.AddSeconds((double) response.expires_in)
                };
                authorizations[path] = auth;
                var newJsonString = JsonConvert.SerializeObject(authorizations);
                File.WriteAllText("Authorizations.json", newJsonString);
            }
            return auth;
        }

    }
}
