public static class SimulationArgs
{
    public static void AddNamedArg(List<string> argsList, string name, object value)
    {
        argsList.Add($"--{name}");
        argsList.Add(value.ToString()!);
    }

    public static List<string> GeneratePerformanceCalculatorArgs(SimulateRequestOsu request, string? filePath = null)
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

    public static List<string> GeneratePerformanceCalculatorArgsDTHT(SimulateRequestOsu request, string? filePath = null)
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

    public static List<string> GeneratePerformanceCalculatorArgs(SimulateRequestTaiko request, string? filePath = null)
    {
        var simulateArgs = new List<string>
        {
            "simulate", "taiko"
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

    public static List<string> GeneratePerformanceCalculatorArgs(SimulateRequestCtb request, string? filePath = null)
    {
        var simulateArgs = new List<string>
        {
            "simulate", "catch"
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
        AddNamedArg(simulateArgs, "droplets", request.droplets);
        AddNamedArg(simulateArgs, "tiny-droplets", request.tiny_droplets);
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

    public static List<string> GeneratePerformanceCalculatorArgs(SimulateRequestMania request, string? filePath = null)
    {
        var simulateArgs = new List<string>
        {
            "simulate", "mania"
        };
        foreach (var mod in request.mods)
        {
            AddNamedArg(simulateArgs, "mod", mod);
        }
        AddNamedArg(simulateArgs, "score", request.score);
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
}
