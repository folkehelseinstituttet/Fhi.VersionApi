namespace VersionApi
{
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