using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Octokit;

namespace GitHubMetrics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetricsController : ControllerBase
    {
        private readonly GitHubClient Client;
        public MetricsController(GitHubClient Client)
        {
            this.Client = Client;
        }

        [HttpGet]
        public async Task<string> Get(int repositoryId)
        {
            int MergedPRs = 0;
            double TotalDuration = 0;
            List<MergedPullRequest> DurationsOpened = new List<MergedPullRequest>();
            PullRequestRequest request = new Octokit.PullRequestRequest
            {
                State = ItemStateFilter.All,
                SortDirection = SortDirection.Descending
            };

            ApiOptions apiOptions = new Octokit.ApiOptions
            {
                PageSize = 100,
                PageCount = 1,
                StartPage = 1
            };

            try
            {
                var PullRequests = await this.Client.PullRequest.GetAllForRepository(repositoryId, request, apiOptions);

                foreach (PullRequest PullRequest in PullRequests)
                {
                    if (PullRequest.MergedAt.HasValue && (PullRequest.Base.Ref == "development" || PullRequest.Base.Ref == "develop"))
                    {
                        var DurationOpened = PullRequest.MergedAt.Value.DateTime.Subtract(PullRequest.CreatedAt.DateTime);
                        DurationsOpened.Add(new MergedPullRequest
                        {
                            id = PullRequest.Number,
                            pr = PullRequest.HtmlUrl,
                            sourceBranch = PullRequest.Head.Ref,
                            author = PullRequest.User.Login,
                            durationOpened = DurationOpened.TotalSeconds,
                            additions = PullRequest.Additions,
                            deletions = PullRequest.Deletions,
                            merger = PullRequest.MergedBy != null ? PullRequest.MergedBy.Login : null
                        });

                        TotalDuration += DurationOpened.TotalSeconds;
                    }
                }

                DurationsOpened.Sort(delegate (MergedPullRequest c1, MergedPullRequest c2) { return c1.durationOpened.CompareTo(c2.durationOpened); });

                return JsonSerializer.Serialize(new MetricsResponse
                {
                    averageDuration = TotalDuration / DurationsOpened.Count() / 3600,
                    mergedRequests = DurationsOpened,
                    numberMerged = DurationsOpened.Count(),
                    nintiethPercentile = DurationsOpened.ElementAt(Convert.ToInt32(DurationsOpened.Count * .9)).durationOpened / 3600
                });

            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);

                return JsonSerializer.Serialize("There was an error processing your request");
            }
        }
    }

    public class MetricsResponse
    {
        public double averageDuration { get; set; }
        public double nintiethPercentile { get; set; }
        public List<MergedPullRequest> mergedRequests { get; set; }
        public int numberMerged { get; set; }

    }

    public class MergedPullRequest
    {
        public long id { get; set; }
        public double durationOpened { get; set; }
        public string pr { get; set; }
        public string author { get; set; }
        public int additions { get; set; }
        public int deletions { get; set; }
        public string merger { get; set; }
        public string sourceBranch { get; set; }
    }
}