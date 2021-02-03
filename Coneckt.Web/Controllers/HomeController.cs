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

        public async Task<IActionResult> Index()
        {
            // Get Payment Sources
            var paymentSources = await _tracfone.GetPaymetSources();
            return View(paymentSources);
        }

        public async Task<IActionResult> AddDevice(AddDeviceActionModel model)
        {
            //BYOP Eligibility
            dynamic byopEligibilityResult = null;
            string byopEligibilityStatus = "10062";
            while (byopEligibilityStatus == "10062")
            {
                byopEligibilityResult = await _tracfone.CheckBYOPEligibility(model.Serial);
                byopEligibilityStatus = byopEligibilityResult["status"]["code"].ToString();
            }

            if (byopEligibilityStatus != "0" && byopEligibilityStatus != "10008" && byopEligibilityStatus != "11023")
            {
                byopEligibilityResult["request"] = "BYOP Eligibility Failed";
                return Json(byopEligibilityResult);
            }

            // Skip BYOP Registration if already added
            if (byopEligibilityStatus == "10008" || byopEligibilityStatus == "11023")
            {
                //Add Device
                var addDeviceResult2 = await _tracfone.AddDevice(model.Serial);
                return Json(addDeviceResult2);
            }

            //BYOP Registration
            dynamic byopRegistrationResult = await _tracfone.BYOPRegistration(model);
            string byopRegistrationStatus = byopRegistrationResult["status"]["code"].ToString();

            if (byopRegistrationStatus != "0")
            {
                byopRegistrationResult["request"] = "BYOP Registration Failed";
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
            var loginCookie = await _tracfone.Login();
            var estOrder = await _tracfone.EstimateOrder(loginCookie, model);
            var paymentMean = await _tracfone.GetPaymentSourceDetails(model.PaymentMeanID);
            var response = new PaymentMean
            {
                ID = model.PaymentMeanID,
                AccountHolderName = paymentMean.response.paymentSourceDetail[0].customerDetails.firstName + " " + paymentMean.response.paymentSourceDetail[0].customerDetails.lastName,
                FirstName = paymentMean.response.paymentSourceDetail[0].customerDetails.firstName,
                LastName = paymentMean.response.paymentSourceDetail[0].customerDetails.lastName,
                IsDefault = true,
                IsExisting = "FALSE",
                Type = "CREDITCARD",
                CreditCard = new CreditCard
                {
                    Type = paymentMean.response.paymentSourceDetail[0].cardDetails.cardType,
                    Year = paymentMean.response.paymentSourceDetail[0].cardDetails.expirationYear,
                    Month = paymentMean.response.paymentSourceDetail[0].cardDetails.expirationMonth,
                    Cvv = model.CVV,
                },
                BillingAddress = model.BillingAddress
            };
            var submitOrder = await _tracfone.SubmitOrder(loginCookie, model, estOrder, response);
            var result = await _tracfone.Activate(model);
            return Json(result);
        }

        public async Task<IActionResult> ExternalPort(PortActionModel model)
        {
            var loginCookie = await _tracfone.Login();
            var estOrder = await _tracfone.EstimateOrder(loginCookie, model);
            var paymentMean = (PaymentMean)await _tracfone.GetPaymentSourceDetails(model.PaymentMeanID);
            await _tracfone.SubmitOrder(loginCookie, model, estOrder, paymentMean);
            var result = await _tracfone.ExternalPort(model);
            return Json(result);
        }

        public async Task<IActionResult> InternalPort(PortActionModel model)
        {
            var loginCookie = await _tracfone.Login();
            var estOrder = await _tracfone.EstimateOrder(loginCookie, model);
            var paymentMean = (PaymentMean)await _tracfone.GetPaymentSourceDetails(model.PaymentMeanID);
            await _tracfone.SubmitOrder(loginCookie, model, estOrder, paymentMean);
            var result = await _tracfone.InternalPort(model);
            return Json(result);
        }

        public async Task<IActionResult> GetAccountDetails(GetAccountDetailsActionModel model)
        {
            dynamic result = (dynamic)await _tracfone.GetAccountDetails(1, 1000);
            return View(result);
        }

        public async Task<IActionResult> GetBalance(GetBalanceActionModel model)
        {
            var result = await _tracfone.GetBalance(model.PhoneNumber);
            return Json(result);
        }

        public async Task<IActionResult> ChangeSIM(PortActionModel model)
        {
            var result = await _tracfone.ChangeSIM(model);
            return Json(result["status"]["message"].ToString());
        }

        public async Task<IActionResult> DeactivateAndRetaineDays(DeleteActionModel model)
        {
            if (model.UseLine)
            {
                var deviceDetails = await _tracfone.GetDeviceDetails(model.Serial, "LINE");
                var serial = deviceDetails.resource.physicalResource.serialNumber;
                model.Serial = serial;
            }
            var result = await _tracfone.DeactivateAndRetaineDays(model.Serial);
            return Json(result["status"]["message"].ToString());
        }

        public async Task<IActionResult> DeactivatePastDue(DeleteActionModel model)
        {
            if (model.UseLine)
            {
                var deviceDetails = await _tracfone.GetDeviceDetails(model.Serial, "LINE");
                var serial = deviceDetails.Resource.PhysicalResource.SerialNumber;
                model.Serial = serial;
            }
            var result = await _tracfone.DeactivatePastDue(model.Serial);
            return Json(result["status"]["message"].ToString());
        }

        public async Task<IActionResult> Reactivate(DeleteActionModel model)
        {
            if (model.UseLine)
            {
                var deviceDetails = await _tracfone.GetDeviceDetails(model.Serial, "LINE");
                var serial = deviceDetails.resource.physicalResource.serialNumber;
                model.Serial = serial;
            }
            var result = await _tracfone.Reactivate(model.Serial);
            return Json(result);
        }

        public async Task<IActionResult> ExecuteBulk()
        {
            var repo = new Repository(_connectionString);
            var bulkData = repo.GetAllBulkData();

            var results = new List<dynamic>();

            foreach (BulkData data in bulkData)
            {
                if (!data.Done)
                {
                    switch (data.Action)
                    {
                        case BulkAction.AddDevice:
                            var addDeviceModel = new AddDeviceActionModel
                            {
                                Serial = data.Serial,
                                Sim = data.Sim
                            };
                            string addDeviceResults = "";

                            //BYOP Eligibility
                            dynamic byopEligibilityResult = null;
                            for (int i = 0; i < 3; i++)
                            {
                                byopEligibilityResult = await _tracfone.CheckBYOPEligibility(addDeviceModel.Serial);
                            }
                            string byopEligibilityStatus = byopEligibilityResult["status"]["code"].ToString();

                            if (byopEligibilityStatus != "0" && byopEligibilityStatus != "10008" && byopEligibilityStatus != "11023")
                            {
                                byopEligibilityResult["request"] = "BYOP Eligibility Failed";
                                addDeviceResults += JsonConvert.SerializeObject(byopEligibilityResult);
                                data.response = addDeviceResults;
                                results.Add(byopEligibilityResult);
                                break;
                            }

                            // Skip BYOP Registration if already added
                            if (byopEligibilityStatus == "10008" || byopEligibilityStatus == "11023")
                            {
                                //Add Device
                                var addDeviceResult2 = await _tracfone.AddDevice(addDeviceModel.Serial);
                                addDeviceResults += JsonConvert.SerializeObject(addDeviceResult2);
                                data.response = addDeviceResults;
                                results.Add(addDeviceResult2);
                                break;
                            }

                            //BYOP Registration
                            dynamic byopRegistrationResult = await _tracfone.BYOPRegistration(addDeviceModel);
                            string byopRegistrationStatus = byopRegistrationResult["status"]["code"].ToString();

                            if (byopRegistrationStatus != "0")
                            {
                                byopRegistrationResult["request"] = "BYOP Registration Failed";
                                addDeviceResults += JsonConvert.SerializeObject(byopRegistrationResult);
                                data.response = byopEligibilityResult;
                                results.Add(byopRegistrationResult);
                                break;
                            }

                            //Add Device
                            var addDeviceResult = await _tracfone.AddDevice(addDeviceModel.Serial);
                            addDeviceResults += JsonConvert.SerializeObject(addDeviceResult);
                            data.response = addDeviceResults;

                            results.Add(addDeviceResults);

                            break;
                        case BulkAction.DeleteDevice:
                            var deleteSerial = data.Serial;

                            var deleteResp = await _tracfone.DeleteDevice(deleteSerial);
                            data.response = JsonConvert.SerializeObject(deleteResp);
                            results.Add(deleteResp);
                            break;
                        case BulkAction.Activate:
                            var activateModel = new ActivateActionModel
                            {
                                Serial = data.Serial,
                                Sim = data.Sim,
                                Zip = data.Zip
                            };

                            var activateResp = await _tracfone.Activate(activateModel);
                            data.response = JsonConvert.SerializeObject(activateResp);
                            results.Add(activateResp);
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

                            var iportResp = await _tracfone.InternalPort(internelPortModel);
                            data.response = JsonConvert.SerializeObject(iportResp);
                            results.Add(iportResp);
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

                            var eportResp = await _tracfone.ExternalPort(externelPortModel);
                            data.response = JsonConvert.SerializeObject(eportResp);
                            results.Add(eportResp);
                            break;
                        case BulkAction.ChangeSIM:
                            var changeSIMModel = new PortActionModel
                            {
                                Serial = data.Serial,
                                Sim = data.Sim,
                                Zip = data.Zip
                            };

                            var changeSIMResp = await _tracfone.ChangeSIM(changeSIMModel);
                            data.response = JsonConvert.SerializeObject(changeSIMResp);
                            results.Add(changeSIMResp);
                            break;
                        case BulkAction.DeactivateAndRetaineDays:
                            var deactivateAndRetaineDaysReponse = await _tracfone.DeactivateAndRetaineDays(data.Serial);
                            data.response = JsonConvert.SerializeObject(deactivateAndRetaineDaysReponse);
                            results.Add(deactivateAndRetaineDaysReponse);
                            break;
                        case BulkAction.DeactivatePastDue:
                            var deactivatePastDueResponse = await _tracfone.DeactivatePastDue(data.Serial);
                            data.response = JsonConvert.SerializeObject(deactivatePastDueResponse);
                            results.Add(deactivatePastDueResponse);
                            break;
                        case BulkAction.Reactivate:
                            var reactivateResponse = await _tracfone.Reactivate(data.Serial); ;
                            data.response = JsonConvert.SerializeObject(reactivateResponse);
                            results.Add(reactivateResponse);
                            break;
                        case BulkAction.GetDeviceDetails:
                            var deviceDetailsResponse = await _tracfone.GetDeviceDetails(data.ResourceIdentifier, data.ResourceType);
                            data.response = JsonConvert.SerializeObject(deviceDetailsResponse);
                            results.Add(deviceDetailsResponse);
                            break;
                    }
                }
            }
            repo.WriteAllResponse(bulkData);
            return Json(results);
        }

        public async Task<IActionResult> GetPaymentDetails(PaymentDetailsModel model)
        {
            return Json(await _tracfone.GetPaymentSourceDetails(model.Id));
        }

        public async Task<IActionResult> GetDeviceDetails(GetDeviceModel model)
        {
            return Json(await _tracfone.GetDeviceDetails(model.Id, model.Type));
        }

        public async Task<IActionResult> GetPaymentSources()
        {
            return Json(await _tracfone.GetPaymetSources());
        }
    }
}