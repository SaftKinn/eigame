namespace RettetDasEi.Sim;

/// <summary>One pest of a wave: when it comes (seconds after the start), what it is, from which side.</summary>
public readonly record struct Spawn(float Time, string Enemy, float Dir);

public static class WaveSchedule
{
    /// <summary>
    /// Every pest of a wave, sorted by time: a group's i-th comes at <c>start + i × interval</c>;
    /// "left" walks right (+1), "right" walks left (−1), "both" alternates, the first from the right.
    /// </summary>
    public static List<Spawn> Build(WaveDef wave)
    {
        var spawns = new List<Spawn>();
        foreach (var group in wave.Groups)
        {
            for (int i = 0; i < group.Count; i++)
            {
                float dir = group.Side switch
                {
                    "left" => 1f,
                    "right" => -1f,
                    _ => i % 2 == 0 ? -1f : 1f,
                };
                spawns.Add(new Spawn(group.Start + i * group.Interval, group.Enemy, dir));
            }
        }
        // The same List.Sort as the prototype's queue: equal times keep the order they had there.
        spawns.Sort((a, b) => a.Time.CompareTo(b.Time));
        return spawns;
    }
}
