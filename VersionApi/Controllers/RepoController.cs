using System.Net.Http.Headers;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace VersionApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class RepoController : Controller
    {
        // Add this from an appropriate account on Azure with access to the organization.
        private const string personalaccesstoken = "xa56o6aujrixropq4n74ehiwhh4xfqqrjd32qxtf3oq4lgfp6h2a";

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
            var res = await CallApi<PrResponse>(url, async response =>
            {
                if (response.Count <= 0)
                {
                    return File(EmptyImage, "image/png");
                }
                var image = await System.IO.File.ReadAllBytesAsync("images/icons8-pull-request-30.png");
                return File(image, "image/png");
            });
            return res;
        }

        private async Task<ActionResult> CallApi<T>(string url, Func<T, Task<ActionResult>> func)
        {
            var client = CreateHttpClient();
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return BadRequest($"Http request failed {response.StatusCode}, Content: {content}");
            try
            {
                var resp = JsonConvert.DeserializeObject<T>(content);
                return resp == null 
                    ? BadRequest($"JsonDeserializer returned nada from {content}") 
                    : await func(resp);
            }
            catch (JsonReaderException)
            {
                return BadRequest("Json deserializer crashed");
            }
        }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalaccesstoken))));
            return client;
        }

        private static readonly byte[] EmptyImage = new byte[]
        {
            0x47, 0x49, 0x46, 0x38, 0x37, 0x61, 0x01, 0x00, 0x01, 0x00, 0x80, 0x01, 0x00, 0xFF, 0xFF, 0xFF, 0x00,
            0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x02, 0x44, 0x01
        };
    }




    public class PrResponse
    {
        public int Count { get; set; }
    }
}
