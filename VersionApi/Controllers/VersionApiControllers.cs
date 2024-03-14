using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace VersionApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class VersionApiControllers : ControllerBase
    {
        static readonly Dictionary<string, VersionDTO> Information;
        private static string path = "";

        static VersionApiControllers()
        {
            Information = ReadDictonary();
        }

        private static Dictionary<string, VersionDTO> ReadDictonary()
        {
            const string filename = "versionApiFile.json";
            var folder = Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData);
            // var folder = @"\\versionapi.file.core.windows.net\versionapi";  // Require setting up permissions for Azure File Storage
            path = Path.Combine(folder, filename);
            if (!System.IO.File.Exists(path))
            {
                return new Dictionary<string, VersionDTO>();

            }

            var allText = System.IO.File.ReadAllText(path);
            var versionApiDict = JsonConvert.DeserializeObject<Dictionary<string, VersionDTO>>(allText);

            return versionApiDict!;

        }

        private string CreateKey(string enviroment, string system, string component) => $"{enviroment}.{system}.{component}";


        private void UploadInformation()
        {
            var jsonString = JsonConvert.SerializeObject(Information, Formatting.Indented);
            System.IO.File.WriteAllText(path, jsonString);
        }

        [HttpGet("GetInformation")]
        public ActionResult<ShieldsIo> GetInformation(string enviroment, string system, string component)
        {
            var dtoFound = Information.TryGetValue(CreateKey(enviroment, system, component), out var dto);

            return Ok(dtoFound == false 
                ? new ShieldsIo("Version", $"Not Found ({enviroment}-{system}-{component})")
                : new ShieldsIo("Version", dto!.Version));
        }

        [HttpGet("GetStatus")]
        [Produces("image/jpeg")]
        public IActionResult GetStatus(string enviroment, string system, string component)
        {
            var dtoFound = Information.TryGetValue(CreateKey(enviroment, system, component), out var dto);
            return dtoFound ? StatusText(dto!.Status) : StatusText("NotFound");
        }

        [HttpGet("SetInformation")]
        public ActionResult SetInformation(string environment, string system, string component, string version, string status)
        {
            VersionDTO dto = new()
            {
                Enviroment = environment,
                System = system,
                Component = component,
                Version = version,
                Status = status,
                Date = DateTime.Now
            };

            var key = CreateKey(environment, system, component);

            var dtoFound = Information.TryGetValue(key, out var outdto);
            if (!dtoFound)
            {
                Information.Add(key, dto);
            }
            else
            {
                Information[key] = dto;
            }

            UploadInformation();
            return Ok();
        }

        [HttpGet("DeleteInformation")]
        public void DeleteInformation(string system, string component)
        {
            var dtoFound = Information.TryGetValue($"{system}.{component}", out var dto);

            if (dtoFound == false)
            {
                return;
            }

            Information.Remove($"{system}.{component}");

            UploadInformation();
        }

        [HttpGet("DeleteAllInformation")]
        public void DeleteAllInformation()
        {
            Information.Clear();
            UploadInformation();
        }

        /// <summary>
        /// Returns a round status image based on the number parameter, which can be 0-3.
        /// 0 = Red, 1 = Green, 2 = Yellow, 3 = Orange, anything else = Question mark
        /// </summary>
        /// <param name="num">0-3</param>
        /// <returns>Round 3D png image</returns>
        [HttpGet("Status")]
        public IActionResult StatusImage(int num)
        {
            var image = Array.Empty<byte>();

            image = num switch
            {
                0 => System.IO.File.ReadAllBytes("images/reddot3D.png"),
                1 => System.IO.File.ReadAllBytes("images/greendot3D.png"),
                2 => System.IO.File.ReadAllBytes("images/yellowdot3D.png"),
                3 => System.IO.File.ReadAllBytes("images/orangedot3D.png"),
                _ => System.IO.File.ReadAllBytes("images/question3D.png"),
            };

            return File(image, "image/jpeg");

        }

        /// <summary>
        /// Returns a round status image based on the text parameter
        /// "unhealthy" or "error" or "red" => Red
        /// "healthy" or "green" => Green
        /// "warning" or "yellow" => Yellow
        /// "degraded" or "orange"
        /// "black", "blue", "pink", "turquoise" => Black, Blue, Pink, Turquoise
        /// "crash" => No entry symbol
        /// Anything else returns a question mark image
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Round 3D png image</returns>
        [HttpGet("StatusText")]
        [Produces("image/jpeg")]
        public IActionResult StatusText(string text)
        {
            var newtext = text.ToLower();

            var image = newtext switch
            {
                "unhealthy" or "error" or "red" => System.IO.File.ReadAllBytes("images/reddot3D.png"),
                "healthy" or "green" => System.IO.File.ReadAllBytes("images/greendot3D.png"),
                "warning" or "yellow" => System.IO.File.ReadAllBytes("images/yellowdot3D.png"),
                "degraded" or "orange" => System.IO.File.ReadAllBytes("images/orangedot3D.png"),
                "black" => System.IO.File.ReadAllBytes("images/blackdot3D.png"),
                "blue" => System.IO.File.ReadAllBytes("images/bluedot3D.png"),
                "pink" => System.IO.File.ReadAllBytes("images/pinkdot3D.png"),
                "turquoise" => System.IO.File.ReadAllBytes("images/turkisdot3D.png"),
                "crash" => System.IO.File.ReadAllBytes("images/crashdot3D.png"),
                _ => System.IO.File.ReadAllBytes("images/question3D.png"),
            };

            return File(image, "image/jpeg");
        }

        [HttpGet("StatusTextFromUrl")]
        [Produces("image/jpeg")]
        public async Task<IActionResult> StatusTextFromUrl(string url)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return StatusText("crash");
            var statusAsString = await response.Content.ReadAsStringAsync();
            return StatusText(statusAsString);
        }

        [HttpGet("HealthStatus")]
        [Produces("image/jpeg")]
        public async Task<IActionResult> HealthStatus(string url)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return StatusText("crash");
            var statusText = await GetStatusText(response);
            return StatusText(statusText);
        }

        private static async Task<string> GetStatusText(HttpResponseMessage response)
        {
            var statusAsString = await response.Content.ReadAsStringAsync();
            StatusResponse status;
            try
            {
                status = JsonConvert.DeserializeObject<StatusResponse>(statusAsString)??new StatusResponse { Status=statusAsString};
            }
            catch (JsonReaderException)
            {
                status = new StatusResponse { Status = statusAsString };
            }
            if (!string.IsNullOrEmpty(status.OverStatus2))
                return status.OverStatus2;
            return !string.IsNullOrEmpty(status.Status) ? status.Status : "Nothing";
        }

        [HttpGet("Dump")]
        public string Dump()
        {
            return JsonConvert.SerializeObject(Information, Formatting.Indented);
        }

    }

}

