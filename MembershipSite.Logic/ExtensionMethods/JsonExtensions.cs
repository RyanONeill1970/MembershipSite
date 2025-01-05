namespace MembershipSite.Logic.ExtensionMethods;

public static class JsonExtensions
{
    private static readonly JsonSerializerOptions options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = true,
    };

    public static string ToJson(this object model)
    {

        return JsonSerializer.Serialize(model, options);
    }
}
