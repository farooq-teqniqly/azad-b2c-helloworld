using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace B2CApi
{
    public static class B2CApi
    {
        [FunctionName("GetValidCompanyDomains")]
        public static async Task<IActionResult> GetValidCompanyDomains(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "domains")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var domains = new[]
            {
                "microsoft.com",
                "test.com",
                "wintellect.com",
                "marel.com"
            };

            return new OkObjectResult(new
            {
                domains
            });
        }

        [FunctionName("StringCollectionContainsClaim")]
        public static async Task<IActionResult> StringCollectionContainsClaim(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "stringcollectioncontainsclaim")]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            using (var reader = new StreamReader(req.Body))
            {
                var json = await reader.ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<StringCollectionContainsClaimData>(json);
                var found = data.InputClaim.Any(
                    c => c.Equals(data.Item, StringComparison.InvariantCultureIgnoreCase));

                return new OkObjectResult(
                    new ContainsClaimResponse {OutputClaim = found});
            }
        }

        [FunctionName("VerifyAccessCode")]
        public static async Task<IActionResult> VerifyAccessCode(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "verifyaccesscode")]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            using (var reader = new StreamReader(req.Body))
            {
                var json = await reader.ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<VerifyAccessCodeData>(json);

                if (string.Compare(data.AccessCode, "67890", StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    return new ConflictObjectResult(new ErrorResponse
                    {
                        Status = StatusCodes.Status409Conflict,
                        Version = "1.0",
                        Code = "error",
                        RequestId = Guid.NewGuid().ToString(),
                        UserMessage = "Invalid access code.",
                        DeveloperMessage = $"Invalid access code {data.AccessCode}."
                    });
                }

                return new OkResult();
            }
        }
    }

    public class StringCollectionContainsClaimData
    {
        public string[] InputClaim { get; set; }

        public string Item { get; set; }
    }

    public class VerifyAccessCodeData
    {
        public string UserEmail { get; set; }
        public string AccessCode { get; set; }
    }

    public class ContainsClaimResponse
    {
        public bool OutputClaim { get; set; }
    }

    public class ErrorResponse
    {
        public string Version { get; set; }
        public int Status { get; set; }
        public string Code { get; set; }
        public string UserMessage { get; set; }
        public string DeveloperMessage { get; set; }
        public string RequestId { get; set; }
        public string MoreInfo { get; set; }
    }
}

