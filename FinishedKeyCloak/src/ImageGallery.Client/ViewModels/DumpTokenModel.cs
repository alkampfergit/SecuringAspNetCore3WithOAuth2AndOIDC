using System.Collections.Generic;

namespace ImageGallery.Client.ViewModels
{
    public class DumpTokenModel
    {
        public string AccessToken { get; set; }

        public string IdToken { get; set; }

        public string RefreshToken { get; set; }

        public IReadOnlyCollection<ClaimDto> Claims { get; set; }
    }

    public class ClaimDto
    {
        public ClaimDto(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public string Type { get; set; }

        public string Value { get; set; }
    }
}
