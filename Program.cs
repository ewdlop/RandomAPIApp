extern alias POSTagger;

using java.util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using POSTagger::edu.stanford.nlp.ling;
using POSTagger::edu.stanford.nlp.tagger.maxent;
using RandomAPIApp.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<JWTOptions>()
    .Bind(builder.Configuration.GetSection(nameof(JWTOptions)));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
        options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidIssuer = builder.Configuration.GetSection($"{nameof(JWTOptions)}:{nameof(JWTOptions.Issuer)}").Value,
                ValidAudience = builder.Configuration.GetSection($"{nameof(JWTOptions)}:{nameof(JWTOptions.Audience)}").Value,
                IssuerSigningKeys = new[]{
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection($"{nameof(JWTOptions)}:{nameof(JWTOptions.Secret)}").Value ??
                        throw new InvalidOperationException("JWT secret is not set")))
                },
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
            };
        });
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    string id = "Bearer";
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1",
    });
    c.AddSecurityDefinition(id, new OpenApiSecurityScheme()
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\". You may use /jwt endpoint to get a jwt token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
    });
    //this is a dictionary....
    c.AddSecurityRequirement(new OpenApiSecurityRequirement() 
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = id
                }
            },
            new List<string>()
        }
    });
});


builder.Services.AddAuthorizationBuilder()
  .AddPolicy(Policy.USER_NAME, policy =>
        policy.RequireClaim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Name));
            
            
WebApplication app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/jwt", (IOptions<JWTOptions> options) =>
{
    Claim[] claims = new Claim[]
    {
        new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Name, "user@example.com"),
    };

    string secretKey = options.Value.Secret;

    // Create a symmetric security key using the secret key
    SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

    // Create the JWT security token
    JwtSecurityToken token = new JwtSecurityToken(
        issuer: options.Value.Issuer,
        audience: options.Value.Audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(30),
        signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
    );
    JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
    string jwtToken = jwtHandler.WriteToken(token);
    return Results.Ok(jwtToken);
}).Produces<string>(StatusCodes.Status200OK)
.WithDescription("Get JWT Token for authorization")
.WithTags("Token");

app.MapGet("/pos", () =>
{
    // Text for tagging
    string text = "A Part-Of-Speech Tagger (POS Tagger) is a piece of software that reads text"
                + "in some language and assigns parts of speech to each word (and other token),"
                + " such as noun, verb, adjective, etc., although generally computational "
                + "applications use more fine-grained POS tags like 'noun-plural'.";
    object[] sentences = MaxentTagger.tokenizeText(new java.io.StringReader(text)).toArray();
    string[] taggedSentences = new string[sentences.Length];
    int i = 0;
    foreach (List sentence in sentences.Cast<List>())
    {
        var taggedSentence = StanfordNLP.MaxentTagger.Value.tagSentence(sentence);
        taggedSentences[i] = SentenceUtils.listToString(taggedSentence, false);
        i++;
    }
    return Results.Ok(taggedSentences);
}).Produces<string[]>(StatusCodes.Status200OK)
.Produces<string>(StatusCodes.Status401Unauthorized)
.RequireAuthorization(Policy.USER_NAME)
.WithDescription("Part-Of-Speech Tagger")
.WithTags("Part-Of-Speech Tagger");

app.Run();

public class StanfordNLPModelPath
{
    public const string POS = "english-left3words-distsim.tagger";
}
public static class StanfordNLP
{
    public readonly static Lazy<MaxentTagger> MaxentTagger = new Lazy<MaxentTagger>(()=>new MaxentTagger(StanfordNLPModelPath.POS));
}

public class Policy
{
    public const string USER_NAME = "UserName";
}