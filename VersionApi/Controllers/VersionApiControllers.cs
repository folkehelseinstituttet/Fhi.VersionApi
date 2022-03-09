using Microsoft.AspNetCore.Mvc;

namespace VersionApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VersionApiControllers : ControllerBase
    {
        static Dictionary<string, VersionDTO> information = new();

        [HttpGet("GetInformation")]
        public ActionResult GetInformation(string system, string component)
        {
            var dtoFound = information.TryGetValue($"{system}.{component}", out var dto);
           
            if (dtoFound == false)
            {
                return Ok(new ShieldsIo("Version", "Not Found"));
            }
            else
            {
                return Ok(new ShieldsIo("Version", dto!.Version));
            }

        }

        [HttpGet("SetInformation")]
        public void SetInformation(string system, string component, string version)
        {
            VersionDTO dto = new()
            {
                System = system,
                Component = component,
                Version = version
            };
            information.Add($"{system}.{component}", dto);
        }
    }

    /// <summary>
    /// To be used with Badges in Shields.io, ref https://shields.io/endpoint
    /// </summary>
    public class ShieldsIo
    {
        public int schemaVersion => 1;
        public string label { get; set; }
        public string message { get; set; }

        public string color { get; set; } = "lightgrey";

        public ShieldsIo(string label, string message)
        {
            this.label = label;
            this.message = message;
        }
    }

}

