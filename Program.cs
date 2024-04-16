static void AddNamedArg(List<string> argsList, string name, object value)
{
    argsList.Add($"--{name}");
    argsList.Add(value.ToString()!);
}
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var consoleOut = Console.Out;
object sharedLock = new();
app.MapPost("/simulate", (SimulateRequest request) =>
{
    consoleOut.WriteLine($"POST: /simulate ({request})");

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
    simulateArgs.Add(request.beatmap_id.ToString());

    string response;
    lock (sharedLock) 
    {
        var responseWriter = new StringWriter();
        Console.SetOut(responseWriter);
        PerformanceCalculator.Program.Main([..simulateArgs]);
        response = responseWriter.ToString();
        var jsonStartIndex = response.IndexOf('{');
        var jsonEndIndex = response.LastIndexOf('}');
        var jsonLength = jsonEndIndex - jsonStartIndex + 1;
        response = response.Substring(jsonStartIndex, jsonLength);
    }
    consoleOut.WriteLine("Response:");
    consoleOut.WriteLine(response);

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