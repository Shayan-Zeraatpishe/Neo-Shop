using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Application.DTOs
{
    public class CommentModerationResultDto
    {
        public bool IsApproved { get; set; }

        public string Reason { get; set; } = string.Empty;
    }

    public class OllamaSettings
    {
        public string BaseUrl { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;
    }

}
