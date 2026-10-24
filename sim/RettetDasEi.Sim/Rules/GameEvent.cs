namespace RettetDasEi.Sim;

/// <summary>What the rules decided, for the view to show — appended by <see cref="GameState"/>, drained once per step.</summary>
public abstract record GameEvent
{
    /// <summary>A wave began; <paramref name="Number"/> counts from 1.</summary>
    public sealed record WaveStarted(int Number) : GameEvent;

    /// <summary>A pest of the running wave is due; <paramref name="Dir"/> +1 walks right (comes from the left), −1 comes from the right.</summary>
    public sealed record SpawnDue(string Enemy, float Dir) : GameEvent;

    /// <summary>Wave <paramref name="Number"/> (from 1) is cleared and paid <paramref name="Reward"/>; a build phase follows.</summary>
    public sealed record WaveCleared(int Number, int Reward) : GameEvent;

    /// <summary>The last wave is cleared.</summary>
    public sealed record Won : GameEvent;

    public sealed record Lost(LossReason Reason) : GameEvent;
}
