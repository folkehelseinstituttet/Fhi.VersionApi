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
        /// <param name="prjUrl">Url including team project, like https://dev.azure.com/{organization}/{project}</param>
        /// <param name="repositoryId">ID for repository. Kan hentes ut med Azure CLI, az repos list</param>
        /// <returns>an image</returns>
        [HttpGet("PR")]
        [Produces("image/png")]
        public async Task<IActionResult> PullRequestsStatus(string prjUrl, string repositoryId)
        {
            var url = $"{prjUrl}/_apis/git/repositories/{repositoryId}/pullrequests?api-version=7.0";
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

        /// <summary>
        /// Returns the latest version number from the code tags
        /// </summary>
        /// <param name="prjUrl">Url including team project, like https://dev.azure.com/{organization}/{project}</param>
        /// <param name="repositoryId">ID for repository. Kan hentes ut med Azure CLI, az repos list</param>
        /// <returns>a text with version number</returns>
        [HttpGet("CodeVersion")]
        [Produces("image/svg+xml")]
        public async Task<IActionResult> CodeVersion(string prjUrl, string repositoryId)
        {
            // https://dev.azure.com/{organization}/{project}/_apis/git/repositories/{repositoryId}/refs?filter=tags/&api-version=6.0-preview.1
            var tags = $"{prjUrl}/_apis/git/repositories/{repositoryId}/refs?filter=tags/&api-version=6.0-preview.1";
            var res = await CallApi<CodeVersionResponse>(tags, async response =>
            {
                var versions = response.Value;
                var latest = versions.Max(o => o.GetVersion());
                var url = $"https://img.shields.io/badge/Tag-{latest?.ToString()}-lightgrey";
                var httpclient = new HttpClient();
                var image = await httpclient.GetAsync(url);
                var content = await image.Content.ReadAsStringAsync();
                return Content(content, "image/svg+xml");
            });
            return res;
        }

        //  https://fhi.visualstudio.com/Fhi.Legemiddelregisteret
        //  1ee28ccf-7d85-4955-b5b9-d7ffaf3495eb


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

        private static readonly byte[] EmptyImage = {
            0x47, 0x49, 0x46, 0x38, 0x37, 0x61, 0x01, 0x00, 0x01, 0x00, 0x80, 0x01, 0x00, 0xFF, 0xFF, 0xFF, 0x00,
            0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x02, 0x44, 0x01
        };
    }

    public class CodeVersionResponse
    {
        public List<CodeVersion> Value { get; set; }
        public int Count { get; set; }
    }

    public class CodeVersion
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable IDE1006 // Naming Styles
        // ReSharper disable once InconsistentNaming
        public string name { get; set; } = "";// like "refs/tags/0.1.10-20201204"
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public string TagString
        {
            get
            {
                var s = name.Replace("refs/tags/", "");
                if (s.Contains("-"))
                    s = s.Substring(0, s.LastIndexOf("-"));
                return s;
            }
        }

        public Version GetVersion()
        {
            var tagString = TagString;
            var versionSplit = tagString.Split('.');
            if (versionSplit.Length != 3)
                return new Version(0, 0, 0); // Not a valid version tag, so we skip this
            try
            {
                var version = new Version(int.Parse(versionSplit[0]), int.Parse(versionSplit[1]), int.Parse(versionSplit[2]));
                return version;
            }
            catch (Exception)
            {
                return new Version(0, 0, 0); // Not a valid version tag, so we skip this
            }
        }
    }




    public class PrResponse
    {
        public int Count { get; set; }
    }


}
