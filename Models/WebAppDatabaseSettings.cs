namespace WebAppAuthenticationServer.Models;

public class WebAppDatabaseSettings
{
    public string? ConnectionString { get; set; }
    public string? DatabaseName { get; set; }
    public string? UserCollectionName { get; set; }
    public string? WebApplicationsCollectionName { get; set; }
}

