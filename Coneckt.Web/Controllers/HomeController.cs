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

namespace Coneckt.Web.Controllers
{
    public class HomeController : Controller
    {
        private string _accessToken;
        private string _clientID;
        private string _username;
        private string _password;
        private string _jwtAccessToken;
        private string _jwtClientID;

        public HomeController(IConfiguration configuration)
        {
            _accessToken = configuration["Credentials:accessToken"];
            _clientID = configuration["Credentials:clientID"];
            _username = configuration["Credentials:username"];
            _password = configuration["Credentials:password"];
            _jwtAccessToken = configuration["Credentials:jwtAccessToken"];
            _jwtClientID = configuration["Credentials:jwtClientID"];
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AddDevice(AddDeviceActionModel model)
        {
            var tracfone = new Tracfone();
            //BYOP Eligibility
            var byopEligibilityAuthUrl = "api/service-qualification-mgmt/oauth/token?grant_type=client_credentials&scope=/service-qualification-mgmt";

            dynamic byopEligibilityAuth = await tracfone.PostAPIResponse(byopEligibilityAuthUrl, _accessToken);

            var byopEligibilityUrl = "api/service-qualification-mgmt/v1/service-qualification";
            var byopEligibilityData = new BYOPEligibiltyData
            {
                RelatedParties = new List<RelatedParty>
                {
                    new RelatedParty
                    {
                        Party=new Party
                        {
                            PartyID="Approved Link",
                            LanguageAbility= "ENG",
                            PartyExtension=new List<Extension>
                            {
                                new Extension
                                {
                                    Name="partyTransactionID",
                                    Value="12345"
                                },
                                new Extension
                                {
                                    Name="sourceSystem",
                                    Value="EBP"
                                }
                            }
                        },
                        RoleType="partner"
                    }
                },
                Location = new Location
                {
                    PostalAddress = new PostalAddress
                    {
                        Zipcode = "33178"
                    }
                },
                ServiceCategory = new List<ServiceCategory>
                {
                    new ServiceCategory
                    {
                        Type="context",
                        Value="BYOP_ELIGIBILITY"
                    }
                },
                ServiceSpecification = new Specification
                {
                    Brand = "CLEARWAY"
                },
                Service = new Service
                {
                    Carrier = new List<Extension>
                    {
                        new Extension
                        {
                             Name="carrierName",
                             Value="VZW"
                        }
                    }
                },
                RelatedResources = new List<RelatedResource>
                {
                    new RelatedResource
                    {
                        SerialNumber=model.Serial,
                        Name="productSerialNumber",
                        Type="HANDSET"
                    }
                }
            };
            var byopEligibilityResult = "";
            for (int i = 0; i < 3; i++)
            {
                byopEligibilityResult = await tracfone.PostAPIResponse(byopEligibilityUrl, $"{byopEligibilityAuth.token_type} {byopEligibilityAuth.access_token}", byopEligibilityData);
            }
            JObject jEligibilityObj = JObject.Parse(byopEligibilityResult);
            string byopEligibilityStatus = jEligibilityObj["status"]["code"].ToString();

            if (byopEligibilityStatus != "200")
            {
                return Json(byopEligibilityResult);
            }

            //BYOP Registration

            //BYOP Registration Auth
            var byopRegistrationAuthUrl = "api/resource-mgmt/oauth/token?grant_type=client_credentials&scope=/resource-mgmt";
            dynamic byopRegistrationAuth = await tracfone.PostAPIResponse(byopRegistrationAuthUrl, _accessToken);

            var byopRegistrationUrl = $"api/resource-mgmt/v1/resource?client_id={_clientID}";
            var byopRegistrationData = new BYOPRegistrationData
            {
                RelatedParties = new List<RelatedParty>
                {
                    new RelatedParty
                    {
                        Party = new Party
                        {
                            PartyID = "Approved Link",
                            LanguageAbility = "ENG",
                            PartyExtension = new List<Extension>
                            {
                                new Extension
                                {
                                    Name = "partyTransactionID",
                                    Value = "1231234234424"
                                },
                                new Extension
                                {
                                    Name = "sourceSystem",
                                    Value = "EBP"
                                }
                            }
                        },
                        RoleType = "partner"
                    }
                },
                Resource = new Resource
                {
                    Location = new Location
                    {
                        PostalAddress = new PostalAddress
                        {
                            Zipcode = "33178"
                        }
                    },
                    Association = new Association
                    {
                        Role = "REGISTER"
                    },
                    ResourceSpecification = new Specification
                    {
                        Brand = "CLEARWAY"
                    },
                    PhysicalResource = new PhysicalResource
                    {
                        ResourceCategory = "HANDSET",
                        ResourceSubCategory = "BYOP",
                        SerialNumber = model.Serial,
                        supportingResources = new List<SupportingResource>
                        {
                            new SupportingResource
                            {
                                ResourceCategory="SIM_SIZE",
                                ResourceIdentifier=""
                            },
                            new SupportingResource
                            {
                                ResourceCategory="SIM_CARD",
                                ResourceIdentifier=model.Sim
                            }
                        }
                    },
                    SupportingLogicalResources = new List<SupportingResource>
                    {
                        new SupportingResource
                        {
                            ResourceCategory="CARRIER",
                            ResourceIdentifier="VZW"
                        }
                    }
                }
            };
            var byopRegistrationResult = await tracfone.PostAPIResponse(byopRegistrationUrl, $"{byopRegistrationAuth.token_type} {byopRegistrationAuth.access_token}", byopRegistrationData);
            JObject jRegistrationObj = JObject.Parse(byopRegistrationResult);
            string byopRegistrationStatus = jRegistrationObj["status"]["code"].ToString();

            if (byopRegistrationStatus != "200")
            {
                return Json(byopRegistrationResult);
            }

            //Add Device
            var addDeviceUrl = $"api/customer-mgmt/addDeviceToAccount?client_id={_jwtClientID}";
            dynamic addDeviceAuth = await tracfone.GetJWTAuthorization(_username, _password, _jwtAccessToken);
            var addDeviceData = new AddDeviceData
            {
                RelatedParties = new List<RelatedParty>
                {
                    new RelatedParty
                    {
                        Party=new Party
                        {
                            PartyID= "Approved Link",
                            LanguageAbility= "ENG",
                            PartyExtension=new List<Extension>
                            {
                                new Extension
                                {
                                    Name= "vendorName",
                                    Value= "Approved Link"
                                },
                                new Extension
                                {
                                    Name= "vendorStore",
                                    Value= "1231234234424"
                                },
                                new Extension
                                {
                                    Name="vendorTerminal",
                                    Value= "1231234234424"
                                },
                                new Extension
                                {
                                    Name= "sourceSystem",
                                    Value= "EBP"
                                },
                                new Extension
                                {
                                     Name= "accountEmail",
                                     Value= _username
                                },
                                new Extension
                                {
                                    Name= "partyTransactionID",
                                    Value= "indirect_1231234234424"
                                }
                            }
                        },
                        RoleType="PARTNER"
                     }
                },
                CustomerAccounts = new List<CustomerAccount>
                {
                    new CustomerAccount
                    {
                        Action="ADD_DEVICE",
                        CustomerProducts=new List<CustomerProduct>
                        {
                            new CustomerProduct
                            {
                                Product=new Product
                                {
                                    ProductSerialNumber=model.Serial,
                                    ProductStatus= "ACTIVE",
                                    AccountId= "681177314",
                                    ProductCategory= "HANDSET",
                                    ProductSpecification=new Specification
                                    {
                                        Brand="CLEARWAY"
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var addDeviceResult = await tracfone.PostAPIResponse(addDeviceUrl, $"Bearer {addDeviceAuth.access_token}", addDeviceData);

            return Json(addDeviceResult);
        }

        public async Task<IActionResult> DeleteDevice(DeleteActionModel model)
        {
            var tracfone = new Tracfone();

            var url = $"api/customer-mgmt/deleteDeviceAccount?client_id={_jwtClientID}";
            var auth = await tracfone.GetJWTAuthorization(_username, _password, _jwtAccessToken);
            var data = new AddDeviceData
            {
                RelatedParties = new List<RelatedParty>
                {
                    new RelatedParty
                    {
                        Party=new Party
                        {
                            PartyID= "",
                            LanguageAbility= "ENG",
                            PartyExtension=new List<Extension>
                            {
                                new Extension
                                {
                                    Name= "vendorName",
                                    Value= "1231234234424"
                                },
                                new Extension
                                {
                                    Name= "vendorStore",
                                    Value= "1231234234424"
                                },
                                new Extension
                                {
                                    Name="vendorTerminal",
                                    Value= "1231234234424"
                                },
                                new Extension
                                {
                                    Name= "sourceSystem",
                                    Value= "EBP"
                                },
                                new Extension
                                {
                                     Name= "accountEmail",
                                     Value= _username
                                },
                                new Extension
                                {
                                    Name= "partyTransactionID",
                                    Value= "indirect_1231234234424"
                                }
                            }
                        },
                        RoleType="partner"
                     }
                },
                CustomerAccounts = new List<CustomerAccount>
                {
                    new CustomerAccount
                    {
                        Action="DELETE_DEVICE",
                        CustomerProducts=new List<CustomerProduct>
                        {
                            new CustomerProduct
                            {
                                Product=new Product
                                {
                                    ProductSerialNumber=model.Serial,
                                    ProductStatus= "ACTIVE",
                                    AccountId= "681177314",
                                    ProductCategory= "HANDSET",
                                    ProductSpecification=new Specification
                                    {
                                        Brand="CLEARWAY"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var result = await tracfone.PostAPIResponse(url, $"Bearer {auth.access_token}", data);
            return Json(result);
        }

        public async Task<IActionResult> Activate(ActivateActionModel model)
        {
            Tracfone tracfone = new Tracfone();
            var url = $"api/order-mgmt/v1/serviceorder?client_id={_clientID}";
            var auth = await tracfone.GetOrderMgmtAuthorization(_accessToken);
            var activateData = new ServiceData
            {
                OrderDate = "2016-04-16T16:42:23-04:00",
                RelatedParties = new List<RelatedParty>
                {
                    new RelatedParty
                    {
                        RoleType = "partner",
                        Party=new Party
                        {
                            PartyExtension=new List<Extension>
                            {
                                new Extension
                                {
                                    Name = "partyTransactionID",
                                    Value = "84306270-c4cd-4142-b41a-311b63b70074"
                                },
                                new Extension
                                {
                                    Name = "sourceSystem",
                                    Value = "EBP"
                                }
                            },
                            PartyID = "vendor name",
                            LanguageAbility = "ENG",
                        }
                    },
                    new RelatedParty
                    {
                        RoleType = "customer",
                        Party =new Party
                        {
                            PartyExtension=new List<Extension>
                            {
                                new Extension
                                {
                                    Name = "accountEmail",
                                    Value =_username
                                }
                            },
                            Individual = new Individual
                            {
                                ID = "681177314"
                            }
                        }
                    }
                },
                ExternalID = "123",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                {
                Product = new Product
                {
                    SubCategory = "BRANDED",
                    ProductCategory = "HANDSET",
                    ProductSpecification = new Specification
                    {
                        Brand = "CLEARWAY"
                    },
                    RelatedServices = new List<RelatedService>
                    {
                        new RelatedService
                        {
                            ID="",
                            Category="SERVICE_PLAN"
                        }
                    },
                    ProductSerialNumber = model.Serial,
                    SupportingResources = new List<SupportingResource>
                    {
                        new SupportingResource
                        {
                             SerialNumber=model.Sim,
                             ResourceType="SIM_CARD"
                        }
                    }
                },
                ID = "1",
                Location = new Location
                {
                    PostalAddress = new PostalAddress
                    {
                        Zipcode = model.Zip
                    }
                },
                Action = "ACTIVATION"
            }
                }
            };
            var result = await tracfone.PostAPIResponse(url, $"{auth.token_type} {auth.access_token}", activateData);
            return Json(result);
        }

        public async Task<IActionResult> ExternalPort(PortActionModel model)
        {
            Tracfone tracfone = new Tracfone();
            var url = $"api/order-mgmt/v1/serviceorder?client_id={_clientID}";
            var auth = await tracfone.GetOrderMgmtAuthorization(_accessToken);
            var portData = new ServiceData
            {
                OrderDate = "2016-04-16T16:42:23-04:00",
                RelatedParties = new List<RelatedParty>
                {
                    new RelatedParty
                    {
                        RoleType = "partner",
                        Party=new Party
                        {
                            PartyExtension=new List<Extension>
                            {
                                new Extension
                                {
                                    Name = "partyTransactionID",
                                    Value = "84306270-c4cd-4142-b41a-311b63b70074"
                                },
                                new Extension
                                {
                                    Name = "sourceSystem",
                                    Value = "EBP"
                                },
                                new Extension
                                {
                                    Name="vendorStore",
                                    Value="302"
                                },
                                new Extension
                                {
                                    Name="vendorTerminal",
                                    Value="302"
                                }
                            },
                            PartyID = "Approvedlink",
                            LanguageAbility = "ENG",
                        }
                    },
                    new RelatedParty
                    {
                        RoleType = "customer",
                        Party =new Party
                        {
                            Individual = new Individual
                            {
                                ID = "681177314"
                            },
                            PartyExtension=new List<Extension>
                            {
                                new Extension
                                {
                                    Name = "accountEmail",
                                    Value = _username
                                },
                                new Extension
                                {
                                     Name= "accountPassword",
                                     Value= ""
                                }
                            }
                        }
                    }
                },
                ExternalID = "123",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                {
                Product = new Product
                {
                    SubCategory = "BRANDED",
                    ProductCategory = "HANDSET",
                    ProductSpecification = new Specification
                    {
                        Brand = "CLEARWAY"
                    },
                    ProductCharacteristics=new List<Extension>
                    {
                        new Extension
                        {
                             Name= "manufacturer",
                             Value= "APPLE"
                        },
                        new Extension
                        {
                            Name= "model",
                            Value= "MKRD2LL/A"
                        }
                    },
                    RelatedServices = new List<RelatedService>
                    {
                        new RelatedService
                        {
                            ID="",
                            Category="SERVICE_PLAN"
                        }
                    },
                    ProductSerialNumber = model.Serial,
                    SupportingResources = new List<SupportingResource>
                    {
                        new SupportingResource
                        {
                            ProductIdentifier="",
                            ResourceType="AIRTIME_CARD"
                        },
                        new SupportingResource
                        {
                             SerialNumber=model.Sim,
                             ResourceType="SIM_CARD"
                        }
                    }
                },
                ID = "1",
                Location = new Location
                {
                    PostalAddress = new PostalAddress
                    {
                        Zipcode = model.Zip
                    }
                },
                Action = "PORT",
                OrderItemExtension=new List<Extension>
                {
                      new Extension
                      {
                          Name= "currentMIN",
                          Value= model.CurrentMIN
                },
                            new Extension
                {
                          Name= "currentServiceProvider",
                          Value= model.CurrentServiceProvider
                },
                            new Extension
                {
                          Name= "currentCarrierType",
                          Value= "Wireless"
                },
                            new Extension
                {
                         Name= "currentAccountNumber",
                         Value= model.CurrentAccountNumber
                },
                    new Extension
                {
                     Name="currentVKey",
                     Value=model.CurrentVKey
                },
                            new Extension
                {
                  Name= "houseNumber",
                  Value= "1259"
                },
                            new Extension
                {
                  Name= "currentAddressLine1",
                  Value= "Unit 1295"
                },
                            new Extension
                {
                  Name= "streetName",
                  Value= "Charleston Road"
                },
                            new Extension
                {
                  Name= "streetType",
                  Value = "RD"
                },
                            new Extension
                {
                  Name= "currentAddressCity",
                  Value= "Miami"
                },
                            new Extension
                {
                  Name= "currentAddressState",
                  Value= "FL"
                },
                    new Extension
                    {
                        Name = "currentAddressZip",
                        Value = "33178"
                    },
                    new Extension
                    {
                        Name = "currentFullName",
                        Value = "Cyber Source"
                    },
                    new Extension
                    {
                        Name = "contactPhone",
                        Value = "3479870002"
                    }
                }
              }
            }
            };

            var result = await tracfone.PostAPIResponse(url, $"{auth.token_type} {auth.access_token}", portData);

            return Json(result);
        }

        public async Task<IActionResult> InternalPort(PortActionModel model)
        {
            Tracfone tracfone = new Tracfone();
            var url = $"api/order-mgmt/v1/serviceorder?client_id={_clientID}";
            var auth = await tracfone.GetOrderMgmtAuthorization(_accessToken);
            var portData = new ServiceData
            {
                OrderDate = "2016-04-16T16:42:23-04:00",
                RelatedParties = new List<RelatedParty>
                {
                    new RelatedParty
                    {
                        RoleType = "partner",
                        Party=new Party
                        {
                            PartyExtension=new List<Extension>
                            {
                                new Extension
                                {
                                    Name = "partyTransactionID",
                                    Value = "84306270-c4cd-4142-b41a-311b63b70074"
                                },
                                new Extension
                                {
                                    Name = "sourceSystem",
                                    Value = "WEB"
                                },
                                new Extension
                                {
                                    Name="vendorStore",
                                    Value="302"
                                },
                                new Extension
                                {
                                    Name="vendorTerminal",
                                    Value="302"
                                }
                            },
                            PartyID = "Approvedlink",
                            LanguageAbility = "ENG",
                        }
                    },
                    new RelatedParty
                    {
                        RoleType = "customer",
                        Party =new Party
                        {
                            Individual = new Individual
                            {
                                ID = "681177314"
                            },
                            PartyExtension=new List<Extension>
                            {
                                new Extension
                                {
                                    Name = "accountEmail",
                                    Value = _username
                                },
                                new Extension
                                {
                                     Name= "accountPassword",
                                     Value= ""
                                }
                            }
                        }
                    }
                },
                ExternalID = "123",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                {
                Product = new Product
                {
                    SubCategory = "BRANDED",
                    ProductCategory = "HANDSET",
                    ProductSpecification = new Specification
                    {
                        Brand = "CLEARWAY"
                    },
                    ProductCharacteristics=new List<Extension>
                    {
                        new Extension
                        {
                             Name= "manufacturer",
                             Value= "APPLE"
                        },
                        new Extension
                        {
                            Name= "model",
                            Value= "MKRD2LL/A"
                        }
                    },
                    RelatedServices = new List<RelatedService>
                    {
                        new RelatedService
                        {
                            ID="",
                            Category="SERVICE_PLAN"
                        }
                    },
                    ProductSerialNumber = model.Serial,
                    SupportingResources = new List<SupportingResource>
                    {
                        new SupportingResource
                        {
                            ProductIdentifier="",
                            ResourceType="AIRTIME_CARD"
                        },
                        new SupportingResource
                        {
                             SerialNumber=model.Sim,
                             ResourceType="SIM_CARD"
                        }
                    }
                },
                ID = "1",
                Location = new Location
                {
                    PostalAddress = new PostalAddress
                    {
                        Zipcode = model.Zip
                    }
                },
                Action = "PORT",
                OrderItemExtension=new List<Extension>
                {
                      new Extension
                      {
                          Name= "currentMIN",
                          Value= model.CurrentMIN
                },
                            new Extension
                {
                          Name= "currentServiceProvider",
                          Value= model.CurrentServiceProvider
                },
                            new Extension
                {
                          Name= "currentCarrierType",
                          Value= "Wireless"
                },
                            new Extension
                {
                         Name= "currentAccountNumber",
                         Value= model.CurrentAccountNumber
                },
                            new Extension
                {
                  Name= "houseNumber",
                  Value= "1259"
                },
                            new Extension
                {
                  Name= "currentAddressLine1",
                  Value= "Unit 1295"
                },
                            new Extension
                {
                  Name= "streetName",
                  Value= "Charleston Road"
                },
                            new Extension
                {
                  Name= "streetType",
                  Value = "RD"
                },
                            new Extension
                {
                  Name= "currentAddressCity",
                  Value= "Miami"
                },
                            new Extension
                {
                  Name= "currentAddressState",
                  Value= "FL"
                },
                    new Extension
                    {
                        Name = "currentAddressZip",
                        Value = "33178"
                    },
                    new Extension
                    {
                        Name = "currentFullName",
                        Value = "Cyber Source"
                    },
                    new Extension
                    {
                        Name = "contactPhone",
                        Value = "3051380236"
                    }
                }
              }
            }
            };

            var result = await tracfone.PostAPIResponse(url, $"{auth.token_type} {auth.access_token}", portData);
            return Json(result);
        }
    }
}
