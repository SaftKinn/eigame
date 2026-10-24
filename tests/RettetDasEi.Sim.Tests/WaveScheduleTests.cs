using RettetDasEi.Sim;

namespace RettetDasEi.Sim.Tests;

public class WaveScheduleTests
{
    static WaveDef Wave(params SpawnGroupDef[] groups) => new() { Reward = 0, Groups = groups.ToList() };

    static SpawnGroupDef Group(string enemy, int count, float start, float interval, string side) =>
        new() { Enemy = enemy, Count = count, Start = start, Interval = interval, Side = side };

    [Fact]
    public void A_group_comes_at_start_plus_i_intervals()
    {
        var spawns = WaveSchedule.Build(Wave(Group("ant", 3, 1f, 1.5f, "left")));

        Assert.Equal(new[] { 1f, 2.5f, 4f }, spawns.Select(s => s.Time));
        Assert.All(spawns, s => Assert.Equal("ant", s.Enemy));
    }

    [Fact]
    public void Left_walks_right_and_right_walks_left()
    {
        Assert.All(WaveSchedule.Build(Wave(Group("ant", 2, 0f, 1f, "left"))), s => Assert.Equal(1f, s.Dir));
        Assert.All(WaveSchedule.Build(Wave(Group("ant", 2, 0f, 1f, "right"))), s => Assert.Equal(-1f, s.Dir));
    }

    [Fact]
    public void Both_alternates_and_the_first_comes_from_the_right()
    {
        var spawns = WaveSchedule.Build(Wave(Group("ant", 4, 0f, 1f, "both")));

        Assert.Equal(new[] { -1f, 1f, -1f, 1f }, spawns.Select(s => s.Dir));
    }

    [Fact]
    public void The_groups_are_merged_in_order_of_time()
    {
        var spawns = WaveSchedule.Build(Wave(
            Group("ant", 3, 1f, 2f, "left"),       // 1, 3, 5
            Group("meatball", 2, 2f, 2f, "right"))); // 2, 4

        Assert.Equal(new[] { 1f, 2f, 3f, 4f, 5f }, spawns.Select(s => s.Time));
        Assert.Equal(new[] { "ant", "meatball", "ant", "meatball", "ant" }, spawns.Select(s => s.Enemy));
    }

    [Fact]
    public void Every_shipped_wave_comes_exactly_as_the_prototype_queued_it()
    {
        // M1 moved the queue without a change in behaviour (ADR 002). The reference below is the
        // prototype's code from Main.TryStartWave as it was (commit 3c88904). It matters where two
        // pests come at the same time: List.Sort is not stable, so the order is whatever that sort
        // made of the same input. Holds for whatever the waves are tuned to.
        var dir = Path.Combine(AppContext.BaseDirectory, "content");
        var db = ContentDb.Load(name => File.ReadAllText(Path.Combine(dir, name)));

        foreach (var wave in db.Waves)
        {
            var expected = PrototypeQueue(wave).Select(q => new Spawn(q.Time, q.Enemy, q.Dir));
            Assert.Equal(expected, WaveSchedule.Build(wave));
        }
    }

    static List<(float Time, string Enemy, float Dir)> PrototypeQueue(WaveDef wave)
    {
        var spawnQueue = new List<(float Time, string Enemy, float Dir)>();
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
                spawnQueue.Add((group.Start + i * group.Interval, group.Enemy, dir));
            }
        }
        spawnQueue.Sort((a, b) => a.Time.CompareTo(b.Time));
        return spawnQueue;
    }
}
