using Newtonsoft.Json;

namespace ATY_HBL2D_SHKSchedulerService.Models
{
    internal class GenericResponse
    {
        public ResponseOrder Response { get; set; }
    }

    public class ResponseOrder
    {
        [JsonProperty("return")]
        public Return Return { get; set; }
    }

    public class Return
    {
        public string returnFlag { get; set; }
        public string returnCode { get; set; }
        public string returnDesc { get; set; }
    }
}
