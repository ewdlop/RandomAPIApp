namespace RandomAPIApp.Options;

public record JWTOptions
{
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string Secret { get; init; }
}
