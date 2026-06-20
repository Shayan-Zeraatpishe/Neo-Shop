using Shop.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Application.Interfaces
{
    public interface ICommentModerationService
    {
        Task<CommentModerationResultDto>
        AnalyzeCommentAsync(string comment);
    }
}
