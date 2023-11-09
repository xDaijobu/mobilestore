using HtmlAgilityPack;

var builder = WebApplication.CreateBuilder(args);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapGet("/getLatestVersion", async (string packageName) => await GetPlaystoreNumber(packageName));

app.MapGet("/getLatestReleaseNotes", async (string packageName) => await GetLatestReleaseNotes(packageName));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();

#region 
Task<string> GetPlaystoreNumber(string packageName)
=> GetStoreVersion("[1][2][140][0][0][0]", packageName);

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