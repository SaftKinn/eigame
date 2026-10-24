using RettetDasEi.Sim;

namespace RettetDasEi.Sim.Tests;

/// <summary>The pepper's itch and sneeze, the mustard's puddles (TestContent: 3 hits, a pause of 1.5 s; slow 0.4).</summary>
public class PestEffectsTests
{
    static readonly ContentDb Db = TestContent.Load();
    static readonly ItchDef Itch = Db.Unit("pepper").Attack!.Itch!;
    static readonly PuddleDef Puddle = Db.Unit("lobber").Attack!.Puddle!;

    [Fact]
    public void A_pest_sneezes_at_the_last_hit_of_its_itch_and_the_itch_starts_again()
    {
        var pest = new PestEffects();

        Assert.False(pest.AddItch(Itch));
        Assert.False(pest.AddItch(Itch));
        Assert.Equal(2f / 3f, pest.ItchShare, 5);

        Assert.True(pest.AddItch(Itch));
        Assert.True(pest.Sneezing);
        Assert.Equal(1.5f, pest.PauseLeft);
        Assert.Equal(0, pest.Itch);
        Assert.Equal(0f, pest.ItchShare);
    }

    [Fact]
    public void No_itch_builds_while_it_sneezes()
    {
        var pest = Sneezed();

        Assert.False(pest.AddItch(Itch));
        Assert.Equal(0, pest.Itch);
    }

    [Fact]
    public void The_pause_ends_when_its_time_is_up_and_itching_counts_again()
    {
        var pest = Sneezed();

        pest.Step(1f);
        Assert.True(pest.Sneezing);
        pest.Step(0.5f);
        Assert.False(pest.Sneezing);
        Assert.Equal(0f, pest.PauseLeft);

        Assert.False(pest.AddItch(Itch));
        Assert.Equal(1, pest.Itch);
    }

    [Fact]
    public void One_long_step_ends_the_pause_like_many_short_ones()
    {
        var once = Sneezed();
        var often = Sneezed();

        once.Step(2f);
        for (int i = 0; i < 20; i++)
            often.Step(0.1f);

        Assert.Equal(once.PauseLeft, often.PauseLeft);
        Assert.False(often.Sneezing);
    }

    [Fact]
    public void Outside_every_puddle_a_pest_keeps_its_speed()
    {
        var pest = new PestEffects();

        Assert.Equal(1f, pest.SpeedFactor);
    }

    [Fact]
    public void Puddles_do_not_stack_the_strongest_counts()
    {
        var weaker = new PuddleDef { Radius = 50f, Seconds = 3f, Slow = 0.7f };
        var pest = new PestEffects();

        pest.InPuddle(weaker);
        pest.InPuddle(Puddle);
        pest.InPuddle(Puddle);
        pest.InPuddle(weaker);

        Assert.Equal(0.4f, pest.SpeedFactor);
    }

    [Fact]
    public void Leaving_the_puddles_gives_the_speed_back()
    {
        var pest = new PestEffects();
        pest.InPuddle(Puddle);

        pest.ClearPuddles();

        Assert.Equal(1f, pest.SpeedFactor);
    }

    static PestEffects Sneezed()
    {
        var pest = new PestEffects();
        for (int i = 0; i < Itch.Hits; i++)
            pest.AddItch(Itch);
        Assert.True(pest.Sneezing);
        return pest;
    }
}
