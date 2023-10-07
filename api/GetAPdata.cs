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

namespace GetAPdata
{    
    public static class GetAPdataMain
    {
        private static HttpClient httpClient = new HttpClient();
        private static readonly string AZURE_CONN_STRING = Environment.GetEnvironmentVariable("AzureSQLConnectionString");

        [FunctionName("GetAPdata")]
        public static async Task<IActionResult> GetAPdata([HttpTrigger("get", Route = "ap-data")] HttpRequest req, ILogger log)
        {                              
            int count = 10;
            Int32.TryParse(req.Query["c"], out count);
            if (count < 1 | count >50) count = 10;

            string icao = req.Query["i"], city = req.Query["city"], cntry = req.Query["cntry"];
            if (icao == null | icao == "null") {icao = "%";} else if (icao.Length > 15) {icao = icao.Substring(0,15);}
            if (city == null | city == "null") {city = "%";} else if (city.Length > 31) {city = city.Substring(0,31);}
            if (cntry == null | cntry == "null") {cntry = "%";} else if (cntry.Length > 31) {cntry = cntry.Substring(0,31);}


            string s_sort = "rd", sort = "elevation desc";
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
                case "ld":
                    sort = "laty desc";
                    break;
                case "la":
                    sort = "laty asc";
                    break;
            }
            
            using(var conn = new SqlConnection(AZURE_CONN_STRING))
            {
                var result = await conn.QuerySingleOrDefaultAsync<string>(
                    "dbo.GetairportData1", 
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
