using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Newtonsoft;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ShowBusData
{    
    public static class ShowBusDataMain
    {
        private static HttpClient httpClient = new HttpClient();
        private static readonly string AZURE_CONN_STRING = Environment.GetEnvironmentVariable("AzureSQLConnectionString");

        [FunctionName("ShowBusData")]
        public static async Task<IActionResult> ShowBusData([HttpTrigger("get", Route = "ap-data")] HttpRequest req, ILogger log)
        {                              
            int count = 10;
            char initial = '%';
            string s_sort = "rd", sort = "elevation desc";
            string s_city = req.Query["city"], city = "%";
            string s_cnty = req.Query["cnty"], cnty = "%";

            if (s_city != null) city = s_city;
            if (s_cnty != null) cnty = s_cnty;

            Int32.TryParse(req.Query["c"], out count);
            Char.TryParse(req.Query["i"], out initial);
            if (initial == '\0') initial = '%';
            if (count == 0) count = 10;
            switch (s_sort = req.Query["s"])
            {
                case "rd":
                    sort = "rwy desc";
                    break;
                case "ra":
                    sort = "rwy asc";
                    break;
                case "ed":
                    sort = "elevation desc";
                    break;
                case "ea":
                    sort = "elevation asc";
                    break;
            }
            
            using(var conn = new SqlConnection(AZURE_CONN_STRING))
            {
                var result = await conn.QuerySingleOrDefaultAsync<string>(
                    "web.GetairportData3", 
                    new {
                        @countn = count,
                        @initchar = initial,
                        @sort = sort,
                        @city = city,
                        @country = cnty
                    }, commandType: CommandType.StoredProcedure);                
                
                return new OkObjectResult(JObject.Parse(result));
            }            
        }
    }
}
