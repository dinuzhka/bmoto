using bmoto.Context;
using bmoto.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace bmoto.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MotoController : ControllerBase
    {

        private readonly ILogger<MotoController> _logger;
        private readonly IConfiguration _configuration;
        public MotoController(ILogger<MotoController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<WebField> Get()
        {

            var connection = SqliteConnector.Instance(_configuration);
            return connection.GetFields();
        }

        [HttpGet]
        [Route("fieldvalues/{field}")]
        public IEnumerable<string> FieldValues(string field)
        {
            var connection = SqliteConnector.Instance(_configuration);
            var report=connection.GetReports();
            var fields = report.Select(r => r[field]).Where(r=>r!=null).Select(r=>r.ToString()).ToList();
            return fields;
        }


        [HttpGet]
        [Route("export")]
        public string ExportData()
        {
            var connection = SqliteConnector.Instance(_configuration);
            var report = connection.GetReports();
            JArray array = new JArray();
            foreach(var r in report)
            {
                array.Add(r);
            }
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(array.ToString());
            return DataTableToJSONWithJavaScriptSerializer(dt);
        }
        private string DataTableToJSONWithJavaScriptSerializer(DataTable table)
        {
            List<Dictionary<string, object>> parentRow = new List<Dictionary<string, object>>();
            Dictionary<string, object> childRow;
            foreach (DataRow row in table.Rows)
            {
                childRow = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    childRow.Add(col.ColumnName, row[col]);
                }
                parentRow.Add(childRow);
            }
            return JsonConvert.SerializeObject(parentRow);
        }

        [HttpPost]
        public void SaveToDb([FromBody] MyPostModel jObject)
        {
            var connection = SqliteConnector.Instance(_configuration);
            connection.SaveData(jObject.model);
        }
    }

    public class MyPostModel
    {
        public string model { get; set; }
    }
}
