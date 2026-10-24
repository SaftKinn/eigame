using RettetDasEi.Sim;

namespace RettetDasEi.Sim.Tests;

/// <summary>
/// Gold, the shop and the phases of one game (TestContent: 100 gold, a block for 20, a shooter for
/// 30, an ant worth 4; wave 1 is three ants at 1, 3 and 5 s for a reward of 30, wave 2 the last).
/// </summary>
public class GameStateTests
{
    static GameState NewGame(out ContentDb db)
    {
        db = TestContent.Load();
        return new GameState(db);
    }

    static GameState InWave(out ContentDb db)
    {
        var game = NewGame(out db);
        Assert.True(game.TryStartWave(eggOnCounter: false));
        game.DrainEvents();
        return game;
    }

    // ------------------------------------------------------------- start and shop

    [Fact]
    public void A_new_game_begins_in_the_build_phase_with_the_start_gold()
    {
        var game = NewGame(out _);

        Assert.Equal(Phase.Build, game.Phase);
        Assert.Equal(100, game.Gold);
        Assert.Equal(0, game.WaveIndex);
        Assert.Equal(2, game.WaveCount);
    }

    [Fact]
    public void Buying_pays_the_price()
    {
        var game = NewGame(out var db);

        Assert.Equal(BuyResult.Bought, game.TryBuy(db.Unit("block")));
        Assert.Equal(80, game.Gold);
    }

    [Fact]
    public void Buying_without_the_gold_is_refused_and_costs_nothing()
    {
        var game = NewGame(out var db);
        for (int i = 0; i < 3; i++)
            game.TryBuy(db.Unit("shooter"));

        Assert.Equal(10, game.Gold);
        Assert.Equal(BuyResult.TooDear, game.TryBuy(db.Unit("block")));
        Assert.Equal(10, game.Gold);
    }

    [Fact]
    public void Buying_during_a_wave_is_refused_and_costs_nothing()
    {
        var game = InWave(out var db);

        Assert.Equal(BuyResult.NotBetweenWaves, game.TryBuy(db.Unit("block")));
        Assert.Equal(100, game.Gold);
    }

    [Fact]
    public void A_piece_still_in_the_hand_since_it_was_bought_sells_for_its_full_price()
    {
        var game = NewGame(out var db);
        game.TryBuy(db.Unit("block"));

        Assert.Equal(20, game.Sell(db.Unit("block"), justBought: true, hp: 100f, maxHp: 100f));
        Assert.Equal(100, game.Gold);
    }

    [Theory]
    [InlineData(100f, 10)] // 20 × 0.5 × 1
    [InlineData(25f, 2)]   // 2.5 rounds to even: 2
    [InlineData(75f, 8)]   // 7.5 rounds to even: 8
    [InlineData(0f, 0)]
    public void A_placed_piece_sells_for_the_share_of_its_health(float hp, int refund)
    {
        var game = NewGame(out var db);

        Assert.Equal(refund, game.RefundFor(db.Unit("block"), justBought: false, hp, maxHp: 100f));
        Assert.Equal(refund, game.Sell(db.Unit("block"), justBought: false, hp, maxHp: 100f));
        Assert.Equal(100 + refund, game.Gold);
    }

    [Fact]
    public void A_pest_killed_pays_its_gold()
    {
        var game = InWave(out var db);

        game.PestKilled(db.Enemy("ant"));

        Assert.Equal(104, game.Gold);
    }

    // ----------------------------------------------------------------- waves

    [Fact]
    public void A_wave_cannot_start_while_the_egg_lies_on_the_counter()
    {
        var game = NewGame(out _);

        Assert.False(game.CanStartWave(eggOnCounter: true));
        Assert.False(game.TryStartWave(eggOnCounter: true));
        Assert.Equal(Phase.Build, game.Phase);
        Assert.Empty(game.Events);
    }

    [Fact]
    public void Starting_a_wave_says_so_and_schedules_its_pests()
    {
        var game = NewGame(out _);

        Assert.True(game.TryStartWave(eggOnCounter: false));

        Assert.Equal(Phase.Wave, game.Phase);
        Assert.Equal(3, game.SpawnsLeft);
        Assert.Equal(new GameEvent[] { new GameEvent.WaveStarted(1) }, game.DrainEvents());
        Assert.False(game.CanStartWave(eggOnCounter: false));
    }

    [Fact]
    public void A_pest_is_due_when_its_time_has_come()
    {
        var game = InWave(out _);

        game.Step(0.9f);
        Assert.Empty(game.DrainEvents());

        game.Step(0.1f);
        Assert.Equal(new GameEvent[] { new GameEvent.SpawnDue("ant", -1f) }, game.DrainEvents());

        game.Step(4f);
        Assert.Equal(new GameEvent[] { new GameEvent.SpawnDue("ant", 1f), new GameEvent.SpawnDue("ant", -1f) }, game.DrainEvents());
        Assert.Equal(0, game.SpawnsLeft);
    }

    [Fact]
    public void One_big_step_releases_the_same_pests_as_many_small_ones()
    {
        var big = InWave(out _);
        var small = InWave(out _);

        big.Step(10f);
        var smallEvents = new List<GameEvent>();
        for (int i = 0; i < 100; i++)
        {
            small.Step(0.1f);
            smallEvents.AddRange(small.DrainEvents());
        }

        Assert.Equal(big.DrainEvents(), smallEvents);
    }

    [Fact]
    public void Outside_a_wave_a_step_does_nothing()
    {
        var game = NewGame(out _);

        game.Step(10f);

        Assert.Equal(0f, game.WaveTime);
        Assert.Empty(game.Events);
    }

    [Fact]
    public void A_wave_is_not_cleared_while_pests_are_alive_or_still_to_come()
    {
        var game = InWave(out _);

        game.CheckCleared(pestsAlive: 0); // three still to come
        game.Step(10f);
        game.DrainEvents();
        game.CheckCleared(pestsAlive: 1);

        Assert.Equal(Phase.Wave, game.Phase);
        Assert.Empty(game.Events);
    }

    [Fact]
    public void A_cleared_wave_pays_its_reward_and_a_build_phase_follows()
    {
        var game = InWave(out _);
        game.Step(10f);
        game.DrainEvents();

        game.CheckCleared(pestsAlive: 0);

        Assert.Equal(Phase.Build, game.Phase);
        Assert.Equal(130, game.Gold);
        Assert.Equal(1, game.WaveIndex);
        Assert.Equal(new GameEvent[] { new GameEvent.WaveCleared(1, 30) }, game.DrainEvents());
        Assert.True(game.CanStartWave(eggOnCounter: false));
    }

    [Fact]
    public void The_next_wave_comes_in_its_own_order_with_a_fresh_clock()
    {
        var game = InWave(out _);
        game.Step(10f);
        game.CheckCleared(pestsAlive: 0);
        game.DrainEvents();

        game.TryStartWave(eggOnCounter: false);
        game.Step(1f);

        Assert.Equal(1f, game.WaveTime);
        Assert.Equal(new GameEvent[]
        {
            new GameEvent.WaveStarted(2),
            new GameEvent.SpawnDue("ant", -1f), // at 0 s, from the right
            new GameEvent.SpawnDue("fly", 1f),  // at 0.5 s, from the left
        }, game.DrainEvents());
    }

    [Fact]
    public void Clearing_the_last_wave_wins_the_game()
    {
        var game = InWave(out _);
        game.Step(10f);
        game.CheckCleared(pestsAlive: 0);
        game.TryStartWave(eggOnCounter: false);
        game.Step(10f);
        game.DrainEvents();

        game.CheckCleared(pestsAlive: 0);

        Assert.Equal(Phase.Over, game.Phase);
        Assert.Equal(2, game.WaveIndex);
        Assert.Equal(new GameEvent[] { new GameEvent.Won() }, game.DrainEvents());
        Assert.False(game.CanStartWave(eggOnCounter: false));
    }

    // ------------------------------------------------------------------ losing

    [Fact]
    public void The_egg_on_the_counter_loses_the_game_and_nothing_more_comes()
    {
        var game = InWave(out _);

        game.Lose(LossReason.EggOnCounter);

        Assert.Equal(Phase.Over, game.Phase);
        Assert.Equal(0, game.SpawnsLeft);
        Assert.Equal(new GameEvent[] { new GameEvent.Lost(LossReason.EggOnCounter) }, game.DrainEvents());

        game.Step(10f);
        game.CheckCleared(pestsAlive: 0);
        Assert.Empty(game.Events);
        Assert.Equal(0, game.WaveIndex);
    }

    [Fact]
    public void A_game_is_lost_only_once()
    {
        var game = InWave(out _);

        game.Lose(LossReason.EggBroken);
        game.Lose(LossReason.EggOnCounter);

        Assert.Equal(new GameEvent[] { new GameEvent.Lost(LossReason.EggBroken) }, game.DrainEvents());
    }

    [Fact]
    public void A_won_game_cannot_be_lost()
    {
        var game = InWave(out _);
        game.Step(10f);
        game.CheckCleared(pestsAlive: 0);
        game.TryStartWave(eggOnCounter: false);
        game.Step(10f);
        game.CheckCleared(pestsAlive: 0);
        game.DrainEvents();

        game.Lose(LossReason.EggBroken);

        Assert.Empty(game.Events);
    }

    [Fact]
    public void The_egg_broken_between_waves_loses_too()
    {
        var game = NewGame(out _);

        game.Lose(LossReason.EggBroken);

        Assert.Equal(Phase.Over, game.Phase);
    }
}
