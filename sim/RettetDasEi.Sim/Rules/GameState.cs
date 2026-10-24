namespace RettetDasEi.Sim;

public enum Phase { Build, Wave, Over }

public enum BuyResult { Bought, NotBetweenWaves, TooDear }

public enum LossReason { EggOnCounter, EggBroken }

/// <summary>
/// One game from the first build phase to won or lost: gold, the wave, the phase and the running
/// wave's schedule. Godot reports what happened — a pest killed, the egg on the counter or broken,
/// how many pests are still alive — and steps it with the physics' delta; what follows is decided
/// here and told as events the view drains (ADR 002).
/// </summary>
public sealed class GameState
{
    readonly ContentDb _db;
    readonly List<Spawn> _queue = new();
    readonly List<GameEvent> _events = new();

    public GameState(ContentDb db)
    {
        _db = db;
        Gold = db.Game.StartGold;
    }

    public Phase Phase { get; private set; } = Phase.Build;
    public int Gold { get; private set; }
    /// <summary>Waves cleared so far — during a wave, the index of the one running.</summary>
    public int WaveIndex { get; private set; }
    public int WaveCount => _db.Waves.Count;
    /// <summary>Seconds since the running wave began.</summary>
    public float WaveTime { get; private set; }
    /// <summary>Pests of the running wave that have not come yet.</summary>
    public int SpawnsLeft => _queue.Count;

    // ---------------------------------------------------------------- the shop

    /// <summary>Pays for a piece of food — only between waves, only with the gold for it.</summary>
    public BuyResult TryBuy(UnitDef def)
    {
        if (Phase != Phase.Build)
            return BuyResult.NotBetweenWaves;
        if (Gold < def.Cost)
            return BuyResult.TooDear;
        Gold -= def.Cost;
        return BuyResult.Bought;
    }

    /// <summary>What selling a piece would give back — the hint while dragging and the payment ask this one rule.</summary>
    public int RefundFor(UnitDef def, bool justBought, float hp, float maxHp) =>
        Shop.Refund(def, _db.Game, justBought, hp, maxHp);

    public int Sell(UnitDef def, bool justBought, float hp, float maxHp)
    {
        int refund = RefundFor(def, justBought, hp, maxHp);
        Gold += refund;
        return refund;
    }

    public void PestKilled(EnemyDef def) => Gold += def.Gold;

    // --------------------------------------------------------------- the waves

    /// <summary>A wave starts only between waves, with the egg off the counter, while waves are left.</summary>
    public bool CanStartWave(bool eggOnCounter) =>
        Phase == Phase.Build && !eggOnCounter && WaveIndex < WaveCount;

    public bool TryStartWave(bool eggOnCounter)
    {
        if (!CanStartWave(eggOnCounter))
            return false;
        Phase = Phase.Wave;
        WaveTime = 0f;
        _queue.Clear();
        _queue.AddRange(WaveSchedule.Build(_db.Waves[WaveIndex]));
        _events.Add(new GameEvent.WaveStarted(WaveIndex + 1));
        return true;
    }

    /// <summary>One physics step of a wave: every pest whose time has come is due (event <see cref="GameEvent.SpawnDue"/>).</summary>
    public void Step(float dt)
    {
        if (Phase != Phase.Wave)
            return;
        WaveTime += dt;
        while (_queue.Count > 0 && _queue[0].Time <= WaveTime)
        {
            var spawn = _queue[0];
            _queue.RemoveAt(0);
            _events.Add(new GameEvent.SpawnDue(spawn.Enemy, spawn.Dir));
        }
    }

    /// <summary>
    /// A wave is cleared when nothing is left to come and no pest is alive: its reward is paid, and
    /// the next build phase begins — or, after the last wave, the game is won.
    /// </summary>
    public void CheckCleared(int pestsAlive)
    {
        if (Phase != Phase.Wave || _queue.Count > 0 || pestsAlive > 0)
            return;
        int reward = _db.Waves[WaveIndex].Reward;
        Gold += reward;
        WaveIndex++;
        if (WaveIndex >= WaveCount)
        {
            Phase = Phase.Over;
            _events.Add(new GameEvent.Won());
            return;
        }
        Phase = Phase.Build;
        _events.Add(new GameEvent.WaveCleared(WaveIndex, reward));
    }

    /// <summary>The egg broke or touched the counter: the game is over, once.</summary>
    public void Lose(LossReason reason)
    {
        if (Phase == Phase.Over)
            return;
        Phase = Phase.Over;
        _queue.Clear();
        _events.Add(new GameEvent.Lost(reason));
    }

    // -------------------------------------------------------------- the events

    public IReadOnlyList<GameEvent> Events => _events;

    /// <summary>What happened since the last call, in order; the list is empty afterwards.</summary>
    public GameEvent[] DrainEvents()
    {
        var events = _events.ToArray();
        _events.Clear();
        return events;
    }
}
