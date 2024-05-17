using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ClassLibraryForOOPaP_6_7
{  
    public class Repository<T> where T : class, new()
    {
        private readonly string _connectionString;

        public Repository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MedicalRecordsDB"].ConnectionString;
        }

        public List<T> GetAll(string query, Func<IDataReader, T> readRecord)
        {
            var records = new List<T>();

            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(query, connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        records.Add(readRecord(reader));
                    }
                }
            }

            return records;
        }

        public void ExecuteNonQuery(string query, Action<SqlCommand> parameterize)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(query, connection);
                parameterize(command);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // Implement Create, Update, Delete methods similarly...
    }

}
