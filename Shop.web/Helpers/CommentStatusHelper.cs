using Shop.Domain.Enums;

namespace Shop.web.Helpers;

public static class CommentStatusHelper
{
    public static string GetPersianLabel(CommentStatus status) => status switch
    {
        CommentStatus.Pending => "در انتظار بررسی",
        CommentStatus.Approved => "تأیید شده",
        CommentStatus.Rejected => "رد شده",
        _ => status.ToString()
    };
}
