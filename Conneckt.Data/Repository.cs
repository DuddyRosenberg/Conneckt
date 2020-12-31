using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;

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
                        Done = (bool)reader["Done"]
                    });
                }

                return bulkData;
            }
        }

        public void WriteAllResponse(List<BulkData> bulkDatas)
        {
            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                OleDbCommand cmd = connection.CreateCommand();
                cmd.CommandText = $"UPDATE bulkaction SET Response = @response, Done = true WHERE ID = @id";
                connection.Open();

                foreach (BulkData data in bulkDatas)
                {
                    cmd.Parameters.AddRange(new OleDbParameter[]
                    {
                        new OleDbParameter("@response", data.response),
                        new OleDbParameter("@id", data.ID)
                    });
                    cmd.ExecuteNonQuery();
                }
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
