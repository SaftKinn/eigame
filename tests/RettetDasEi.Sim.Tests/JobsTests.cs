using RettetDasEi.Sim;

namespace RettetDasEi.Sim.Tests;

/// <summary>
/// The food's jobs (ADR 003), with TestContent's entries: a bomb (damage 50, radius 100, push 400),
/// a sticky block (tears at 400), a wall (takes 25 % of a push), a shooter that pierces 3.
/// </summary>
public class JobsTests
{
    static readonly ContentDb Db = TestContent.Load();
    static readonly BombDef TheBomb = Db.Unit("bomb").Bomb!;

    // ------------------------------------------------------------------ bomb

    [Theory]
    [InlineData(0f, 20f, true)]
    [InlineData(120f, 20f, true)]   // edge exactly at the radius: 100 + 20
    [InlineData(120.5f, 20f, false)]
    [InlineData(140f, 52f, true)]   // a big pest reaches in further
    public void A_pest_is_hit_when_its_edge_lies_within_the_radius(float distance, float pestRadius, bool hit) =>
        Assert.Equal(hit, Bomb.Hits(TheBomb, distance, pestRadius));

    [Fact]
    public void Own_food_gets_the_whole_push_at_the_centre_and_less_further_out()
    {
        Assert.Equal(400f, Bomb.PushAt(TheBomb, 0f, pieceRadius: 50f));
        Assert.Equal(200f, Bomb.PushAt(TheBomb, 75f, pieceRadius: 50f)); // halfway to 150
        Assert.Equal(0f, Bomb.PushAt(TheBomb, 150f, pieceRadius: 50f));
        Assert.Equal(0f, Bomb.PushAt(TheBomb, 400f, pieceRadius: 50f));
    }

    // ------------------------------------------------------------------ glue

    [Theory]
    [InlineData(110f, false)] // an ant's push
    [InlineData(399.9f, false)]
    [InlineData(400f, true)]
    [InlineData(1500f, true)] // the pudding's
    public void Only_a_hard_blow_tears_the_glue(float impulse, bool tears) =>
        Assert.Equal(tears, Glue.Tears(Db.Unit("sticky").Glue!, impulse));

    // ---------------------------------------------------------------- shield

    [Fact]
    public void A_shield_takes_only_its_share_of_a_push_and_other_food_all_of_it()
    {
        Assert.Equal(120f, Shield.PushOn(Db.Unit("wall").Shield, 480f));
        Assert.Equal(480f, Shield.PushOn(null, 480f));
    }

    // ---------------------------------------------------------------- flight

    [Theory]
    [InlineData(0f, false)]
    [InlineData(1.99f, false)]
    [InlineData(2f, true)]   // TestContent: flyPatience 2
    [InlineData(5f, true)]
    public void A_flyer_bites_what_is_in_its_way_once_its_patience_is_up(float blocked, bool givesUp) =>
        Assert.Equal(givesUp, Flight.GivesUp(Db.Game, blocked));

    // ---------------------------------------------------------------- pierce

    [Fact]
    public void A_piercing_shot_flies_on_until_it_has_hit_its_number_of_pests()
    {
        var attack = Db.Unit("shooter").Attack!;

        Assert.True(Pierce.FliesOn(attack, hits: 1));
        Assert.True(Pierce.FliesOn(attack, hits: 2));
        Assert.False(Pierce.FliesOn(attack, hits: 3));
    }

    [Fact]
    public void A_shot_without_pierce_stops_at_the_first_pest() =>
        Assert.False(Pierce.FliesOn(Db.Unit("pepper").Attack!, hits: 1));
}
