namespace MediPro.Api.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    /// <summary>Symmetric key for HS256 — must be sufficiently long for algorithm.</summary>
    public string SigningKey { get; set; } = "";
    public int ExpiryMinutes { get; set; } = 120;
}
