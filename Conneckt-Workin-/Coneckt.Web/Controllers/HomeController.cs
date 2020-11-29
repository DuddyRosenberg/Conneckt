using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Coneckt.Web.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Conneckt.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using System.IO;

namespace Coneckt.Web.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration;
        private Tracfone _tracfone;
        private string _connectionString;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            _tracfone = new Tracfone(configuration);
            _connectionString = configuration.GetConnectionString("ConnectionString");
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AddDevice(AddDeviceActionModel model)
        {
            string result = "";

            //BYOP Eligibility
            dynamic byopEligibilityResult = null;
            string byopEligibilityStatus = "";

            for (int i = 0; i < 3; i++)
            {
                byopEligibilityResult = await _tracfone.CheckBYOPEligibility(model.Serial);
                byopEligibilityStatus = byopEligibilityResult["status"]["code"].ToString();
                result = "BYOP Eligibility:" + byopEligibilityResult["status"]["message"].ToString();

                if (byopEligibilityStatus == "10008" ||
                    byopEligibilityStatus == "11023" ||
                    byopEligibilityStatus == "0")
                    break;

            }

            if (byopEligibilityStatus != "10008" &&
                    byopEligibilityStatus != "11023" &&
                    byopEligibilityStatus != "0")
            {
                return Json(result);
            }

            //BYOP Registration
            dynamic byopRegistrationResult = await _tracfone.BYOPRegistration(model);
            if (byopRegistrationResult == "0")
            {
                result += "\nBYOP Registration:" + byopRegistrationResult["status"]["message"].ToString();
            }
            else
            {
                return Json(result);
            }
            //Add Device
            var addDeviceResult = await _tracfone.AddDevice(model.Serial);
            result += "\nAdd Device" + addDeviceResult["status"]["message"].ToString();
            return Json(result);
        }

        public async Task<IActionResult> DeleteDevice(DeleteActionModel model)
        {
            var result = await _tracfone.DeleteDevice(model.Serial);
            return Json(result["status"]["message"].ToString());
        }

        public async Task<IActionResult> Activate(ActivateActionModel model)
        {
            var result = await _tracfone.Activate(model);
            return Json(result["status"]["message"].ToString());
        }

        public async Task<IActionResult> ExternalPort(PortActionModel model)
        {
            var result = await _tracfone.ExternalPort(model);
            return Json(result["status"]["message"].ToString());
        }

        public async Task<IActionResult> InternalPort(PortActionModel model)
        {
            var result = await _tracfone.InternalPort(model);
            return Json(result["status"]["message"].ToString());
        }

        public async Task<IActionResult> GetAccountDetails(GetAccountDetailsActionModel model)
        {
            dynamic result = (dynamic)await _tracfone.GetAccountDetails(0, 20);
            return View();
        }

        public async Task<IActionResult> GetBalance(GetBalanceActionModel model)
        {
            var result = await _tracfone.GetBalance(model.PhoneNumber);
            return Json(result["status"]["message"].ToString());
        }

        public async Task<IActionResult> ExecuteBulk()
        {
            var repo = new Repository(_connectionString);
            var bulkData = repo.GetAllBulkData();

            var results = new List<IActionResult>();
            foreach (BulkData data in bulkData)
            {
                switch (data.Action)
                {
                    case BulkAction.AddDevice:
                        var addDeviceModel = new AddDeviceActionModel
                        {
                            Serial = data.Serial,
                            Sim = data.Sim
                        };

                        results.Add(await AddDevice(addDeviceModel));
                        break;
                    case BulkAction.DeleteDevice:
                        var deleteModel = new DeleteActionModel
                        {
                            Serial = data.Serial
                        };

                        results.Add(await DeleteDevice(deleteModel));
                        break;
                    case BulkAction.Activate:
                        var activateModel = new ActivateActionModel
                        {
                            Serial = data.Serial,
                            Sim = data.Sim,
                            Zip = data.Zip
                        };

                        results.Add(await Activate(activateModel));
                        break;
                    case BulkAction.InternalPort:
                        var internelPortModel = new PortActionModel
                        {
                            Serial = data.Serial,
                            Sim = data.Sim,
                            Zip = data.Zip,
                            CurrentAccountNumber = data.CurrentAccountNumber,
                            CurrentMIN = data.CurrentMIN,
                            CurrentServiceProvider = data.CurrentServiceProvider,
                            CurrentVKey = data.CurrentVKey
                        };

                        results.Add(await InternalPort(internelPortModel));
                        break;
                    case BulkAction.ExternalPort:
                        var externelPortModel = new PortActionModel
                        {
                            Serial = data.Serial,
                            Sim = data.Sim,
                            Zip = data.Zip,
                            CurrentAccountNumber = data.CurrentAccountNumber,
                            CurrentMIN = data.CurrentMIN,
                            CurrentServiceProvider = data.CurrentServiceProvider,
                        };

                        results.Add(await ExternalPort(externelPortModel));
                        break;
                }
            }

            return Json(results);
        }
    }
}
