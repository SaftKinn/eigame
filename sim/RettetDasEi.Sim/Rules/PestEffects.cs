namespace RettetDasEi.Sim;

/// <summary>
/// What the food's jobs have done to one pest (ADR 003): the pepper's itch and its sneeze, the
/// mustard puddles it stands in. Godot reports the hits and the puddles it touches and steps it
/// with the physics' delta; this says how fast the pest may move and whether it may attack.
/// </summary>
public sealed class PestEffects
{
    int _itchHits = 1;

    /// <summary>Pepper hits since the last sneeze.</summary>
    public int Itch { get; private set; }
    /// <summary>0..1, how close the next sneeze is — the view lets the pest tremble by it.</summary>
    public float ItchShare => Itch / (float)_itchHits;
    /// <summary>Seconds of the sneeze's pause left.</summary>
    public float PauseLeft { get; private set; }
    /// <summary>While it sneezes a pest stands still and does not attack.</summary>
    public bool Sneezing => PauseLeft > 0f;
    /// <summary>0..1, the share of its speed it keeps this step (1 outside every puddle).</summary>
    public float SpeedFactor { get; private set; } = 1f;

    /// <summary>
    /// One pepper hit. True when it sneezes now: the itch starts again and the pause begins. No
    /// itch builds while it sneezes, so a fast pepper cannot keep it sneezing for good.
    /// </summary>
    public bool AddItch(ItchDef itch)
    {
        if (Sneezing)
            return false;
        _itchHits = itch.Hits;
        Itch++;
        if (Itch < itch.Hits)
            return false;
        Itch = 0;
        PauseLeft = itch.Pause;
        return true;
    }

    public void Step(float dt) => PauseLeft = MathF.Max(0f, PauseLeft - dt);

    /// <summary>Before the puddles of a step are reported: none.</summary>
    public void ClearPuddles() => SpeedFactor = 1f;

    /// <summary>A puddle it stands in. Puddles do not stack — the strongest slow counts.</summary>
    public void InPuddle(PuddleDef puddle) => SpeedFactor = MathF.Min(SpeedFactor, puddle.Slow);
}
