using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Linq;
using Newtonsoft.Json;

namespace Conneckt.Data
{
    public class Repository
    {
        private string _connectionString;
        public Repository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<BulkData> GetAllBulkData()
        {
            var bulkData = new List<BulkData>();
            using (var connection = new OleDbConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM bulkaction";
                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    bulkData.Add(new BulkData
                    {
                        ID = reader.Get<int>("ID"),
                        Action = (BulkAction)reader["Action"],
                        Zip = reader.Get<string>("Zip"),
                        Serial = reader.Get<string>("Serial"),
                        Sim = reader.Get<string>("Sim"),
                        CurrentMIN = reader.Get<string>("CurrentMIN"),
                        CurrentServiceProvider = reader.Get<string>("CurrentServiceProvider"),
                        CurrentAccountNumber = reader.Get<string>("CurrentAccountNumber"),
                        CurrentVKey = reader.Get<string>("CurrentVKey"),
                        Done = (bool)reader["Done"],
                        ResourceIdentifier = reader.Get<string>("ResourceIdentifier"),
                        ResourceType = reader.Get<string>("ResourceType"),
                        ProductID = reader.Get<string>("ProductID"),
                        PaymentMeanID = reader.Get<string>("PaymentMeanID"),
                        ProductName = reader.Get<string>("ProductName"),
                        CVV = reader.Get<string>("CVV"),
                        BillingAddress = new Address
                        {
                            AddressLine1 = reader.Get<string>("AddressLine1"),
                            City = reader.Get<string>("City"),
                            StateOrProvince = reader.Get<string>("StateOrProvince"),
                            Country = reader.Get<string>("Country"),
                            ZipCode = reader.Get<string>("ZipCode"),
                        }
                    });
                }

                return bulkData;
            }
        }

        public void WriteAllResponse(List<BulkData> bulkDatas)
        {
            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                foreach (BulkData data in bulkDatas)
                {
                    OleDbCommand cmd = connection.CreateCommand();
                    connection.Open();

                    if (data.Action == BulkAction.GetDeviceDetails)
                    {
                        if (data.ResponseObj != null)
                        {
                            SaveDeviceDetails(data.ResponseObj.resource);

                        }
                    }
                    if (data.response != null)
                    {
                        cmd.CommandText = $"UPDATE bulkaction SET Response = @response, Done = true WHERE ID = @id";
                        cmd.Parameters.AddRange(new OleDbParameter[]
                        {
                            new OleDbParameter("@response", data.response),
                            new OleDbParameter("@id", data.ID)
                        });
                    }
                    else
                    {
                        cmd.CommandText = $"UPDATE bulkaction SET Done = true WHERE ID = @id";
                        cmd.Parameters.AddRange(new OleDbParameter[]
                        {
                            new OleDbParameter("@id", data.ID)
                        });

                    }

                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        public void SaveDeviceDetails(dynamic data)
        {
            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                var resource = data.resource;
                var physicalResource = data.physicalResource;

                // var supportingResources2 = physicalResource.supportingResources;

                
                List<SupportingResource> supportingResources = physicalResource.supportingResources.ToObject<List<SupportingResource>>();
                var simCardResource = supportingResources.FirstOrDefault(sr => sr.ResourceCategory == "SIM_CARD");
                var lineResource = supportingResources.FirstOrDefault(sr => sr.ResourceCategory == "LINE");

                List<RelatedService> relatedServices = physicalResource.relatedServices.ToObject<List<RelatedService>>();
                var servicePlan = relatedServices.FirstOrDefault(rs => rs.Category == "SERVICE_PLAN");

                OleDbCommand cmd = connection.CreateCommand();

                
                cmd.CommandText = "INSERT INTO DEVICEDETAILS(ResourceCategory, SerialNumber, SIMCard, SIMStatus, Line, LineStatus, Carrier, PlanName, Subcategory, ValidThru, JSONResponse)";
                cmd.CommandText += "VALUES(@ResourceCategory, @SerialNumber, @SIMCard, @SIMStatus, @Line, @LineStatus, @Carrier, @PlanName, @Subcategory, @ValidThru, @JSONResponse)";
                cmd.Parameters.AddRange(new OleDbParameter[]
                       {
                           new OleDbParameter("@ResourceCategory",physicalResource.resourceCategory),
                           new OleDbParameter("@SerialNumber",physicalResource.serialNumber),
                           new OleDbParameter("@SIMCard",simCardResource.SerialNumber),
                           new OleDbParameter("@SIMStatus",simCardResource.Status),
                           new OleDbParameter("@Line",lineResource.SerialNumber),
                           new OleDbParameter("@LineStatus",lineResource.Status),
                           new OleDbParameter("@Carrier", lineResource.Carrier.Name),
                           new OleDbParameter("@PlanName",servicePlan.Name),
                           new OleDbParameter("@Subcategory",servicePlan.Subcategory),
                           new OleDbParameter("@ValidThru",servicePlan.ValidFor.EndDate),
                           new OleDbParameter("@JSONResponse", JsonConvert.SerializeObject(data))
                       });

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<ProductID> GetProductIDs()
        {
            using (var connection = new OleDbConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = "SELECT * FROM PRODUCTS";
                var reader = command.ExecuteReader();

                var productIds = new List<ProductID>();
                while (reader.Read())
                {
                    productIds.Add(new ProductID
                    {
                        ID = (string)reader["ProductId"],
                        Name = (string)reader["ProductName"]
                    });
                }
                return productIds;
            }
        }
    }

    public static class ReaderExtensions
    {
        public static T Get<T>(this OleDbDataReader reader, string name)
        {
            object value = reader[name];
            if (value == DBNull.Value)
            {
                return default(T);
            }

            return (T)value;
        }
    }
}
 