using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DACH.IIIF
{
    public class DachIIIF
    {
        private readonly ILogger<DachIIIF> _logger;

        public DachIIIF(ILogger<DachIIIF> logger)
        {
            _logger = logger;
        }

        [Function("DachIIIF")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
