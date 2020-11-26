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
        private string _clientID;
        private string _jwtClientID;

        public Tracfone(IConfiguration configuration)
        {
            _authorizations = new TracfoneAuthorizations(configuration);
            _email = configuration["Credentials:username"];
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

            return await TracfoneAPI.PostAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}", data);
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

            return await TracfoneAPI.PostAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}", data);
        }

        public async Task<dynamic> AddDevice(string serial)
        {
            var url = $"api/customer-mgmt/addDeviceToAccount?client_id={_jwtClientID}";
            var auth = await _authorizations.GetServiceMgmtJWT();
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

            return await TracfoneAPI.PostAPIResponse(url, $"Bearer {auth.AccessToken}", data);
        }

        public async Task<dynamic> DeleteDevice(string serial)
        {
            var url = $"api/customer-mgmt/deleteDeviceAccount?client_id={_jwtClientID}";
            var auth = await _authorizations.GetServiceMgmtJWT();
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

            return await TracfoneAPI.PostAPIResponse(url, $"Bearer {auth.AccessToken}", data);
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

            return await TracfoneAPI.PostAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}", data);
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
                        Value = "3479870002"
                    }
                }
              }
            }
            };

            return await TracfoneAPI.PostAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}", data);
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

            return await TracfoneAPI.PostAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}", data);
        }

        public async Task<dynamic> GetAccountDetails(int offset, int limit)
        {
            var url = $@"api/customer-mgmt/account/{_email}
                            ?brand=CLEARWAY
                            &source=EBP
                            &channel=WEB
                            &offset={offset}
                            &limit={limit}
                            &order-by=desc
                            &client_id={_jwtClientID}
                            &email={_email}";
            var auth = await _authorizations.GetServiceMgmtJWT();

            return await TracfoneAPI.GetAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}");
        }

        public async Task<dynamic> GetBalance(string phoneNumber)
        {
            var url = $@"api/service-mgmt/v1/service/balance
                            ?client_id={_jwtClientID}
                            &type=LINE
                            &identifier={phoneNumber}
                            &sourceSystem=EBP
                            &brandName=Clearway
                            &language=ENG ";
            var auth = await _authorizations.GetServiceMgmtJWT();

            return await TracfoneAPI.GetAPIResponse(url, $"{auth.TokenType} {auth.AccessToken}");
        }
    }
}
