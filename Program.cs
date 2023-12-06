using com.sun.tools.@internal.xjc.reader.gbind;
using com.sun.tools.javac.util;
using edu.stanford.nlp.ie.crf;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.parser.lexparser;
using edu.stanford.nlp.pipeline;
using edu.stanford.nlp.process;
using edu.stanford.nlp.tagger.maxent;
using edu.stanford.nlp.time;
using edu.stanford.nlp.trees;
using edu.stanford.nlp.util;
using java.util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using RandomAPIApp.DTOs;
using RandomAPIApp.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static com.sun.tools.javah.Util;

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddAuthorizationBuilder()
  .AddPolicy(Policy.USER_NAME, policy =>
        policy.RequireClaim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Name));

ODataConventionModelBuilder modelBuilder = new();
modelBuilder.EntityType<OrderDTO>();
modelBuilder.EntitySet<CustomerDTO>("Customers");

//builder.Services.AddControllers().AddOData(
//options =>
//{
//    options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(null)
//    .AddRouteComponents("odata", modelBuilder.GetEdmModel());
//});
builder.Services.AddControllers();


WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.UseCors();

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

app.MapPost("/api/pos", ([FromBody] PartOfSpeechTaggerDTO pos) =>
{
    java.io.StringReader reader = new java.io.StringReader(pos.Input);
    object[] sentences = MaxentTagger.tokenizeText(reader).toArray();
    string[] taggedSentences = new string[sentences.Length];
    int i = 0;
    foreach (List sentence in sentences.Cast<List>())
    {
        var taggedSentence = StanfordNLP.MaxentTagger.Value.tagSentence(sentence);
        taggedSentences[i] = SentenceUtils.listToString(taggedSentence, false);
        i++;
    }
    reader.close();
    return Results.Ok(new PartOfSpeechTaggerDTO
    {
        Input = pos.Input,
        Output = taggedSentences
    });
}).Produces<PartOfSpeechTaggerDTO>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.RequireAuthorization(Policy.USER_NAME)
.WithDescription("Part-Of-Speech Tagger")
.WithTags("Part-Of-Speech Tagger");

app.MapPost("/api/parser", ([FromBody] ParserDTO parser) =>
{
    //The PTB (Penn Treebank) Tokenizer is a tool for dividing a block of text into individual tokens,
    //or "words," that are appropriate for use in natural language processing tasks
    TokenizerFactory tokenizerFactory = PTBTokenizer.factory(new CoreLabelTokenFactory(), "");
    java.io.StringReader sentenceReader = new java.io.StringReader(parser.Input);
    List rawWords = tokenizerFactory.getTokenizer(sentenceReader).tokenize();
    sentenceReader.close();
    Tree tree2 = StanfordNLP.Parser.Value.apply(rawWords);
    ////prints out a representation of a tree in Penn Treebank format.
    ////The Penn Treebank is a set of annotated corpora for natural language processing tasks,
    ////including part-of-speech tagging and parsing.
    //tree2.pennPrint();

    // Extract dependencies from lexical tree
    PennTreebankLanguagePack tlp = new PennTreebankLanguagePack();
    GrammaticalStructureFactory gsf = tlp.grammaticalStructureFactory();
    GrammaticalStructure gs = gsf.newGrammaticalStructure(tree2);
    List tdl = gs.typedDependenciesCCprocessed();
    //var tp = new TreePrint("penn,typedDependenciesCollapsed");
    //tp.printTree(tree2);
    string output = SentenceUtils.listToString(tdl, false);
    return Results.Ok(new ParserDTO()
    {
        Input = parser.Input,
        Output = output
    });
}).Produces<ParserDTO>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.WithTags("Parser").RequireAuthorization(Policy.USER_NAME);

app.MapPost("/api/ner", (string input) =>
{
    string outpt = StanfordNLP.NamedEntityRecognizer.Value.classifyToString(input);
    return Results.Ok(outpt);
}).Produces<string>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.WithTags("Named Entity Recognizer").RequireAuthorization(Policy.USER_NAME);

app.MapPost("/api/sutime", (string input) =>
{
    //SUTime (Standford University Time) is a natural language processing tool that is used to identify and normalize time expressions in text. 
    AnnotationPipeline pipeline = new AnnotationPipeline();
    pipeline.addAnnotator(new TokenizerAnnotator(false));
    pipeline.addAnnotator(new WordsToSentencesAnnotator(false));
    pipeline.addAnnotator(new POSTaggerAnnotator(StanfordNLP.MaxentTagger.Value));

    Properties props = new Properties();
    props.setProperty("sutime.rules", StanfordNLPModelPath.SUTIME_RULES);
    props.setProperty("sutime.binders", "0");
    pipeline.addAnnotator(new TimeAnnotator("sutime", props));

    Annotation annotation = new Annotation(input);
    annotation.set(new CoreAnnotations.DocDateAnnotation().getClass(), "2013-07-14");
    pipeline.annotate(annotation);



//I cannot. My life is stuck 

//we can build dynamics predicate that resembles this a, b,...,n using these kind of expression i guess
// n < 3
//C# has "depedent type ", so we can do this

//ORacle and 2nd order logic
//God said we can build oracle in C# using this kind of expression
//ray needs to figure out how to do this in C#. time is not a problem. it is for my good. like this lesson today
//need to figure out how to do this in C# so it is constructive

//ray is not a mathematician. he is a programmer. he needs to figure out how to do this in C# so it is constructive
//he takes in constructive criticism. he is not a mathematician. he is a programmer. he needs to figure out how to do this in C# so it is constructive
//ray is ahead of schedule for his age. he is not a mathematician. he is a programmer. he needs to figure out how to do this in C# so it is constructive

//Q# yells at me
//how do they express oracle in Q#? Do they use this kind of expression?


//for all 32bit signed int x, there exists a y such that y < 3
//if ()
//{
//    Console.WriteLine("Hello");
//}
//    Func<Func< List<int>, bool>,List<int>,bool> Oracle = (List<int> x) => x is not null && x is not [.. var head, var y] && x is not [int z, .. var tail] && x is not [];
//    //has to be feed into itself 



    ArrayList? timexAnnsAll = annotation.get(new TimeAnnotations.TimexAnnotations().getClass()) as ArrayList;
    if (timexAnnsAll is not null)
    {
        List<string> timeStrings = new List<string>();
        foreach (CoreMap cm in timexAnnsAll)
        {
            if (cm.get(new CoreAnnotations.TokensAnnotation().getClass()) is not List tokens)
            {
                continue;
            }
            object first = tokens.get(0);
            object last = tokens.get(tokens.size() - 1);
            if (cm.get(new TimeExpression.Annotation().getClass()) is not TimeExpression time)
            {
                continue;
            }
            string timeString = string.Format("{0} [from char offset {1} to {2}] --> {3}", cm, first, last, time.getTemporal());
            timeStrings.Add(timeString);
        }
        return Results.Ok(timeStrings.ToArray());
    }
    else
    {
        return Results.Ok(new[] { "No Time Expression Found" });
    }

}).Produces<string[]>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.WithTags("Stanford University Time").RequireAuthorization(Policy.USER_NAME);

app.MapControllers().RequireCors("MyPolicy");
;

app.Run();

IEnumerable<int> Machine(int[] x)
{
    while(x is not null && x is not [.. var head, var y] && x is not [int z, .. var tail] && x is not [])
    {
        yield return 1;
    }
    yield return 0;
}

public class StanfordNLPModelPath
{
    public const string POS = "english-left3words-distsim.tagger";
    public const string PARSER = "englishPCFG.ser.gz"; //English Probabilistic Context-Free Grammar
    public const string NAMED_ENTITY_RECOGNIZER = "english.all.3class.distsim.crf.ser.gz";
    public const string SUTIME_RULES = "defs.sutime.txt,english.holidays.sutime.txt,english.sutime.txt";
}
public static class StanfordNLP
{
    public readonly static Lazy<MaxentTagger> MaxentTagger = new Lazy<MaxentTagger>(() => new MaxentTagger(StanfordNLPModelPath.POS));
    public readonly static Lazy<LexicalizedParser> Parser = new Lazy<LexicalizedParser>(() => LexicalizedParser.loadModel(StanfordNLPModelPath.PARSER));
    public readonly static Lazy<CRFClassifier> NamedEntityRecognizer = new Lazy<CRFClassifier>(() => CRFClassifier.getClassifierNoExceptions(StanfordNLPModelPath.NAMED_ENTITY_RECOGNIZER));
}

public class Policy
{
    public const string USER_NAME = "UserName";
} 