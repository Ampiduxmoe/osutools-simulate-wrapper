using FsBeatmapProcessor;

static void AddNamedArg(List<string> argsList, string name, object value)
{
    argsList.Add($"--{name}");
    argsList.Add(value.ToString()!);
}

static List<string> GeneratePerformanceCalculatorArgs(SimulateRequest request, string? filePath = null)
{
    var simulateArgs = new List<string>
    {
        "simulate", "osu"
    };
    foreach (var mod in request.mods)
    {
        AddNamedArg(simulateArgs, "mod", mod);
    }
    if (request.combo != null)
    {
        AddNamedArg(simulateArgs, "combo", request.combo);
    }
    AddNamedArg(simulateArgs, "misses", request.misses);
    AddNamedArg(simulateArgs, "mehs", request.mehs);
    AddNamedArg(simulateArgs, "goods", request.goods);
    simulateArgs.Add("--json");
    if (filePath != null)
    {
        simulateArgs.Add(filePath); 
    }
    else
    {
        simulateArgs.Add(request.beatmap_id.ToString()); 
    } 
    return simulateArgs;  
}

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

static void ChangeBeatmapSpeed(Beatmap map, decimal speedMultiplier) {
    var ar = map.ApproachRate;
    var od = map.OverallDifficulty;
    map.SetRate(speedMultiplier);
    map.ApproachRate = BeatmapDifficulty.CalculateMultipliedAR(ar, speedMultiplier);
    map.OverallDifficulty = BeatmapDifficulty.CalculateMultipliedOD(od, speedMultiplier);
}

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

object simulationLock = new();
object mapEditingLock = new();
var httpClient = new HttpClient();

app.MapPost("/simulate", (SimulateRequest request) =>
{
    Console.WriteLine($"POST: /simulate ({request})");
    var args = GeneratePerformanceCalculatorArgs(request);
    string response = GetSimulationJson(args, simulationLock);
    Console.WriteLine("Response:");
    Console.WriteLine(response);
    return response;
});
app.MapPost("/simulate/dt", async (SimulateDtRequest request) =>
{
    Console.WriteLine($"POST: /simulate/dt ({request})");
    var beatmapId = request.beatmap_id;
    var beatmapFilePath = $"cache/{beatmapId}.osu";
    if (!File.Exists(beatmapFilePath))
    {
        var beatmapFile = await httpClient.GetByteArrayAsync($"https://osu.ppy.sh/osu/{beatmapId}");
        File.WriteAllBytes(beatmapFilePath, beatmapFile);
    }
    string response;
    lock (mapEditingLock)
    {
        var tmpBeatmapFilePath = "tmp.osu";
        if (File.Exists(tmpBeatmapFilePath))
        {
            File.Delete(tmpBeatmapFilePath);
        }
        File.Copy(beatmapFilePath, tmpBeatmapFilePath);
        var beatmap = new Beatmap(tmpBeatmapFilePath);
        var dtRate = request.dt_rate;
        var newBaseRate = dtRate / 1.5M;
        if (Math.Abs(newBaseRate - 1M) > 0.001M)
        {
            ChangeBeatmapSpeed(beatmap, newBaseRate);
            beatmap.Save();
        }
        var args = GeneratePerformanceCalculatorArgs(request, tmpBeatmapFilePath);
        response = GetSimulationJson(args, simulationLock);
        File.Delete(tmpBeatmapFilePath);
    }
    Console.WriteLine("Response:");
    Console.WriteLine(response);
    return response;
});
app.MapGet("/simulate/status", () =>
{
    return "ok";
});

app.Run();

record SimulateRequest(
    string[] mods,
    int? combo,
    int misses,
    int mehs,
    int goods,
    int beatmap_id
) {
    public override string ToString()
    {
        return $"SimulateRequest {{ mods = [{string.Join(", ", mods)}], combo = {combo}, misses = {misses}, mehs = {mehs}, goods = {goods}, beatmap_id = {beatmap_id} }}";
    }
};

record SimulateDtRequest(
    decimal dt_rate,
    string[] mods,
    int? combo,
    int misses,
    int mehs,
    int goods,
    int beatmap_id
) : SimulateRequest(mods, combo, misses, mehs, goods, beatmap_id)
{
    public override string ToString()
    {
        return $"SimulateDtRequest {{ dt_rate = {dt_rate}, mods = [{string.Join(", ", mods)}], combo = {combo}, misses = {misses}, mehs = {mehs}, goods = {goods}, beatmap_id = {beatmap_id} }}";
    }
};