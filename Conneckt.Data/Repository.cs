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
                command.CommandText = "SELECT * FROM BULK";
                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    bulkData.Add(new BulkData
                    {
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
