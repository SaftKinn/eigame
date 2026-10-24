using System.Collections.Generic;
using System.Linq;
using Godot;
using RettetDasEi.Sim;

namespace RettetDasEi;

public enum Phase { Build, Wave, Over }

/// <summary>
/// The whole prototype in one scene, built from code: the kitchen, the tower of food, the waves
/// of pests and the shop. Between waves food is bought and stacked by hand (the physics runs the
/// whole time); during a wave the food shoots by itself. The egg must not touch the counter.
/// </summary>
public partial class Main : Node2D
{
    public static Main I { get; private set; } = null!;

    public const uint WorldLayer = 1;
    public const uint UnitsLayer = 2;

    public const float ScreenWidth = 1920f;
    public const float CounterTop = 860f;
    public const float BoardTop = 830f;
    public const float BoardLeft = 740f;
    public const float BoardRight = 1180f;
    const float BoardRamp = 24f;
    const float RotateStep = Mathf.Pi / 12f;

    public Phase Phase { get; private set; } = Phase.Build;
    public int Gold { get; private set; }
    public int WaveIndex { get; private set; }
    public IReadOnlyList<Enemy> Enemies => _enemies;
    public UnitBody? HeldUnit => _held;
    public Node2D FxRoot { get; private set; } = null!;

    readonly List<Enemy> _enemies = new();
    readonly List<UnitBody> _units = new();
    readonly List<(float Time, string Enemy, float Dir)> _spawnQueue = new();

    UnitBody _egg = null!;
    StaticBody2D _counter = null!;
    Node2D _unitsRoot = null!;
    Node2D _enemiesRoot = null!;
    Node2D _projectilesRoot = null!;
    Camera2D _camera = null!;
    Hud _hud = null!;

    float _waveTime;
    float _shake;
    bool _fast;

    UnitBody? _held;
    bool _heldIsNew;
    Vector2 _grabOffset;
    float _heldRotation;
    Vector2? _pendingPick;

    public override void _Ready()
    {
        I = this;
        Engine.TimeScale = 1.0;
        GameContent.Load();
        Gold = GameContent.Db.Game.StartGold;

        AddChild(new KitchenBackdrop { ZIndex = -10 });
        _counter = AddStatic(new Vector2(ScreenWidth / 2f, CounterTop + 150f), new Vector2(ScreenWidth + 400f, 300f));
        AddStatic(new Vector2((BoardLeft + BoardRight) / 2f, (BoardTop + CounterTop) / 2f), new Vector2(BoardRight - BoardLeft, CounterTop - BoardTop));
        AddStatic(new Vector2(-20f, 0f), new Vector2(40f, 4000f));
        AddStatic(new Vector2(ScreenWidth + 20f, 0f), new Vector2(40f, 4000f));

        _unitsRoot = AddLayer("Units", 0);
        _enemiesRoot = AddLayer("Enemies", 1);
        _projectilesRoot = AddLayer("Projectiles", 2);
        AddChild(new RangeOverlay { ZIndex = 3 });
        FxRoot = AddLayer("Fx", 5);

        _camera = new Camera2D { Position = new Vector2(ScreenWidth / 2f, 540f) };
        AddChild(_camera);
        _camera.MakeCurrent();

        _hud = new Hud();
        AddChild(_hud);
        _hud.Build(GameContent.Db.Units.Where(u => u.InShop), BuyFromShop, TryStartWave, Restart);

        var eggDef = GameContent.Db.Unit(GameContent.Db.Game.EggUnit);
        _egg = CreateUnit(eggDef, new Vector2((BoardLeft + BoardRight) / 2f, BoardTop - eggDef.Height / 2f - 1f), 0f);

        _hud.ShowMessage("Zieh Essen aus dem Laden aufs Brett und bau einen Turm. Das Ei gehört nach oben!", 7f);
    }

    StaticBody2D AddStatic(Vector2 center, Vector2 size)
    {
        var body = new StaticBody2D
        {
            Position = center,
            CollisionLayer = WorldLayer,
            CollisionMask = 0,
            PhysicsMaterialOverride = new PhysicsMaterial { Friction = 1f, Bounce = 0f },
        };
        body.AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = size } });
        AddChild(body);
        return body;
    }

    Node2D AddLayer(string name, int z)
    {
        var node = new Node2D { Name = name, ZIndex = z };
        AddChild(node);
        return node;
    }

    // ------------------------------------------------------------ the loop

    public override void _Process(double delta)
    {
        _shake = Mathf.MoveToward(_shake, 0f, (float)delta * 40f);
        _camera.Offset = _shake > 0.01f
            ? new Vector2((float)GD.RandRange(-1.0, 1.0), (float)GD.RandRange(-1.0, 1.0)) * _shake
            : Vector2.Zero;
        _hud.Refresh(Gold, WaveIndex, GameContent.Db.Waves.Count, Phase, CanStartWave(), EggOnCounter(), _fast);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_pendingPick is Vector2 pick)
        {
            _pendingPick = null;
            if (Phase == Phase.Build && _held == null)
            {
                var unit = UnitAt(pick);
                if (unit != null)
                    StartHold(unit, false, pick);
            }
        }

        if (_held != null)
            UpdateHeld();

        if (Phase != Phase.Wave)
            return;

        _waveTime += (float)delta;
        while (_spawnQueue.Count > 0 && _spawnQueue[0].Time <= _waveTime)
        {
            var spawn = _spawnQueue[0];
            _spawnQueue.RemoveAt(0);
            SpawnEnemy(GameContent.Db.Enemy(spawn.Enemy), spawn.Dir);
        }

        if (EggOnCounter())
        {
            GameOver("Das Ei ist auf die Theke gefallen!");
            return;
        }
        if (_spawnQueue.Count == 0 && _enemies.Count == 0)
            WaveCleared();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true } mouse)
        {
            if (mouse.ButtonIndex == MouseButton.Left && Phase == Phase.Build && _held == null)
                _pendingPick = GetGlobalMousePosition();
            else if (_held != null && mouse.ButtonIndex == MouseButton.WheelUp)
                _heldRotation -= RotateStep;
            else if (_held != null && mouse.ButtonIndex == MouseButton.WheelDown)
                _heldRotation += RotateStep;
        }
        else if (@event is InputEventKey { Pressed: true, Echo: false } key)
        {
            switch (key.Keycode)
            {
                case Key.Space:
                    TryStartWave();
                    break;
                case Key.R:
                    Restart();
                    break;
                case Key.F:
                    _fast = !_fast;
                    Engine.TimeScale = _fast ? 2.0 : 1.0;
                    break;
                case Key.Q:
                    if (_held != null)
                        _heldRotation -= RotateStep;
                    break;
                case Key.E:
                    if (_held != null)
                        _heldRotation += RotateStep;
                    break;
            }
        }
    }

    // ------------------------------------------------------------- waves

    bool CanStartWave() =>
        Phase == Phase.Build && _held == null && !EggOnCounter() && WaveIndex < GameContent.Db.Waves.Count;

    bool EggOnCounter() => IsInstanceValid(_egg) && !_egg.IsBroken && !_egg.IsHeld && _egg.IsTouching(_counter);

    void TryStartWave()
    {
        if (!CanStartWave())
        {
            if (Phase == Phase.Build && EggOnCounter())
                _hud.ShowMessage("Das Ei muss aufs Brett oder auf den Turm!", 2.5f);
            return;
        }

        Phase = Phase.Wave;
        _waveTime = 0f;
        _spawnQueue.Clear();
        foreach (var group in GameContent.Db.Waves[WaveIndex].Groups)
        {
            for (int i = 0; i < group.Count; i++)
            {
                float dir = group.Side switch
                {
                    "left" => 1f,
                    "right" => -1f,
                    _ => i % 2 == 0 ? -1f : 1f,
                };
                _spawnQueue.Add((group.Start + i * group.Interval, group.Enemy, dir));
            }
        }
        _spawnQueue.Sort((a, b) => a.Time.CompareTo(b.Time));
        _hud.ShowMessage($"Welle {WaveIndex + 1}!", 2f);
    }

    void SpawnEnemy(EnemyDef def, float dir)
    {
        var enemy = new Enemy();
        enemy.Setup(def, dir);
        float x = dir < 0f ? ScreenWidth + 60f : -60f;
        float y = def.Movement == "fly" ? (float)GD.RandRange(200.0, 420.0) : GroundY(x) - def.Radius;
        enemy.Position = new Vector2(x, y);
        _enemiesRoot.AddChild(enemy);
        _enemies.Add(enemy);
    }

    void WaveCleared()
    {
        int reward = GameContent.Db.Waves[WaveIndex].Reward;
        Gold += reward;
        WaveIndex++;
        if (WaveIndex >= GameContent.Db.Waves.Count)
        {
            Phase = Phase.Over;
            _hud.ShowEnd("Gewonnen!", $"Das Ei hat alle {GameContent.Db.Waves.Count} Wellen überlebt.");
            return;
        }
        Phase = Phase.Build;
        _hud.ShowMessage(reward > 0 ? $"Welle {WaveIndex} geschafft! +{reward} Gold" : $"Welle {WaveIndex} geschafft!", 3.5f);
    }

    void GameOver(string reason)
    {
        if (Phase == Phase.Over)
            return;
        Phase = Phase.Over;
        _spawnQueue.Clear();
        Shake(16f);
        if (IsInstanceValid(_egg) && !_egg.IsBroken)
            _egg.Break();
        _hud.ShowEnd("Das Ei ist kaputt!", $"{reason}\nDu hast {WaveIndex} von {GameContent.Db.Waves.Count} Wellen geschafft.");
    }

    void Restart()
    {
        Engine.TimeScale = 1.0;
        GetTree().ReloadCurrentScene();
    }

    // ------------------------------------------------------- what others ask

    /// <summary>Height of the floor at x: the board in the middle, a short ramp, the counter elsewhere.</summary>
    public float GroundY(float x)
    {
        float outside = Mathf.Max(BoardLeft - x, x - BoardRight);
        if (outside <= 0f)
            return BoardTop;
        if (outside >= BoardRamp)
            return CounterTop;
        return Mathf.Lerp(BoardTop, CounterTop, outside / BoardRamp);
    }

    /// <summary>The nearest living pest within <paramref name="range"/> pixels to the left or right.</summary>
    public Enemy? FindTarget(Vector2 from, float range)
    {
        Enemy? best = null;
        float bestDx = float.MaxValue;
        foreach (var enemy in _enemies)
        {
            if (enemy.IsDead)
                continue;
            var p = enemy.GlobalPosition;
            if (p.X < -10f || p.X > ScreenWidth + 10f)
                continue;
            float dx = Mathf.Abs(p.X - from.X);
            if (dx <= range && dx < bestDx)
            {
                best = enemy;
                bestDx = dx;
            }
        }
        return best;
    }

    public UnitBody? HighestUnit()
    {
        UnitBody? best = null;
        foreach (var unit in _units)
        {
            if (unit.IsHeld || unit.IsBroken)
                continue;
            if (best == null || unit.GlobalPosition.Y < best.GlobalPosition.Y)
                best = unit;
        }
        return best;
    }

    /// <summary>The piece of food a pest-sized circle at <paramref name="at"/> would touch, if any.</summary>
    public UnitBody? UnitTouching(Shape2D shape, Vector2 at)
    {
        var query = new PhysicsShapeQueryParameters2D
        {
            Shape = shape,
            Transform = new Transform2D(0f, at),
            CollisionMask = UnitsLayer,
            CollideWithBodies = true,
            CollideWithAreas = false,
        };
        foreach (var hit in GetWorld2D().DirectSpaceState.IntersectShape(query, 4))
            if (hit["collider"].AsGodotObject() is UnitBody unit && !unit.IsBroken && !unit.IsHeld)
                return unit;
        return null;
    }

    /// <summary>Walkers queue up behind each other instead of melting into one.</summary>
    public bool IsBlockedByEnemy(Enemy me, float nextX)
    {
        foreach (var other in _enemies)
        {
            if (other == me || other.IsDead || other.IsFlyer || other.Dir != me.Dir)
                continue;
            float ahead = (other.Position.X - nextX) * me.Dir;
            if (ahead > 0f && ahead < (me.Def.Radius + other.Def.Radius) * 0.6f)
                return true;
        }
        return false;
    }

    public void SpawnProjectile(AttackDef attack, Vector2 from, Enemy target)
    {
        var projectile = new Projectile();
        projectile.Setup(attack, from, target);
        _projectilesRoot.AddChild(projectile);
    }

    public void Shake(float amount) => _shake = Mathf.Max(_shake, amount);

    public void OnEnemyKilled(Enemy enemy)
    {
        _enemies.Remove(enemy);
        Gold += enemy.Def.Gold;
        Fx.Number(FxRoot, enemy.GlobalPosition + new Vector2(0f, -enemy.Def.Radius - 44f), $"+{enemy.Def.Gold}", Fx.GoldColor, 26);
    }

    public void OnUnitBroken(UnitBody unit)
    {
        _units.Remove(unit);
        if (unit == _held)
        {
            _held = null;
            _hud.ShowSellHint(null);
        }
        WakeAll();
        if (unit.IsEgg && Phase != Phase.Over)
            GameOver("Das Ei ist zerbrochen!");
    }

    // ------------------------------------------------------ shop and hand

    UnitBody CreateUnit(UnitDef def, Vector2 position, float rotation)
    {
        var unit = new UnitBody();
        unit.Setup(def, GameContent.Db.Game);
        unit.Position = position;
        unit.Rotation = rotation;
        _unitsRoot.AddChild(unit);
        _units.Add(unit);
        return unit;
    }

    void BuyFromShop(UnitDef def)
    {
        if (Phase != Phase.Build)
        {
            _hud.ShowMessage("Kaufen geht nur zwischen den Wellen.", 2f);
            return;
        }
        if (_held != null)
            return;
        if (Gold < def.Cost)
        {
            _hud.ShowMessage("Nicht genug Gold.", 2f);
            return;
        }
        Gold -= def.Cost;
        var mouse = GetGlobalMousePosition();
        var unit = CreateUnit(def, mouse, 0f);
        StartHold(unit, true, mouse);
    }

    UnitBody? UnitAt(Vector2 point)
    {
        var query = new PhysicsPointQueryParameters2D
        {
            Position = point,
            CollisionMask = UnitsLayer,
            CollideWithBodies = true,
            CollideWithAreas = false,
        };
        foreach (var hit in GetWorld2D().DirectSpaceState.IntersectPoint(query, 8))
            if (hit["collider"].AsGodotObject() is UnitBody unit && !unit.IsBroken)
                return unit;
        return null;
    }

    void StartHold(UnitBody unit, bool isNew, Vector2 mouse)
    {
        _held = unit;
        _heldIsNew = isNew;
        _grabOffset = isNew ? Vector2.Zero : unit.GlobalPosition - mouse;
        _heldRotation = unit.GlobalRotation;
        unit.BeginHold();
        WakeAll();
    }

    void UpdateHeld()
    {
        var unit = _held!;
        bool overShop = _hud.IsOverShop(GetViewport().GetMousePosition());
        var target = GetGlobalMousePosition() + _grabOffset;
        if (!overShop)
            target.Y = Mathf.Min(target.Y, CounterTop - 8f);
        target.X = Mathf.Clamp(target.X, 30f, ScreenWidth - 30f);
        unit.GlobalPosition = target;
        unit.GlobalRotation = _heldRotation;

        bool sell = overShop && unit.Def.InShop;
        bool blocked = !overShop && Overlaps(unit);
        unit.SetGhost(sell ? GhostMode.Sell : blocked ? GhostMode.Blocked : GhostMode.Ok);
        _hud.ShowSellHint(sell ? RefundFor(unit) : null);

        if (!Input.IsMouseButtonPressed(MouseButton.Left))
            Drop(sell);
    }

    int RefundFor(UnitBody unit) => _heldIsNew
        ? unit.Def.Cost
        : Mathf.RoundToInt(unit.Def.Cost * GameContent.Db.Game.SellRefund * unit.Hp / unit.MaxHp);

    void Drop(bool sell)
    {
        var unit = _held!;
        _held = null;
        _hud.ShowSellHint(null);

        if (sell)
        {
            int refund = RefundFor(unit);
            Gold += refund;
            _units.Remove(unit);
            unit.QueueFree();
            if (!_heldIsNew)
                Fx.Number(FxRoot, GetGlobalMousePosition() + new Vector2(0f, -260f), $"+{refund}", Fx.GoldColor, 30);
            WakeAll();
            return;
        }

        // Back above the counter (the egg cannot be sold), then up until it is free: a piece
        // let go inside the tower pops out on top of it and drops the last few pixels.
        var position = unit.GlobalPosition;
        position.Y = Mathf.Min(position.Y, CounterTop - 8f);
        unit.GlobalPosition = position;
        for (int i = 0; i < 250 && Overlaps(unit); i++)
            unit.GlobalPosition += new Vector2(0f, -4f);
        unit.EndHold();
        WakeAll();
    }

    bool Overlaps(UnitBody unit)
    {
        var query = new PhysicsShapeQueryParameters2D
        {
            Shape = unit.ShapeResource,
            Transform = unit.ShapeNode.GlobalTransform,
            CollisionMask = WorldLayer | UnitsLayer,
            CollideWithBodies = true,
            CollideWithAreas = false,
            Exclude = new Godot.Collections.Array<Rid> { unit.GetRid() },
        };
        return GetWorld2D().DirectSpaceState.IntersectShape(query, 1).Count > 0;
    }

    /// <summary>Sleeping bodies do not notice when the piece under them is taken away.</summary>
    void WakeAll()
    {
        foreach (var unit in _units)
            if (!unit.IsHeld && !unit.IsBroken)
                unit.Sleeping = false;
    }
}
