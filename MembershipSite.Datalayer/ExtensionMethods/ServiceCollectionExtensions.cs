namespace MembershipSite.Datalayer.ExtensionMethods;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatalayer(this IServiceCollection services)
    {
        services
            .AddTransient<AuditLogDal>()
            .AddTransient<MemberDal>();

        return services;
    }
}
