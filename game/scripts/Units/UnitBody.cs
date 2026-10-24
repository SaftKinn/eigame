using Godot;
using RettetDasEi.Sim;

namespace RettetDasEi;

/// <summary>How a piece of food in the hand is tinted.</summary>
public enum GhostMode { Ok, Blocked, Sell }

/// <summary>
/// One piece of food in the tower: a physics body with health that, if it has an attack, shoots
/// at the nearest pest in range. There are no frames of animation — a shot is the picture
/// squashing back, snapping forward and settling (ADR 001), and the recoil is a real impulse.
/// </summary>
public partial class UnitBody : RigidBody2D
{
    public UnitDef Def { get; private set; } = null!;
    public float Hp { get; private set; }
    public float MaxHp { get; private set; }
    public bool IsEgg { get; private set; }
    public bool IsHeld { get; private set; }
    public bool IsBroken { get; private set; }
    public CollisionShape2D ShapeNode { get; private set; } = null!;
    public Shape2D ShapeResource { get; private set; } = null!;

    public float BoundRadius => Mathf.Max(Def.Width, Def.Height) * 0.5f;

    /// <summary>Higher up shoots further: the core rule of the tower.</summary>
    public float EffectiveRange => Def.Attack == null
        ? 0f
        : Def.Attack.Range + Mathf.Max(0f, Main.BoardTop - GlobalPosition.Y) * GameContent.Db.Game.RangePerHeight;

    UnitVisual _visual = null!;
    HpBar _hpBar = null!;
    float _cooldown;
    Tween? _shot;

    public void Setup(UnitDef def, GameDef game)
    {
        Def = def;
        Hp = MaxHp = def.Hp;
        IsEgg = def.Id == game.EggUnit;
        Name = def.Id;

        Mass = def.Mass;
        PhysicsMaterialOverride = new PhysicsMaterial { Friction = def.Friction, Bounce = 0f };
        LinearDamp = game.UnitLinearDamp;
        AngularDamp = game.UnitAngularDamp;
        ContactMonitor = true;
        MaxContactsReported = 8;
        FreezeMode = FreezeModeEnum.Kinematic;
        CollisionLayer = Main.UnitsLayer;
        CollisionMask = Main.WorldLayer | Main.UnitsLayer;

        ShapeResource = BuildShape(def, out float shapeRotation);
        ShapeNode = new CollisionShape2D { Shape = ShapeResource, Rotation = shapeRotation };
        AddChild(ShapeNode);

        _visual = new UnitVisual();
        _visual.Setup(def);
        AddChild(_visual);

        _hpBar = new HpBar { TopLevel = true, Visible = false, ZIndex = 20 };
        AddChild(_hpBar);

        _cooldown = def.Attack != null ? def.Attack.Cooldown * 0.5f : 0f;
    }

    static Shape2D BuildShape(UnitDef def, out float rotation)
    {
        rotation = 0f;
        switch (def.Shape)
        {
            case "circle":
                return new CircleShape2D { Radius = def.Width / 2f };
            case "capsule":
            case "egg":
                // Godot's capsule stands upright; a wide one is laid on its side.
                if (def.Width >= def.Height)
                {
                    rotation = Mathf.Pi / 2f;
                    return new CapsuleShape2D { Radius = def.Height / 2f, Height = def.Width };
                }
                return new CapsuleShape2D { Radius = def.Width / 2f, Height = def.Height };
            default:
                return new RectangleShape2D { Size = new Vector2(def.Width, def.Height) };
        }
    }

    public override void _Process(double delta)
    {
        if (IsBroken)
            return;
        _hpBar.GlobalPosition = GlobalPosition + new Vector2(0f, -BoundRadius - 16f);
        _hpBar.Visible = Hp < MaxHp && !IsHeld;
        _hpBar.SetRatio(Hp / MaxHp);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsBroken || IsHeld || Def.Attack == null || Main.I.Phase != Phase.Wave)
            return;
        _cooldown -= (float)delta;
        if (_cooldown > 0f)
            return;
        var target = Main.I.FindTarget(GlobalPosition, EffectiveRange);
        if (target == null)
        {
            _cooldown = 0.15f;
            return;
        }
        _cooldown = Def.Attack.Cooldown;
        Shoot(target);
    }

    /// <summary>Ausholen, loslassen, nachfedern — the whole "animation" of a shot.</summary>
    void Shoot(Enemy target)
    {
        float face = target.GlobalPosition.X >= GlobalPosition.X ? 1f : -1f;
        _visual.SetFace(face);
        _shot?.Kill();
        _visual.Scale = Vector2.One;
        _visual.Rotation = 0f;

        var tween = CreateTween();
        _shot = tween;
        tween.TweenProperty(_visual, "scale", new Vector2(1.12f, 0.88f), 0.12)
            .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        tween.Parallel().TweenProperty(_visual, "rotation", -face * 0.12f, 0.12);
        tween.TweenCallback(Callable.From(() => Fire(target)));
        tween.TweenProperty(_visual, "scale", new Vector2(0.9f, 1.1f), 0.05);
        tween.Parallel().TweenProperty(_visual, "rotation", face * 0.1f, 0.05);
        tween.TweenProperty(_visual, "scale", Vector2.One, 0.25)
            .SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
        tween.Parallel().TweenProperty(_visual, "rotation", 0f, 0.25);
    }

    void Fire(Enemy target)
    {
        if (IsBroken || Def.Attack == null || Main.I.Phase != Phase.Wave)
            return;
        if (!IsInstanceValid(target) || target.IsDead)
        {
            var other = Main.I.FindTarget(GlobalPosition, EffectiveRange);
            if (other == null)
                return;
            target = other;
        }
        var direction = (target.GlobalPosition - GlobalPosition).Normalized();
        var from = GlobalPosition + direction * BoundRadius * 0.6f;
        Main.I.SpawnProjectile(Def.Attack, from, target);
        if (Def.Attack.Recoil > 0f)
            ApplyCentralImpulse(-direction * Def.Attack.Recoil);
    }

    /// <summary>A pest's blow: a push at the point it struck, then the damage.</summary>
    public void TakeHit(float damage, Vector2 impulse, Vector2 atGlobal)
    {
        if (IsBroken)
            return;
        Sleeping = false;
        ApplyImpulse(impulse, atGlobal - GlobalPosition);
        TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        if (IsBroken)
            return;
        Hp -= damage;
        _visual.SetHpRatio(Hp / MaxHp);
        _visual.Flash();
        Fx.Number(Main.I.FxRoot, GlobalPosition + new Vector2(0f, -BoundRadius - 34f), Mathf.RoundToInt(damage).ToString(), Fx.HurtColor, 24);
        if (Hp <= 0f)
            Break();
    }

    public void Break()
    {
        if (IsBroken)
            return;
        IsBroken = true;
        var fx = Main.I.FxRoot;
        if (IsEgg)
        {
            Fx.Crumbs(fx, GlobalPosition, new Color(0.97f, 0.77f, 0.19f), 34, 380f);
            Fx.Crumbs(fx, GlobalPosition, Colors.White, 16, 300f);
        }
        else
        {
            Fx.Crumbs(fx, GlobalPosition, Color.FromHtml(Def.Fill), 18, 300f);
        }
        Main.I.Shake(IsEgg ? 14f : 5f);
        Main.I.OnUnitBroken(this);
        QueueFree();
    }

    public bool IsTouching(Node2D body) => GetCollidingBodies().Contains(body);

    // ------------------------------------------------------- drag and drop

    public void BeginHold()
    {
        IsHeld = true;
        _shot?.Kill();
        _visual.Scale = Vector2.One;
        _visual.Rotation = 0f;
        Freeze = true;
        CollisionLayer = 0;
        CollisionMask = 0;
        ZIndex = 10;
    }

    public void EndHold()
    {
        IsHeld = false;
        CollisionLayer = Main.UnitsLayer;
        CollisionMask = Main.WorldLayer | Main.UnitsLayer;
        ZIndex = 0;
        Modulate = Colors.White;
        Freeze = false;
        LinearVelocity = Vector2.Zero;
        AngularVelocity = 0f;
        Sleeping = false;
    }

    public void SetGhost(GhostMode mode)
    {
        Modulate = mode switch
        {
            GhostMode.Blocked => new Color(1f, 0.45f, 0.45f, 0.8f),
            GhostMode.Sell => new Color(1f, 0.85f, 0.35f, 0.55f),
            _ => new Color(1f, 1f, 1f, 0.85f),
        };
    }
}
