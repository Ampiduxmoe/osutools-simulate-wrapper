public record SimulateRequestOsu(
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
        return $"SimulateRequestOsu {{ mods = [{string.Join(", ", mods)}], misses = {misses}, mehs = {mehs}, goods = {goods}, beatmap_id = {beatmap_id} }}, combo = {combo}, da_settings = {da_settings}";
    }
};

public record SimulateRequestOsuDt(
    decimal dt_rate,
    string[] mods,
    int misses,
    int mehs,
    int goods,
    int beatmap_id,
    int? combo = null,
    DifficultyAdjustSettings? da_settings = null
) : SimulateRequestOsu(mods, misses, mehs, goods, beatmap_id, combo, da_settings)
{
    public override string ToString()
    {
        return $"SimulateRequestOsuDt {{ dt_rate = {dt_rate}, mods = [{string.Join(", ", mods)}], misses = {misses}, mehs = {mehs}, goods = {goods}, beatmap_id = {beatmap_id} }}, combo = {combo}, da_settings = {da_settings}";
    }
};

public record SimulateRequestOsuHt(
    decimal ht_rate,
    string[] mods,
    int misses,
    int mehs,
    int goods,
    int beatmap_id,
    int? combo = null,
    DifficultyAdjustSettings? da_settings = null
) : SimulateRequestOsu(mods, misses, mehs, goods, beatmap_id, combo, da_settings)
{
    public override string ToString()
    {
        return $"SimulateRequestOsuHt {{ ht_rate = {ht_rate}, mods = [{string.Join(", ", mods)}], misses = {misses}, mehs = {mehs}, goods = {goods}, beatmap_id = {beatmap_id} }}, combo = {combo}, da_settings = {da_settings}";
    }
};

public record DifficultyAdjustSettings(
    decimal? ar = null,
    decimal? cs = null,
    decimal? hp = null,
    decimal? od = null
) {
    public override string ToString()
    {
        return $"DifficultyAdjustSettings {{ ar = {ar}, cs = {cs}, hp = {hp}, od = {od} }}";
    }
}

public record SimulateRequestTaiko(
    string[] mods,
    int misses,
    int goods,
    int beatmap_id,
    int? combo = null
) {
    public override string ToString()
    {
        return $"SimulateRequestTaiko {{ mods = [{string.Join(", ", mods)}], misses = {misses}, goods = {goods}, beatmap_id = {beatmap_id} }}, combo = {combo}";
    }
}

public record SimulateRequestCtb(
    string[] mods,
    int misses,
    int droplets,
    int tiny_droplets,
    int beatmap_id,
    int? combo = null
) {
    public override string ToString()
    {
        return $"SimulateRequestTaiko {{ mods = [{string.Join(", ", mods)}], misses = {misses}, droplets = {droplets}, tiny_droplets = {tiny_droplets}, beatmap_id = {beatmap_id} }}, combo = {combo}";
    }
}

public record SimulateRequestMania(
    string[] mods,
    int score,
    int beatmap_id
) {
    public override string ToString()
    {
        return $"SimulateRequestTaiko {{ mods = [{string.Join(", ", mods)}], score = {score}, beatmap_id = {beatmap_id} }}";
    }
}