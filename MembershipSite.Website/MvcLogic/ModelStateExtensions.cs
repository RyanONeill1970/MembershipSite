namespace MembershipSite.Website.MvcLogic;

public static class ModelStateExtensions
{
    public static Dictionary<string, string[]> LogErrors(this ModelStateDictionary modelState)
    {
        var errors = modelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
            );

        var modelStateSummary = string.Join(", ", errors.Select(e => $"{e.Key}: {string.Join(", ", e.Value)}"));
        AppLogging.Write($"SaveMemberDataAsync ModelState invalid - {modelStateSummary}");

        return errors;
    }
}
