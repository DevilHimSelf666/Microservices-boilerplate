namespace Gateway.Security;

public sealed class NoopSsoConfigurator : ISsoConfigurator
{
    public void Configure(IApplicationBuilder app)
    {
        // TODO: Replace with organization-provided AddSso(app) call when the package is available.
    }
}
