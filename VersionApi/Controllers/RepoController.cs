using System.Net.Http.Headers;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace VersionApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class RepoController : Controller
    {
        private const string personalaccesstoken = "mg64neqhjf2dvcbgr4qpqddskgefk2artydxi6rjjchpxp4dx2la";





        /// <summary>
        /// Returns a PR image if there are any PRs in the repo, blank otherwise
        /// </summary>
        /// <param name="repoUrl">Url including team project, like https://dev.azure.com/{organization}/{project}</param>
        /// <param name="repositoryId">ID for repository. Kan hentes ut med Azure CLI, az repos list</param>
        /// <returns></returns>
        [HttpGet("PR")]
        [Produces("image/png")]
        public async Task<IActionResult> PullRequestsStatus(string repoUrl, string repositoryId)
        {
            var url = $"{repoUrl}/_apis/git/repositories/{repositoryId}/pullrequests?api-version=7.0";
            var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalaccesstoken))));

            var response = await client.GetAsync(url);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return BadRequest($"Http request failed {response.StatusCode}");
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var resp = JsonConvert.DeserializeObject<PrResponse>(content);
                if (resp != null)
                {
                    if (resp.Count > 0)
                    {
                        var image = await System.IO.File.ReadAllBytesAsync("images/icons8-pull-request-30.png");
                        return File(image, "image/png");
                    }

                    var noimage = Array.Empty<byte>();
                    return File(noimage,"image/png");
                }
                return BadRequest($"JsonDeserializer returned nada from {content}");
            }
            catch (JsonReaderException)
            {
                return BadRequest("Json deserializer crashed");
            }

        }
    }




    public class PrResponse
    {
        public int Count { get; set; }
    }
}
