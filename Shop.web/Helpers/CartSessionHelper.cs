namespace Shop.web.Helpers;

public static class CartSessionHelper
{
    public const string SessionKey = "CartSessionId";

    public static string GetOrCreateSessionId(HttpContext context)
    {
        var sessionId = context.Session.GetString(SessionKey);
        if (!string.IsNullOrEmpty(sessionId))
            return sessionId;

        sessionId = Guid.NewGuid().ToString("N");
        context.Session.SetString(SessionKey, sessionId);
        return sessionId;
    }
}
