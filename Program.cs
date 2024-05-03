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

static List<string> GeneratePerformanceCalculatorArgsDTHT(SimulateRequest request, string? filePath = null)
{
    var simulateArgs = new List<string>
    {
        "simulate", "osu"
    };
    foreach (var mod in request.mods)
    {
        string[] modsToSkip = ["da", "hr", "ez"];
        if (modsToSkip.Any(m => m == mod.ToLower()))
        {
            continue;
        }
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

static void ChangeBeatmapSpeed(Beatmap map, decimal speedMultiplier)
{
    var ar = map.ApproachRate;
    var od = map.OverallDifficulty;
    map.SetRate(speedMultiplier);
    map.ApproachRate = BeatmapDifficulty.CalculateMultipliedAR(ar, speedMultiplier);
    map.OverallDifficulty = BeatmapDifficulty.CalculateMultipliedOD(od, speedMultiplier);
}

static void ApplyHR(Beatmap map)
{
    map.ApproachRate = BeatmapDifficulty.CalculateHardRockModAR(map.ApproachRate);
    map.CircleSize = BeatmapDifficulty.CalculateHardRockModCS(map.CircleSize);
    map.OverallDifficulty = BeatmapDifficulty.CalculateHardRockModOD(map.OverallDifficulty);
    map.HPDrainRate = BeatmapDifficulty.CalculateHardRockModHP(map.HPDrainRate);
}

static void ApplyEZ(Beatmap map)
{
    map.ApproachRate = BeatmapDifficulty.CalculateEasyModAR(map.ApproachRate);
    map.CircleSize = BeatmapDifficulty.CalculateEasyModCS(map.CircleSize);
    map.OverallDifficulty = BeatmapDifficulty.CalculateEasyModOD(map.OverallDifficulty);
    map.HPDrainRate = BeatmapDifficulty.CalculateEasyModHP(map.HPDrainRate);
}

static void ApplyDA(Beatmap map, DifficultyAdjustSettings da)
{
    map.ApproachRate = da.ar;
    map.CircleSize = da.cs;
    map.OverallDifficulty = da.od;
    map.HPDrainRate = da.hp;
}

static bool ApplyMods(Beatmap map, IEnumerable<string> mods, DifficultyAdjustSettings? da_settings)
{
    var beatmapChanged = false;
    if (mods.Any(m => m.ToLower() == "da"))
    {
        if (da_settings != null)
        {
            ApplyDA(map, da_settings);
            beatmapChanged = true;
        }
    }
    else if (mods.Any(m => m.ToLower() == "hr"))
    {
        ApplyHR(map);
        beatmapChanged = true;
    }
    else if (mods.Any(m => m.ToLower() == "ez"))
    {
        ApplyEZ(map);
        beatmapChanged = true;
    }
    return beatmapChanged;
}

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

object simulationLock = new();
object mapEditingLock = new();
var httpClient = new HttpClient();

app.MapPost("/simulate", async (SimulateRequest request) =>
{
    Console.WriteLine($"POST: /simulate ({request})");
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
    Console.WriteLine("Response:");
    Console.WriteLine(response);
    return response;
});
app.MapPost("/simulate/dt", async (SimulateDtRequest request) =>
{
    Console.WriteLine($"POST: /simulate/dt ({request})");
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
    Console.WriteLine("Response:");
    Console.WriteLine(response);
    return response;
});
app.MapPost("/simulate/ht", async (SimulateHtRequest request) =>
{
    Console.WriteLine($"POST: /simulate/ht ({request})");
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
    int misses,
    int mehs,
    int goods,
    int beatmap_id,
    int? combo = null,
    DifficultyAdjustSettings? da_settings = null
) {
    public override string ToString()
    {
        return $"SimulateRequest {{ mods = [{string.Join(", ", mods)}], combo = {combo}, misses = {misses}, mehs = {mehs}, goods = {goods}, beatmap_id = {beatmap_id} }}";
    }
};

record SimulateDtRequest(
    decimal dt_rate,
    string[] mods,
    int misses,
    int mehs,
    int goods,
    int beatmap_id,
    int? combo = null,
    DifficultyAdjustSettings? da_settings = null
) : SimulateRequest(mods, misses, mehs, goods, beatmap_id, combo, da_settings)
{
    public override string ToString()
    {
        return $"SimulateDtRequest {{ dt_rate = {dt_rate}, mods = [{string.Join(", ", mods)}], combo = {combo}, misses = {misses}, mehs = {mehs}, goods = {goods}, beatmap_id = {beatmap_id} }}";
    }
};

record SimulateHtRequest(
    decimal ht_rate,
    string[] mods,
    int misses,
    int mehs,
    int goods,
    int beatmap_id,
    int? combo = null,
    DifficultyAdjustSettings? da_settings = null
) : SimulateRequest(mods, misses, mehs, goods, beatmap_id, combo, da_settings)
{
    public override string ToString()
    {
        return $"SimulateHtRequest {{ ht_rate = {ht_rate}, mods = [{string.Join(", ", mods)}], combo = {combo}, misses = {misses}, mehs = {mehs}, goods = {goods}, beatmap_id = {beatmap_id} }}";
    }
};

record DifficultyAdjustSettings(
    decimal ar,
    decimal cs,
    decimal hp,
    decimal od
) {
    public override string ToString()
    {
        return $"DifficultyAdjustSettings {{ ar = {ar}, cs = {cs}, hp = {hp}, od = {od} }}";
    }
}