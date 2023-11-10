using System.Text;
using HtmlAgilityPack;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddJsonConsole();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

var app = builder.Build();
app.MapGet("/", () => "Hello World!");
app.MapGet("/getLatestVersion", async (string label, string packageName) => await GetPlaystoreNumber(label, packageName));

app.MapGet("/getLatestReleaseNotes", async (string packageName) => await GetLatestReleaseNotes(packageName));

app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
        string.Join("\n", endpointSources.SelectMany(source => source.Endpoints)));

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.Run();

#region 
async Task<Response> GetPlaystoreNumber(string label, string packageName)
{
    var version = await GetStoreVersion("[1][2][140][0][0][0]", packageName);
    return new Response(1, label, version, "white");
}

Task<string> GetLatestReleaseNotes(string packageName)
    => GetStoreVersion("[1][2][144][1][1]", packageName);

async Task<string> GetStoreVersion(string magicNumber, string packageName = "")
{
    var version = string.Empty;
    var url = $"https://play.google.com/store/apps/details?id={packageName}&hl=en";

    try
    {
        var htmlWeb = new HtmlWeb();
        var htmlDoc = await htmlWeb.LoadFromWebAsync(url);
        var script = htmlDoc.DocumentNode.Descendants()
                                         .Where(n => n.Name == "script" && n.InnerText.Contains("AF_initDataCallback({key: 'ds:5'"))
                                         .FirstOrDefault()?.InnerText;
        var engine = new Jurassic.ScriptEngine();
        var eval = "(function() { var AF_initDataCallback = function(p) { return p.data" + magicNumber + "; };  return " + script + " })()";
        var result = engine.Evaluate(eval);

        return result is null ? string.Empty : result.ToString();
    }
    catch (Exception ex)
    {
        return "salah";
        throw new Exception($"Error parsing content from the Play Store. Url={url}.", ex);
    }
}
#endregion



record Response(int SchemaVersion, string Label, string Message, string Color);
record Route(string Name, string HttpMetods, string RawText, string PathSegments, string Parameters, string InboundPrecedence, string OutboundPrecedence);