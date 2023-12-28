using System.Text;
using System.Text.Json.Nodes;
using HtmlAgilityPack;

var builder = WebApplication.CreateBuilder(args);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Logging.AddConsole();
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
app.MapGet("/getLatestVersion/{platform}/{packageName}/{label}", async Task<Response> (string platform, string packageName, string label) =>
{
    var versionNumber = "";
    if (platform.ToLower() == "ios")
        versionNumber = await GetiOSLatestVersionNumber(packageName);
    else
        versionNumber = await GetAndroidLatestVersionNumber(packageName);

    return new Response(1, label, versionNumber, "white");
});

app.MapGet("/getLatestReleaseNotes/{platform}/{packageName}", async Task<string> (string platform, string packageName) =>
{
    string result = "";
    if (platform.ToLower() == "ios")
        result = await GetiOSLatestReleaseNotes(packageName);
    else
        result = await GetAndroidLatestReleaseNotes(packageName);

    return result;
});

app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
        string.Join("\n", endpointSources.SelectMany(source => source.Endpoints)));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();

#region Android
Task<string> GetAndroidLatestVersionNumber(string packageName)
   => GetStoreVersion("[1][2][140][0][0][0]", packageName);

Task<string> GetAndroidLatestReleaseNotes(string packageName)
    => GetStoreVersion("[1][2][144][1][1]", packageName);

async Task<string> GetStoreVersion(string magicNumber, string packageName)
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
        throw new Exception($"Error parsing content from the Play Store. Url={url}.", ex);
    }
}
#endregion

#region iOS
async Task<string> GetiOSLatestVersionNumber(string bundleId)
{
    var app = await LookupApp(bundleId);
    return app?.Version ?? "";
}

async Task<string> GetiOSLatestReleaseNotes(string bundleId)
{
    var app = await LookupApp(bundleId);
    return app?.ReleaseNotes ?? "";
}

async Task<AppiOS?> LookupApp(string bundleId)
{
    try
    {
        string platformCountryCode = "id";
        using var http = new HttpClient();
        string url = $"http://itunes.apple.com/lookup?id={bundleId}&country={platformCountryCode}";
        var response = await http.GetAsync(url);
        var content = response.Content == null ? null : await response.Content.ReadAsStringAsync();

        if (string.IsNullOrEmpty(content))
            return null;

        var appLookup = JsonValue.Parse(content);

        return new AppiOS(appLookup["results"][0]["version"].ToString(),
                          appLookup["results"][0]["trackViewUrl"].ToString(),
                          appLookup["results"][0]["releaseNotes"].ToString());
    }
    catch (Exception e)
    {
        throw new Exception($"Error looking up app details in App Store. BundleIdentifier={bundleId}.", e);
    }
}
#endregion


record Response(int SchemaVersion, string Label, string Message, string Color);
record AppiOS(string Version, string Url, string ReleaseNotes);