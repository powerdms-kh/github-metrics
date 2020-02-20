using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GitHubMetrics.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SplitController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly Splitio.Services.Client.Interfaces.ISplitClient _splitClient;

        public SplitController(Splitio.Services.Client.Interfaces.ISplitClient splitClient)
        {
            _splitClient = splitClient;
        }

        [HttpGet]
        public string Get()
        {
            return _splitClient.GetTreatment("1", "INGESTION_V2");
        }
    }
}
