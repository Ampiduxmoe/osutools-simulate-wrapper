using FsBeatmapProcessor;
using static SimulationArgs;
using static BeatmapDifficulty;

static string GetSimulationJson(List<string> simulationArgs, object simulationLock)
{
    lock (simulationLock)
    {
        var prevOut = Console.Out;
        var responseWriter = new StringWriter();
        Console.SetOut(responseWriter);
        PerformanceCalculator.Program.Main([..simulationArgs]);
        var output = responseWriter.ToString();
        var jsonStartIndex = output.IndexOf('{');
        var jsonEndIndex = output.LastIndexOf('}');
        var jsonLength = jsonEndIndex - jsonStartIndex + 1;
        output = output.Substring(jsonStartIndex, jsonLength);
        Console.SetOut(prevOut);
        return output;
    }
}

static string GetBeatmapFilePath(int id) => $"cache/{id}.osu";
static string GetTmpBeatmapFilePath(int id) => "tmp.osu";

static async Task DownloadBeatmapIfNeeded(int id, HttpClient httpClient)
{
    var beatmapFilePath = GetBeatmapFilePath(id);
    if (!File.Exists(beatmapFilePath))
    {
        var beatmapFile = await httpClient.GetByteArrayAsync($"https://osu.ppy.sh/osu/{id}");
        File.WriteAllBytes(beatmapFilePath, beatmapFile);
    }
}

static Beatmap GetTmpBeatmap(int id)
{
    var beatmapFilePath = GetBeatmapFilePath(id);
    var tmpBeatmapFilePath = GetTmpBeatmapFilePath(id);
    if (File.Exists(tmpBeatmapFilePath))
    {
        File.Delete(tmpBeatmapFilePath);
    }
    File.Copy(beatmapFilePath, tmpBeatmapFilePath);
    return new Beatmap(tmpBeatmapFilePath);
}


var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

object simulationLock = new();
object mapEditingLock = new();
var requestCount = 0;
var httpClient = new HttpClient();

app.MapPost("/simulate/osu", async (SimulateRequestOsu request) =>
{
    var requestNumber = ++requestCount;
    Console.WriteLine($"[{requestNumber}] POST: /simulate/osu ({request})");
    var beatmapId = request.beatmap_id;
    await DownloadBeatmapIfNeeded(beatmapId, httpClient);
    string response;
    lock (mapEditingLock)
    {
        var tmpBeatmap = GetTmpBeatmap(beatmapId);
        var beatmapChanged = ApplyMods(
            tmpBeatmap, 
            request.mods.Where(m => m.ToLower() == "da"), 
            request.da_settings
        );
        if (beatmapChanged)
        {
            tmpBeatmap.Save();
        }
        var tmpBeatmapFilePath = GetTmpBeatmapFilePath(beatmapId);
        var args = GeneratePerformanceCalculatorArgs(request, tmpBeatmapFilePath);
        response = GetSimulationJson(args, simulationLock);
        File.Delete(tmpBeatmapFilePath);
    }
    Console.WriteLine($"[{requestNumber}] Response:\n{response}");
    return response;
});
app.MapPost("/simulate/osu/dt", async (SimulateRequestOsuDt request) =>
{
    var requestNumber = ++requestCount;
    Console.WriteLine($"[{requestNumber}] POST: /simulate/osu/dt ({request})");
    var beatmapId = request.beatmap_id;
    await DownloadBeatmapIfNeeded(beatmapId, httpClient);
    string response;
    lock (mapEditingLock)
    {
        var tmpBeatmap = GetTmpBeatmap(beatmapId);
        var beatmapChanged = ApplyMods(tmpBeatmap, request.mods, request.da_settings);
        var dtRate = request.dt_rate;
        var newBaseRate = dtRate / 1.5M;
        if (Math.Abs(newBaseRate - 1M) > 0.001M)
        {
            ChangeBeatmapSpeed(tmpBeatmap, newBaseRate);
            beatmapChanged = true;
        }
        if (beatmapChanged)
        {
            tmpBeatmap.Save();
        }
        var tmpBeatmapFilePath = GetTmpBeatmapFilePath(beatmapId);
        var args = GeneratePerformanceCalculatorArgsDTHT(request, tmpBeatmapFilePath);
        response = GetSimulationJson(args, simulationLock);
        File.Delete(tmpBeatmapFilePath);
    }
    Console.WriteLine($"[{requestNumber}] Response:\n{response}");
    return response;
});
app.MapPost("/simulate/osu/ht", async (SimulateRequestOsuHt request) =>
{
    var requestNumber = ++requestCount;
    Console.WriteLine($"[{requestNumber}] POST: /simulate/osu/ht ({request})");
    var beatmapId = request.beatmap_id;
    await DownloadBeatmapIfNeeded(beatmapId, httpClient);
    string response;
    lock (mapEditingLock)
    {
        var tmpBeatmap = GetTmpBeatmap(beatmapId);
        var beatmapChanged = ApplyMods(tmpBeatmap, request.mods, request.da_settings);
        var htRate = request.ht_rate;
        var newBaseRate = htRate / 0.75M;
        if (Math.Abs(newBaseRate - 1M) > 0.001M)
        {
            ChangeBeatmapSpeed(tmpBeatmap, newBaseRate);
            beatmapChanged = true;
        }
        if (beatmapChanged)
        {
            tmpBeatmap.Save();
        }
        var tmpBeatmapFilePath = GetTmpBeatmapFilePath(beatmapId);
        var args = GeneratePerformanceCalculatorArgsDTHT(request, tmpBeatmapFilePath);
        response = GetSimulationJson(args, simulationLock);
        File.Delete(tmpBeatmapFilePath);
    }
    Console.WriteLine($"[{requestNumber}] Response:\n{response}");
    return response;
});
app.MapPost("/simulate/taiko", async (SimulateRequestTaiko request) =>
{
    var requestNumber = ++requestCount;
    Console.WriteLine($"[{requestNumber}] POST: /simulate/taiko ({request})");
    var beatmapId = request.beatmap_id;
    await DownloadBeatmapIfNeeded(beatmapId, httpClient);
    string response;
    lock (mapEditingLock)
    {
        var args = GeneratePerformanceCalculatorArgs(request);
        response = GetSimulationJson(args, simulationLock);
    }
    Console.WriteLine($"[{requestNumber}] Response:\n{response}");
    return response;
});
app.MapPost("/simulate/ctb", async (SimulateRequestCtb request) =>
{
    var requestNumber = ++requestCount;
    Console.WriteLine($"[{requestNumber}] POST: /simulate/ctb ({request})");
    var beatmapId = request.beatmap_id;
    await DownloadBeatmapIfNeeded(beatmapId, httpClient);
    string response;
    lock (mapEditingLock)
    {
        var args = GeneratePerformanceCalculatorArgs(request);
        response = GetSimulationJson(args, simulationLock);
    }
    Console.WriteLine($"[{requestNumber}] Response:\n{response}");
    return response;
});
app.MapPost("/simulate/mania", async (SimulateRequestMania request) =>
{
    var requestNumber = ++requestCount;
    Console.WriteLine($"[{requestNumber}] POST: /simulate/mania ({request})");
    var beatmapId = request.beatmap_id;
    await DownloadBeatmapIfNeeded(beatmapId, httpClient);
    string response;
    lock (mapEditingLock)
    {
        var args = GeneratePerformanceCalculatorArgs(request);
        response = GetSimulationJson(args, simulationLock);
    }
    Console.WriteLine($"[{requestNumber}] Response:\n{response}");
    return response;
});
app.MapGet("/simulate/status", () =>
{
    return "ok";
});

app.Run();
