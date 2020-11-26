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
            //BYOP Eligibility
            dynamic byopEligibilityResult = null;
            for (int i = 0; i < 3; i++)
            {
                byopEligibilityResult = await _tracfone.CheckBYOPEligibility(model.Serial);
            }
            string byopEligibilityStatus = byopEligibilityResult["status"]["code"].ToString();

            if (byopEligibilityStatus != "200")
            {
                return Json(byopEligibilityResult);
            }

            //BYOP Registration
            dynamic byopRegistrationResult = await _tracfone.BYOPRegistration(model);
            string byopRegistrationStatus = byopRegistrationResult["status"]["code"].ToString();

            if (byopRegistrationStatus != "200")
            {
                return Json(byopRegistrationResult);
            }

            //Add Device
            var addDeviceResult = await _tracfone.AddDevice(model.Serial);
            return Json(addDeviceResult);
        }

        public async Task<IActionResult> DeleteDevice(DeleteActionModel model)
        {
            var result = await _tracfone.DeleteDevice(model.Serial);
            return Json(result);
        }

        public async Task<IActionResult> Activate(ActivateActionModel model)
        {
            var result = await _tracfone.Activate(model);
            return Json(result);
        }

        public async Task<IActionResult> ExternalPort(PortActionModel model)
        {
            var result = await _tracfone.ExternalPort(model);
            return Json(result);
        }

        public async Task<IActionResult> InternalPort(PortActionModel model)
        {
            var result = await _tracfone.InternalPort(model);
            return Json(result);
        }

        public async Task<IActionResult> GetAccountDetails(GetAccountDetailsActionModel model)
        {
            var result = await _tracfone.GetAccountDetails(model.Offset, model.Limit);
            return View(result);
        }

        public async Task<IActionResult> GetBalance(GetBalanceActionModel model)
        {
            var result = await _tracfone.GetBalance(model.PhoneNumber);
            return Json(result);
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

                        //BYOP Eligibility
                        dynamic byopEligibilityResult = null;
                        for (int i = 0; i < 3; i++)
                        {
                            byopEligibilityResult = await _tracfone.CheckBYOPEligibility(addDeviceModel.Serial);
                        }
                        string byopEligibilityStatus = byopEligibilityResult["status"]["code"].ToString();

                        if (byopEligibilityStatus != "200")
                        {
                            results.Add(byopEligibilityResult);
                        }

                        //BYOP Registration
                        dynamic byopRegistrationResult = await _tracfone.BYOPRegistration(addDeviceModel);
                        string byopRegistrationStatus = byopRegistrationResult["status"]["code"].ToString();

                        if (byopRegistrationStatus != "200")
                        {
                            results.Add(byopRegistrationResult);
                        }

                        //Add Device
                        var addDeviceResult = await _tracfone.AddDevice(addDeviceModel.Serial);
                        results.Add(addDeviceResult);
                        break;
                    case BulkAction.DeleteDevice:
                        var deleteSerial = data.Serial;


                        results.Add(await _tracfone.DeleteDevice(deleteSerial));
                        break;
                    case BulkAction.Activate:
                        var activateModel = new ActivateActionModel
                        {
                            Serial = data.Serial,
                            Sim = data.Sim,
                            Zip = data.Zip
                        };

                        results.Add(await _tracfone.Activate(activateModel));
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

                        results.Add(await _tracfone.InternalPort(internelPortModel));
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

                        results.Add(await _tracfone.ExternalPort(externelPortModel));
                        break;
                }
            }

            return Json(results);
        }
    }
}
