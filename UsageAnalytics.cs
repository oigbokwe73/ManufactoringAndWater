using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using Xenhey.BPM.Core.Net8.Implementation;
using Xenhey.BPM.Core.Net8;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;

namespace ManufactoringAndWater
{
    public class UsageAnalytics
    {
        private readonly ILogger _logger;

        public UsageAnalytics(ILogger<UsageAnalytics> logger)
        {
            _logger = logger;
        }

        private HttpRequest _req;
        private NameValueCollection nvc = new NameValueCollection();
        [Function("usageanalytics")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "usageanalytics/{id}")]HttpRequest req, string id)
        {
            var input = JsonConvert.SerializeObject(new { id });
            _req = req;

            _logger.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = input;
            _req.Headers.ToList().ForEach(item => { nvc.Add(item.Key, item.Value.FirstOrDefault()); });
            var results = orchrestatorService.Run(requestBody);
            return resultSet(results);
        }

        private ActionResult resultSet(string reponsePayload)
        {
            var returnContent = new ContentResult();
            var mediaSelectedtype = nvc.Get("Content-Type");
            returnContent.Content = reponsePayload;
            returnContent.ContentType = mediaSelectedtype;
            return returnContent;
        }
        private IOrchestrationService orchrestatorService
        {
            get
            {
                return new RemoteOrchrestratorService(nvc);
            }
        }

    }
}
