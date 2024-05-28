using FsBeatmapProcessor;

public static class BeatmapDifficulty
{
    public static decimal CalculateMultipliedAR(decimal ar, decimal speedMultiplier)
    {
        decimal newApproachDuration = ApproachRateToMs(ar) / speedMultiplier;
        decimal newAr = MsToApproachRate(newApproachDuration);
        return newAr;
    }
    public static decimal CalculateMultipliedOD(decimal od, decimal speedMultiplier)
    {
        decimal newHitWindowMs = OverallDifficultyToMs(od) / speedMultiplier;
        decimal newOd = MsToOverallDifficulty(newHitWindowMs);
        return newOd;
    }

    // https://osu.ppy.sh/wiki/en/Beatmap/Approach_rate#mod-effects
    public static decimal CalculateHardRockModAR(decimal ar) => Math.Min(ar * 1.4M, 10M);
    // https://osu.ppy.sh/wiki/en/Beatmap/Circle_size#mod-effects
    public static decimal CalculateHardRockModCS(decimal cs) => Math.Min(cs * 1.3M, 10M);
    // https://osu.ppy.sh/wiki/en/Beatmap/Overall_difficulty#mod-effects
    public static decimal CalculateHardRockModOD(decimal od) => Math.Min(od * 1.4M, 10M);
    // https://osu.ppy.sh/wiki/en/Beatmap/HP_drain_rate#mod-effects
    public static decimal CalculateHardRockModHP(decimal hp) => Math.Min(hp * 1.4M, 10M);

    // https://osu.ppy.sh/wiki/en/Beatmap/Approach_rate#mod-effects
    public static decimal CalculateEasyModAR(decimal ar) => ar / 2M;
    // https://osu.ppy.sh/wiki/en/Beatmap/Circle_size#mod-effects
    public static decimal CalculateEasyModCS(decimal cs) => cs / 2M;
    // https://osu.ppy.sh/wiki/en/Beatmap/Overall_difficulty#mod-effects
    public static decimal CalculateEasyModOD(decimal od) => od / 2M;
    // https://osu.ppy.sh/wiki/en/Beatmap/HP_drain_rate#mod-effects
    public static decimal CalculateEasyModHP(decimal hp) => hp / 2M;

    private static decimal ApproachRateToMs(decimal approachRate)
    {
        if (approachRate <= 5)
        {
            return 1800.0M - approachRate * 120.0M;
        }
        else
        {
            decimal remainder = approachRate - 5;
            return 1200.0M - remainder * 150.0M;
        }
    }
    private static decimal MsToApproachRate(decimal ms)
    {
        if (ms >= 1200)
        {
            return (1800.0M - ms) / 120;
        }
        else
        {
            return (1200.0M - ms) / 150 + 5;
        }
    }
    private static decimal OverallDifficultyToMs(decimal od) => 80.0M - 6.0M * od;
    private static decimal MsToOverallDifficulty(decimal ms) => (80.0M - ms) / 6.0M;

    public static void ApplyHR(Beatmap map)
    {
        map.ApproachRate = CalculateHardRockModAR(map.ApproachRate);
        map.CircleSize = CalculateHardRockModCS(map.CircleSize);
        map.OverallDifficulty = CalculateHardRockModOD(map.OverallDifficulty);
        map.HPDrainRate = CalculateHardRockModHP(map.HPDrainRate);
    }

    public static void ApplyEZ(Beatmap map)
    {
        map.ApproachRate = CalculateEasyModAR(map.ApproachRate);
        map.CircleSize = CalculateEasyModCS(map.CircleSize);
        map.OverallDifficulty = CalculateEasyModOD(map.OverallDifficulty);
        map.HPDrainRate = CalculateEasyModHP(map.HPDrainRate);
    }

    public static bool ApplyDA(Beatmap map, DifficultyAdjustSettings da)
    {
        var beatmapChanged = false;
        if (da.ar != null && da.ar != map.ApproachRate)
        {
            map.ApproachRate = (decimal)da.ar;
            beatmapChanged = true;
        }
        if (da.cs != null && da.cs != map.CircleSize)
        {
            map.CircleSize = (decimal)da.cs;
            beatmapChanged = true;
        }
        if (da.od != null && da.od != map.OverallDifficulty)
        {
            map.OverallDifficulty = (decimal)da.od;
            beatmapChanged = true;
        }
        if (da.hp != null && da.hp != map.HPDrainRate)
        {
            map.HPDrainRate = (decimal)da.hp;
            beatmapChanged = true;
        }
        return beatmapChanged;
    }

    public static bool ApplyMods(Beatmap map, IEnumerable<string> mods, DifficultyAdjustSettings? da_settings)
    {
        var beatmapChanged = false;
        if (mods.Any(m => m.ToLower() == "da"))
        {
            if (da_settings != null)
            {
                beatmapChanged |= ApplyDA(map, da_settings);
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

    public static void ChangeBeatmapSpeed(Beatmap map, decimal speedMultiplier)
    {
        var ar = map.ApproachRate;
        var od = map.OverallDifficulty;
        map.SetRate(speedMultiplier);
        map.ApproachRate = CalculateMultipliedAR(ar, speedMultiplier);
        map.OverallDifficulty = CalculateMultipliedOD(od, speedMultiplier);
    }
}