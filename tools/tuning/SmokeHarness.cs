using System.Linq;
using Godot;
using RettetDasEi.Sim;

namespace RettetDasEi;

// The tuning harness — never part of the game: tools/tuning/setup.sh copies game/ and sim/ to
// tuning-run/ and hooks this into that copy's Main. A fixed tower by name (-- --tower=…, see
// setup.sh), every wave started by itself, the game quits at won or lost and prints how far it
// came, the gold after each wave, the largest crowd, a probe's tilt and drift, and the fly bites.
public partial class Main
{
    float _harnessT;
    int _harnessCost;

    void HarnessTower()
    {
        string size = OS.GetCmdlineUserArgs().FirstOrDefault(a => a.StartsWith("--tower="))?.Substring(8) ?? "small";
        if (size is "pepperweak" or "pepperstrong")
        {
            // Four peppers high on a cheese: a weak base (a toast) and a strong one (a cheese).
            _egg.Position = new Vector2(790f, BoardTop - 32f);
            float baseTop;
            if (size == "pepperweak")
            {
                Place("toast", 990f, BoardTop - 42f);
                baseTop = BoardTop - 84f;
            }
            else
            {
                Place("cheese", 990f, BoardTop - 28f);
                baseTop = BoardTop - 56f;
            }
            _probe = PlaceGet("cheese", 990f, baseTop - 28f);
            _probeX = 990f;
            Place("pepper", 940f, baseTop - 56f - 36f);
            Place("pepper", 990f, baseTop - 56f - 36f);
            Place("pepper", 1040f, baseTop - 56f - 36f);
            Place("pepper", 990f, baseTop - 56f - 106f);
            GD.Print($"[harness] tower={size} cost={_harnessCost}");
            return;
        }
        if (size == "his")
        {
            // Sezgin's winning tower (screenshot 2026-10-24): the egg upright between four peppers.
            _probe = PlaceGet("cheese", 960f, BoardTop - 28f);
            _probeX = 960f;
            _egg.Position = new Vector2(960f, BoardTop - 56f - 46f);
            _egg.Rotation = Mathf.Pi / 2f;
            Place("pepper", 905f, BoardTop - 56f - 36f);
            Place("pepper", 1015f, BoardTop - 56f - 36f);
            Place("pepper", 905f, BoardTop - 56f - 106f);
            Place("pepper", 1015f, BoardTop - 56f - 106f);
            Place("cheese", 960f, BoardTop - 56f - 140f - 30f);
            Place("pickle", 960f, BoardTop - 56f - 140f - 56f - 24f);
            Place("pickle", 960f, BoardTop - 56f - 140f - 56f - 70f);
            Place("toast", 1120f, BoardTop - 43f);
            GD.Print($"[harness] tower={size} cost={_harnessCost}");
            return;
        }
        Place("cheese", 960f, BoardTop - 28f);
        _egg.Position = new Vector2(960f, BoardTop - 56f - 32f);
        Place("pickle", 815f, BoardTop - 22f);
        Place("pepper", 1060f, BoardTop - 35f);
        Place("toast", 1135f, BoardTop - 42f);
        Place("tomato", 1320f, CounterTop - 33f);
        if (size == "w3")
        {
            Place("cheese", 1500f, CounterTop - 28f);
            Place("mustard", 1500f, CounterTop - 56f - 62f);
        }
        if (size is "mid" or "big" or "bigless")
        {
            Place("cheese", 1500f, CounterTop - 28f);
            Place("mustard", 1500f, CounterTop - 56f - 62f);
            Place("cheese", 420f, CounterTop - 28f);
            Place("pickle", 420f, CounterTop - 56f - 22f);
            Place("tomato", 660f, CounterTop - 33f);
        }
        if (size is "big" or "bigless")
        {
            Place("pepper", 1620f, CounterTop - 35f);
            Place("toast", 1700f, CounterTop - 42f);
            Place("pickle", 250f, CounterTop - 22f);
            Place("toast", 120f, CounterTop - 42f);
            if (size == "big") Place("mustard", 560f, CounterTop - 62f);
        }
        GD.Print($"[harness] tower={size} cost={_harnessCost}");
    }

    UnitBody? _probe; float _probeX; float _maxTilt; float _maxDrift;
    UnitBody PlaceGet(string id, float x, float y)
    {
        var def = GameContent.Db.Unit(id);
        _harnessCost += def.Cost;
        var unit = CreateUnit(def, new Vector2(x, y), 0f);
        // As if laid by hand: only what the hand lets go sticks by coming to rest (D24).
        unit.GlueWindow = 5f;
        return unit;
    }

    void Place(string id, float x, float y) => PlaceGet(id, x, y);

    int _maxCrowd;
    void HarnessTick(double delta)
    {
        _harnessT += (float)delta;
        if (_probe != null && IsInstanceValid(_probe) && !_probe.IsBroken) { _maxTilt = Mathf.Max(_maxTilt, Mathf.Abs(Mathf.RadToDeg(Mathf.AngleDifference(0f, _probe.Rotation)))); _maxDrift = Mathf.Max(_maxDrift, Mathf.Abs(_probe.GlobalPosition.X - _probeX)); }
        foreach (var en in _enemies) if (en.IsAttacking && !en.IsFlyer) _maxCrowd = Mathf.Max(_maxCrowd, CrowdIndex(en) + 1);
        if (_game.Phase == Phase.Build && _harnessT > 2.5f)
        {
            _harnessT = 0f;
            TryStartWave();
        }
    }

    void HarnessEvent(GameEvent e)
    {
        if (e is GameEvent.WaveCleared c)
            GD.Print($"[harness] cleared {c.Number} gold={_game.Gold} crowd={_maxCrowd} tilt={_maxTilt:0} drift={_maxDrift:0}");
        if (e is GameEvent.Won)
            GD.Print($"[harness] egg hp left {_egg.Hp:0}/{_egg.MaxHp:0}");
        if (e is GameEvent.Won)
        {
            GD.Print($"[harness] RESULT won tilt={_maxTilt:0} drift={_maxDrift:0}");
            GetTree().Quit();
        }
        if (e is GameEvent.Lost l)
        {
            GD.Print($"[harness] RESULT lost in wave {_game.WaveIndex + 1} ({l.Reason}) tilt={_maxTilt:0} drift={_maxDrift:0}");
            GetTree().Quit();
        }
    }
}
