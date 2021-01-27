using Coneckt.Web.Models;
using Conneckt.Data;
using Microsoft.Extensions.Configuration;
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
    //This class has all the functions to call tracfone APIs(besides auth)
    //All the data that doesn't change gets populated in these functions
    //All the data that does change should be passed in through the controller
    public class Tracfone
    {
        private TracfoneAuthorizations _authorizations;
        private string _email;
        private string _password;
        private string _clientID;
        private string _jwtClientID;

        public Tracfone(IConfiguration configuration)
        {
            _authorizations = new TracfoneAuthorizations(configuration);
            _email = configuration["Credentials:username"];
            _password = configuration["Credentials:username"];
            _clientID = configuration["Credentials:clientID"];
            _jwtClientID = configuration["Credentials:jwtClientID"];
        }

        public async Task<dynamic> CheckBYOPEligibility(string serial)
        {
            var url = "api/service-qualification-mgmt/v1/service-qualification";
            var auth = await _authorizations.GetServiceQualificationMgmt();
            var data = new BYOPEligibiltyData
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
                        SerialNumber=serial,
                        Name="productSerialNumber",
                        Type="HANDSET"
                    }
                }
            };

            var response = await TracfoneAPI.PostAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}", data);
            var responseData = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(responseData);
        }

        public async Task<dynamic> BYOPRegistration(AddDeviceActionModel model)
        {
            var url = $"api/resource-mgmt/v1/resource?client_id={_clientID}";
            var auth = await _authorizations.GetResourceMgmt();
            var data = new BYOPRegistrationData
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

            var response = await TracfoneAPI.PostAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}", data);
            var responseData = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(responseData);
        }

        public async Task<dynamic> AddDevice(string serial)
        {
            var url = $"api/customer-mgmt/addDeviceToAccount?client_id={_jwtClientID}";
            var auth = await _authorizations.GetCustomerMgmtJWT();
            var data = new AddDeviceData
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
                                     Value= _email
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
                                    ProductSerialNumber=serial,
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

            var response = await TracfoneAPI.PostAPIResponse(url, $"Bearer {auth.AccessToken}", data);
            var responseData = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(responseData);
        }

        public async Task<dynamic> DeleteDevice(string serial)
        {
            var url = $"api/customer-mgmt/deleteDeviceAccount?client_id={_jwtClientID}";
            var auth = await _authorizations.GetCustomerMgmtJWT();
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
                                     Value= _email
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
                                    ProductSerialNumber=serial,
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

            var response = await TracfoneAPI.PostAPIResponse(url, $"Bearer {auth.AccessToken}", data);
            var responseData = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(responseData);
        }

        public async Task<dynamic> EstimateOrder(string loginCookie, ActivateActionModel model)
        {
            var url = $"api/order-mgmt/v1/productorder/estimate?client_id={_clientID}";
            var auth = await _authorizations.GetOrderMgmt();
            var data = new ServiceData
            {
                RelatedParties = new List<RelatedParty>
                {
                    new RelatedParty
                    {
                        RoleType="buyerOrg",
                        Party=new Party
                        {
                            PartyID="AMZ1FDFDKMDFSD",
                            LanguageAbility="ENG",
                            Organization=new Organization
                            {
                                OrganizationName= "TRAC1432"
                            },
                            PartyExtension=new List<Extension>
                            {
                                new Extension
                                {
                                    Name="accountEmail",
                                    Value=""
                                },
                                new Extension
                                {
                                    Name="sourceSystem",
                                    Value="EBP"
                                },
                                new Extension
                                {
                                    Name="UserIdentityToken",
                                    Value=loginCookie
                                },
                                new Extension
                                {
                                    Name="Channel",
                                    Value="B2B"
                                }
                            }
                        }
                    },
                    new RelatedParty
                    {
                        RoleType="customer",
                        Party=new Party
                        {
                            LanguageAbility="ENG"
                        }
                    }
                },
                ExternalID = "ASKNN123N2334123",
                OrderDate = "2018-11-16T16:42:23-04:00",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Action="ESTIMATE",
                        ID="1",
                        ProductOffering=new ProductOffering
                        {
                            ID="12554",
                            Category="Unlimited talk, text, data with the first 2GB at high-speed then 2G*",
                            ProductSpecification=new Specification
                            {
                                Brand="CLEARWAY"
                            },
                            SupportingResources=new List<SupportingResource>
                            {
                                new SupportingResource
                                {
                                    SerialNumber=model.Serial,
                                    ResourceType="HANDSET"
                                }
                            },
                            CharacteristicSpecification=new List<Extension>
                            {
                                new Extension
                                {
                                    Name="FULFILLMENT_TYPE",
                                    Value="NOW"
                                },
                                new Extension
                                {
                                    Name="TYPE",
                                    Value="UPSELL"
                                },
                                new Extension
                                {
                                    Name="AUTOREFIL",
                                    Value="TRUE"
                                }
                            }
                        },
                        Quantity=1
                    }
                },
                Location = new List<Location>
                {
                    new Location
                    {
                        AddressType="BILLING",
                        Address=new PostalAddress
                        {
                            Zipcode=model.Zip
                        }
                    }
                }
            };

            return TracfoneAPI.PostAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}", data, loginCookie);
        }

        public async Task<dynamic> SubmitOrder(string loginCookie, ActivateActionModel model, dynamic estimate, PaymentMean paymentMean)
        {
            var url = $"api/order-mgmt/v2/productorder?client_id={_clientID}";
            var auth = await _authorizations.GetOrderMgmt();
            var data = new OrderData
            {
                Request = new Request
                {
                    ID = estimate.Id,
                    ExternalID = "34972507",
                    Location = new List<Location>
                    {
                        new Location
                        {
                            AddressType="BILLING",
                            Address=new PostalAddress
                            {
                                Zipcode= model.Zip
                            }
                        }
                    },
                    RelatedParties = new List<RelatedParty>
                    {
                        new RelatedParty
                        {
                            Party=new Party
                            {
                                PartyID="CUST_HASH"
                            },
                            RoleType="customer"
                        },
                        new RelatedParty
                        {
                            Party=new Party
                            {
                                Individual=new Individual
                                {
                                    PersonalizationId=estimate.relatedParties[0].party.partyExtension[1].value
                                },
                                PartyExtension=new List<Extension>
                                {
                                    new Extension
                                    {
                                        Name="partySignature",
                                        Value=estimate.relatedParties[0].party.partyExtension[0].value
                                    }
                                }
                            },
                            RoleType="buyerAdmin"
                        },
                        new RelatedParty
                        {
                            Party=new Party
                            {
                                PartyID="CLEARWAYWEB",
                                LanguageAbility="ENG",
                                PartyExtension=new List<Extension>
                                {
                                    new Extension
                                    {
                                        Name="partyTransactionID",
                                        Value="web_1231234234424"
                                    },
                                    new Extension
                                    {
                                        Name="sourceSystem",
                                        Value="EBP"
                                    },
                                    new Extension
                                    {
                                        Name="language",
                                        Value= "ENG"
                                    },
                                    new Extension
                                    {
                                        Name="BrandName",
                                        Value= "CLEARWAY"
                                    },
                                    new Extension
                                    {
                                        Name="Channel",
                                        Value="B2B"
                                    }
                                }
                            },
                            RoleType="internal"
                        }
                    },
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem2
                        {
                            ID="9030716",
                            Quantity="1",
                            Location=new Location
                            {
                                PostalAddress=new PostalAddress
                                {
                                    Zipcode=model.Zip
                                }
                            },
                            Action="NEW",
                            ProductOffering=new ProductOffering2
                            {
                                ID="12554",
                                Category="Unlimited talk, text, data with the first 2GB at high-speed then 2G*",
                                ProductSpecification=new List<Specification>
                                { new Specification
                                {
                                    Brand="CLEARWAY"
                                }
                                },
                                SupportingResources=new List<SupportingResource>
                                {
                                    new SupportingResource
                                {
                                    ResourceType="HANDSET",
                                    SerialNumber=model.Serial
                                }
                                }
                            },
                            CharacteristicSpecification=new List<Extension>
                            {
                                new Extension
                                {
                                    Name="FULFILLMENT_TYPE",
                                    Value="NOW"
                                }
                            },
                           OrderItemPrice=new OrderPrice
                           {
                               Price=new Price
                               {
                                   Amount=estimate.orderItems[0].orderItemPrice.price.amount,
                                   CurrencyCode="USD"
                               }
                           },
                           Product=new Product
                           {
                               ProductSpecification=new Specification
                               {
                                   Brand="CLEARWAY"
                               },
                               RelatedServices=new List<RelatedService>
                               {
                                   new RelatedService
                                   {
                                       Category="SERVICE_PLAN",
                                       IsRedeemNow=false
                                   }
                               }
                           }
                        }
                    },
                    OrderPrice = new OrderPrice
                    {
                        Price = new Price
                        {
                            Amount = estimate.orderPrice.tax.totalAmountWithDiscountAndTax,
                            CurrencyCode = "USD"
                        }
                    },
                    BillingAccount = new BillingAccount
                    {
                        PaymentPlan = new PaymentPlan
                        {
                            PaymentMean = new PaymentMean
                            {
                                ID = model.PaymentMeanID,
                                AccountHolderName = "test",
                                FirstName = paymentMean.FirstName,
                                LastName = paymentMean.LastName,
                                IsDefault = true,
                                IsExisting = "FALSE",
                                Type = "CREDITCARD",
                                CreditCard = new CreditCard
                                {
                                    Type = paymentMean.CreditCard.Type,
                                    Year = paymentMean.CreditCard.Year,
                                    Month = paymentMean.CreditCard.Month,
                                    Cvv = model.CVV
                                },
                                BillingAddress = model.BillingAddress
                            }
                        }
                    }
                }
            };

            return TracfoneAPI.PostAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}", data, loginCookie);
        }

        public async Task<string> Login()
        {
            var url = "api/ecomm/customer-mgmt/v2/customer/login";
            var auth = await _authorizations.GetEcomm();
            var data = new AddDeviceData
            {
                RelatedParties = new List<RelatedParty>
                {
                    new RelatedParty
                    {
                        Party=new Party
                        {
                            PartyExtension=new List<Extension>
                            {
                                new Extension
                                {
                                    Name= "sourceSystem",
                                    Value= "EBP"
                                },
                                new Extension
                                {
                                    Name="BrandName",
                                    Value= "CLEARWAY"
                                },
                                new Extension
                                {
                                    Name="Channel",
                                    Value="B2B"
                                }
                            },
                            PartyID="Web"
                        },
                        RoleType="partner"
                    }
                },
                CustomerAccounts = new List<CustomerAccount>
                {
                    new CustomerAccount
                    {
                       Brand="CLEARWAY",
                       CustomerAccountExtension=new List<Extension>
                       {
                           new Extension
                           {
                               Name="password",
                               Value=_password
                           },
                           new Extension
                           {
                               Name="username",
                               Value=_email
                           }
                       }
                    }
                }
            };

            HttpResponseMessage response = await TracfoneAPI.PostAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}", data);
            var headers = response.Headers;
            var setCookies = headers.GetValues("Set-Cookie");
            var cookie = setCookies.FirstOrDefault(s => s.StartsWith("LtpaToken2"));
            return cookie;
        }

        public async Task<dynamic> Activate(ActivateActionModel model)
        {
            var url = $"api/order-mgmt/v1/serviceorder?client_id={_clientID}";
            var auth = await _authorizations.GetOrderMgmt();
            var data = new ServiceData
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
                                    Value =_email
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

            var response = await TracfoneAPI.PostAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}", data);
            var responseData = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(responseData);
        }

        public async Task<dynamic> ExternalPort(PortActionModel model)
        {
            var url = $"api/order-mgmt/v1/serviceorder?client_id={_clientID}";
            var auth = await _authorizations.GetOrderMgmt();
            var data = new ServiceData
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
                                    Value = _email
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
                        Value = model.CurrentMIN
                    }
                }
              }
            }
            };

            var response = await TracfoneAPI.PostAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}", data);
            var responseData = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(responseData);
        }

        public async Task<dynamic> InternalPort(PortActionModel model)
        {
            var url = $"api/order-mgmt/v1/serviceorder?client_id={_clientID}";
            var auth = await _authorizations.GetOrderMgmt();
            var data = new ServiceData
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
                                    Value = _email
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
                        Value = model.Zip
                    },
                    new Extension
                    {
                        Name = "currentFullName",
                        Value = "Cyber Source"
                    },
                    new Extension
                    {
                        Name = "contactPhone",
                        Value = model.CurrentMIN
                    }
                }
              }
            }
            };

            var response = await TracfoneAPI.PostAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}", data);
            var responseData = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(responseData);
        }

        public async Task<dynamic> GetAccountDetails(int offset, int limit)
        {
            var url = $"api/customer-mgmt/account/{_email}" +
                            "?brand=CLEARWAY" +
                            "&source=EBP" +
                            "&channel=WEB" +
                            $"&offset={offset}" +
                            $"&limit={limit}" +
                            "&order-by=desc" +
                            $"&client_id={_jwtClientID}" +
                            $"&email={_email}";
            var auth = await _authorizations.GetCustomerMgmtJWT();

            var response = await TracfoneAPI.GetAPIResponse(url, $"Bearer {auth.AccessToken}");
            return response;
        }

        public async Task<dynamic> GetBalance(string phoneNumber)
        {
            var url = $@"api/service-mgmt/v1/service/balance" +
                            $"?client_id={_jwtClientID}" +
                            "&type=LINE" +
                            $"&identifier={phoneNumber}" +
                            "&sourceSystem=EBP" +
                            "&brandName=Clearway" +
                            "&language=ENG ";
            var auth = await _authorizations.GetServiceMgmtJWT();

            return await TracfoneAPI.GetAPIResponse(url, $"Bearer {auth.AccessToken}");
        }

        public async Task<dynamic> ChangeSIM(PortActionModel model)
        {
            var url = $"api/order-mgmt/v2/serviceorder?client_id={_clientID}";
            var auth = await _authorizations.GetOrderMgmt();
            var data = new ServiceData
            {
                ExternalID = "123456",
                OrderDate = "2017-08-10T12:18:46-0400",
                RelatedParties = new List<RelatedParty>
                {
                    new RelatedParty
                    {
                        Party=new Party
                        {
                            PartyID="vendor name",
                            LanguageAbility="ENG",
                            PartyExtension=new List<Extension>
                            {
                                new Extension
                                {
                                    Name="partyTransactionID",
                                    Value="1231234234424"
                                },
                                new Extension
                                {
                                    Name="sourceSystem",
                                    Value="EBP"
                                }
                            }
                        },
                        RoleType= "partner"
                    }
                },
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Action="SIM_CHANGE",
                        ID="1",
                        Quantity=1,
                        Product=new Product
                        {
                            ProductSerialNumber=model.Serial,
                            ProductCategory="HANDSET",
                            ProductSpecification=new Specification
                            {
                                Brand= "CLEARWAY"
                            },
                            SupportingResources=new List<SupportingResource>
                            {
                                new SupportingResource
                                {
                                    ResourceType="SIM_CARD",
                                    SerialNumber=model.Sim
                                }
                            }
                        },
                        OrderItemExtension=new List<Extension>
                        {
                            new Extension
                            {
                                Name="currentMin",
                                Value= model.CurrentMIN
                            }
                        }
                    }
                },
                Location = new Location
                {
                    PostalAddress = new PostalAddress
                    {
                        Zipcode = model.Zip
                    }
                }
            };

            var response = await TracfoneAPI.PostAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}", data);
            var responseData = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(responseData);
        }

        public async Task<dynamic> DeactivateAndRetaineDays(string serial)
        {
            var url = $"api/order-mgmt/v2/serviceorder?client_id={_clientID}";
            var auth = await _authorizations.GetOrderMgmt();
            var data = new ServiceData
            {
                OrderDate = "2016-04-16T16:42:23-04:00",
                RelatedParties = new List<RelatedParty>
                {
                    new RelatedParty
                    {
                        RoleType="partner",
                        Party=new Party
                        {
                            PartyExtension=new List<Extension>
                            {
                                new Extension
                                {
                                    Name="partyTransactionID",
                                    Value="28fed77a-aa9e-4738-af47-e3db34fc7b58"
                                },
                                new Extension
                                {
                                    Name="sourceSystem",
                                    Value= "EBP"
                                }
                            },
                            PartyID= "Approved Link",
                            LanguageAbility= "ENG",
                        }
                    }
                },
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ID="1",
                        Product=new Product
                        {
                            SubCategory="BRANDED",
                            ProductCategory="HANDSET",
                            ProductSpecification=new Specification
                            {
                                Brand="CLEARWAY"
                            },
                            ProductSerialNumber=serial,
                        },
                        Action="DEACTIVATION",
                        Location=new Location
                        {
                            PostalAddress=new PostalAddress
                            {
                                Zipcode="31088"
                            }
                        },
                        OrderItemExtension=new List<Extension>
                        {
                            new Extension
                            {
                                Name= "reason",
                                Value="CUSTOMER REQD"
                            }
                        }
                    }
                },
                ExternalID = "123"
            };

            var response = await TracfoneAPI.PostAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}", data);
            var responseData = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(responseData);
        }

        public async Task<dynamic> DeactivatePastDue(string serial)
        {
            var url = $"api/order-mgmt/v2/serviceorder?client_id={_clientID}";
            var auth = await _authorizations.GetOrderMgmt();
            var data = new ServiceData
            {
                OrderDate = "2016-04-16T16:42:23-04:00",
                RelatedParties = new List<RelatedParty>
                {
                    new RelatedParty
                    {
                        RoleType= "partner",
                        Party=new Party
                        {
                            PartyExtension=new List<Extension>
                            {
                                new Extension
                                {
                                    Name="partyTransactionID",
                                    Value="28fed77a-aa9e-4738-af47-e3db34fc7b58"
                                },
                                new Extension
                                {
                                    Name="sourceSystem",
                                    Value= "EBP"
                                }
                            },
                            PartyID= "WEB",
                            LanguageAbility= "ENG",
                        }
                    }
                },
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ID="1",
                        Product=new Product
                        {
                            SubCategory="BRANDED",
                            ProductCategory="HANDSET",
                            ProductSpecification=new Specification
                            {
                                Brand="CLEARWAY"
                            },
                            ProductSerialNumber=serial,
                        },
                        Action="DEACTIVATION",
                        Location=new Location
                        {
                            PostalAddress=new PostalAddress
                            {
                                Zipcode="33178"
                            }
                        },
                        OrderItemExtension=new List<Extension>
                        {
                            new Extension
                            {
                                Name= "reason",
                                Value="PASTDUE"
                            }
                        }
                    }
                },
                ExternalID = "123"
            };

            var response = await TracfoneAPI.PostAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}", data);
            var responseData = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(responseData);
        }

        public async Task<dynamic> GetPaymetSources()
        {
            var url = $"api/customer-mgmt/v1/customer/paymentSource" +
                $"?account_id=681177314" +
                $"&language=ENG" +
                $"&sourceSystem=EBP" +
                $"&brand=CLEARWAY" +
                $"&email={_email}" +
                $"&client_id={_jwtClientID}";
            var auth = await _authorizations.GetCustomerMgmtJWT();

            var response = await TracfoneAPI.GetPaymentAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}");
            return response;
        }

        public async Task<dynamic> GetPaymentSourceDetails(string accountIdentifier)
        {
            var url = $"api/customer-mgmt/v1/customer/paymentSourceDetail" +
                $"?accountIdentifier={accountIdentifier}" +
                $"&language=ENG" +
                $"&sourceSystem=EBP" +
                $"&brand=CLEARWAY" +
                $"&email={_email}&" +
                $"client_id={_jwtClientID}";
            var auth = await _authorizations.GetCustomerMgmtJWT();

            return await TracfoneAPI.GetAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}");
        }

        public async Task<dynamic> GetDeviceDetails(string input, string inputType)
        {
            var url = $"api/resource-mgmt/resource" +
                $"?email={_email}" +
                $"&client_id={_jwtClientID}" +
                $"&sourceSystem=EBP" +
                $"&partyID=CLEARWAY" +
                $"&brand=CLEARWAY" +
                $"&channel=B2B" +
                $"&resourceCategory={inputType}" +
                $"&type=serialNumber" +
                $"&resourceIdentifier={input}" +
                $"&lang=ENG" +
                $"&projection=account,supportingresources,plans";
            var auth = await _authorizations.GetResourceMgmtJWT();

            var result = await TracfoneAPI.GetAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}");
            return result;
        }
        public async Task<dynamic> Reactivate(string serialNumber)
        {
            var url = $"api/order-mgmt/v1/serviceorder?client_id={_clientID}";
            var auth = await _authorizations.GetOrderMgmt();
            var data = new ServiceData
            {
                OrderDate = "2016-04-16T16:42:23-04:00",
                RelatedParties = new List<RelatedParty>
                {
                    new RelatedParty
                    {
                        RoleType="partner",
                        Party=new Party
                        {
                            PartyExtension=new List<Extension>
                            {
                                new Extension
                                {
                                    Name="partyTransactionID",
                                    Value="84306270-c4cd-4142-b41a-311b63b70074"
                                },
                                new Extension
                                {
                                    Name="sourceSystem",
                                    Value="EBP"
                                }
                            },
                            PartyID="vendor name",
                            LanguageAbility="ENG"
                        }
                    },
                    new RelatedParty
                    {
                        RoleType="customer",
                        Party=new Party
                        {
                            Individual=new Individual
                            {
                                ID="681177314"
                            },
                            PartyExtension=new List<Extension>
                            {
                                new Extension
                                {
                                    Name="accountEmail",
                                    Value=_email
                                },
                                new Extension
                                {
                                    Name="accountPassword",
                                    Value=_password
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
                        Product=new Product
                        {
                            SubCategory="BRANDED",
                            ProductCategory="HANDSET",
                            ProductSpecification=new Specification
                            {
                                Brand="CLEARWAY"
                            },
                            RelatedServices=new List<RelatedService>
                            {
                                new RelatedService
                                {
                                    ID="",
                                    Category="SERVICE_PLAN"
                                }
                            },
                            ProductSerialNumber=serialNumber,
                            SupportingResources=new List<SupportingResource>
                            {
                                new SupportingResource
                                {
                                    SerialNumber=serialNumber,
                                    ResourceType="SIM_CARD"
                                }
                            }
                        },
                        ID="1",
                        Location=new Location
                        {
                            PostalAddress=new PostalAddress
                            {
                                Zipcode="31088"
                            }
                        },
                        Action="REACTIVATION"
                    }
                }
            };

            var response = await TracfoneAPI.PostAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}", data);
            var responseData = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(responseData);
        }
    }
}