using RettetDasEi.Sim;

namespace RettetDasEi.Sim.Tests;

/// <summary>Higher up shoots further (TestContent: the shooter's range 400, rangePerHeight 0.5).</summary>
public class ReachTests
{
    static readonly ContentDb Db = TestContent.Load();
    static readonly AttackDef Attack = Db.Unit("shooter").Attack!;

    [Fact]
    public void On_the_board_a_shooter_has_its_own_range() =>
        Assert.Equal(400f, Reach.Of(Attack, Db.Game, heightAboveBoard: 0f));

    [Fact]
    public void Every_pixel_above_the_board_adds_range_per_height() =>
        Assert.Equal(450f, Reach.Of(Attack, Db.Game, heightAboveBoard: 100f));

    [Fact]
    public void Below_the_board_there_is_no_less_range() =>
        Assert.Equal(400f, Reach.Of(Attack, Db.Game, heightAboveBoard: -30f));
}
