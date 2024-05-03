public static class BeatmapDifficulty {
    public const decimal MIN_AR = 0M;
    public const decimal MAX_AR = 11.5M;
    public const decimal MIN_OD = 0M;
    public const decimal MAX_OD = 11M + 2M/3M;
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
}