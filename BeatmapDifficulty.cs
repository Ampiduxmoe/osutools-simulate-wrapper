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