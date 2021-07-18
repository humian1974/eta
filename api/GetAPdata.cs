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

namespace GetAPdata // ShowBusData
{    
    public static class GetAPdataMain // ShowBusDataMain
    {
        private static HttpClient httpClient = new HttpClient();
        private static readonly string AZURE_CONN_STRING = Environment.GetEnvironmentVariable("AzureSQLConnectionString");

        [FunctionName("GetAPdata")]
        public static async Task<IActionResult> GetAPdata([HttpTrigger("get", Route = "ap-data")] HttpRequest req, ILogger log)
        {                              
            int count = 10;
            string s_sort = "rd", sort = "elevation desc";
            string s_icao = req.Query["i"], icao = "%";
            string s_city = req.Query["city"], city = "%";
            string s_cntry = req.Query["cntry"], cntry = "%";

            if (s_icao != null & s_icao != "null") icao = s_icao;
            if (s_city != null & s_city != "null") city = s_city;
            if (s_cntry != null & s_cntry != "null") cntry = s_cntry;

            Int32.TryParse(req.Query["c"], out count);
            if (count < 1 | count >50) count = 10;

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
                        @icao = icao,
                        @sort = sort,
                        @city = city,
                        @country = cntry
                    }, commandType: CommandType.StoredProcedure);                
                
                return new OkObjectResult(JObject.Parse(result));
            }            
        }
    }
}
