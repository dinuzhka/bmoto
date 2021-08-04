using bmoto.DTO;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bmoto.Context
{
    public class SqliteConnector
    {
        private readonly string conStr;

        private static SqliteConnector instance;
        private SqliteConnector(IConfiguration configuration)
        {
            var filePath = configuration["SqliteFile"];
            string fixedConnectionString = filePath.Replace("{AppDir}", AppDomain.CurrentDomain.BaseDirectory);
            conStr = @$"Data Source={fixedConnectionString}";
        }
        public static SqliteConnector Instance(IConfiguration configuration)
        {
            if (instance == null)
            {
                instance = new SqliteConnector(configuration);
            }
            return instance;
        }

        public List<WebField> GetFields()
        {
            var fields = new List<WebField>();

            var query = "SELECT * FROM MotoFields ";
            using (var con = new SqliteConnection(conStr))
            {
                con.Open();

                var command = con.CreateCommand();
                command.CommandText = query;

                using (var rdr = command.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        fields.Add(new WebField()
                        {
                            Section = rdr["Section"].ToString(),
                            Type = rdr["Type"].ToString(),
                            Max = float.Parse(rdr["Max"].ToString()),
                            Name = rdr["Name"].ToString(),
                            Format = !string.IsNullOrEmpty(rdr["Format"].ToString()) ? rdr["Format"].ToString() : rdr["Type"].ToString(),
                            Mandatory = int.Parse(rdr["Mandatory"].ToString()) == 1,
                            IsPoints = int.Parse(rdr["CalculatePoints"].ToString()) == 1,

                        });
                    }
                }
            }
            
            return fields;
        }

        public List<JObject> GetReports()
        {
            var fields = new List<JObject>();

            var query = "SELECT * FROM AuditReports ";
            using (var con = new SqliteConnection(conStr))
            {
                con.Open();

                var command = con.CreateCommand();
                command.CommandText = query;

                using (var rdr = command.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        JObject obj=JsonConvert.DeserializeObject<JObject>(rdr["AuditObj"].ToString());
                        fields.Add(obj);
                    }
                }
            }

            return fields;
        }

        internal void SaveData(string model)
        {
            var query = "INSERT INTO AuditReports (AuditObj) VALUES (@AuditObj)";
            using (var con = new SqliteConnection(conStr))
            {
                con.Open();

                var command = con.CreateCommand();
                command.Parameters.Add(new SqliteParameter("AuditObj", model));
                command.CommandText = query;
                command.ExecuteNonQuery();
            }

        }
    }
}
