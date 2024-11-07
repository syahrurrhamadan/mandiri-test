using System.ComponentModel.DataAnnotations;

namespace WebApi.Dto
{
    public class GeneralSettings
    {
        public string? AppUrl { get; set; }
        public string? PrivateKey { get; set; }
    }
}
