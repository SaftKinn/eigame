using Godot;
using RettetDasEi.Sim;

namespace RettetDasEi;

/// <summary>
/// A kitchen pest. Walkers move along the counter (and up onto the board) towards the tower,
/// flyers head for its highest piece. When one touches a piece of food it attacks: leans back,
/// lunges, and at the lunge the food gets a real physics push — the tower's wobble is the
/// attack animation. It is not a physics body itself; it only asks the physics what it touches.
/// </summary>
public partial class Enemy : Node2D
{
    const float Windup = 0.28f;
    const float Strike = 0.08f;
    const float Recover = 0.34f;
    const float HopCycle = 1.1f;
    const float KnockSeconds = 0.32f;
    /// <summary>How far a flyer turns from the straight way to find a way round food, nearest first.</summary>
    static readonly float[] SteerAngles = { 30f, 60f, 90f, 120f, 150f };
    static int _attacksBegun;

    public EnemyDef Def { get; private set; } = null!;
    /// <summary>The itch, the sneeze and the slow — the rules core's (ADR 002, 003).</summary>
    public PestEffects Effects { get; } = new();
    /// <summary>+1 walks right (came from the left), -1 walks left.</summary>
    public float Dir { get; private set; }
    public float Hp { get; private set; }
    public bool IsDead { get; private set; }
    public bool IsFlyer => Def.Movement == "fly";
    public bool IsAttacking => _attacking;
    /// <summary>When it began its attack, counted over all pests — who lies under whom in a heap.</summary>
    public int AttackOrder { get; private set; }
    /// <summary>How it moves right now; shooters aim ahead of it.</summary>
    public Vector2 Velocity { get; private set; }

    EnemyVisual _visual = null!;
    CircleShape2D _probe = null!;
    bool _attacking;
    bool _struck;
    float _attackT;
    UnitBody? _target;
    float _t;
    float _hopT;
    float _blockedT;
    float _steer;
    float _knockT;
    float _knockTime;
    float _knockDistance;
    Vector2 _knockFrom;
    Vector2 _knockTo;

    public void Setup(EnemyDef def, float dir)
    {
        Def = def;
        Hp = def.Hp;
        _probe = new CircleShape2D { Radius = def.Radius + 3f };
        _visual = new EnemyVisual();
        _visual.Setup(def);
        AddChild(_visual);
        SetDir(dir);
        _t = (float)GD.RandRange(0.0, 3.0);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsDead)
            return;
        float dt = (float)delta;
        _t += dt;
        if (Main.I.Phase == Phase.Over)
        {
            Velocity = Vector2.Zero;
            IdleMotion();
            return;
        }

        // The food's jobs (ADR 003): the core keeps the itch, the sneeze and the slow; the
        // puddles it stands in are reported every step.
        Effects.Step(dt);
        Effects.ClearPuddles();
        if (!IsFlyer)
            Main.I.ApplyPuddles(this);

        if (_knockT < _knockTime)
            KnockStep(dt);
        else if (Effects.Sneezing)
        {
            Velocity = Vector2.Zero;
            IdleMotion();
        }
        else if (_attacking)
            AttackStep(dt);
        else if (IsFlyer)
            FlyStep(dt);
        else
            WalkStep(dt);
        Tremble();
    }

    /// <summary>The itch is seen: the closer the next sneeze, the more it trembles.</summary>
    void Tremble()
    {
        float share = Effects.ItchShare;
        if (share > 0f)
            _visual.Position += new Vector2((float)GD.RandRange(-1.0, 1.0), (float)GD.RandRange(-1.0, 1.0)) * share * 5f;
    }

    // ---------------------------------------------------------------- moving

    void WalkStep(float dt)
    {
        float speed = Def.Speed * Effects.SpeedFactor;
        float step = speed * dt;
        float lift = 0f;
        if (Def.Movement == "hop")
        {
            // Squat on the ground, then a long leap forward.
            _hopT += dt;
            float p = (_hopT % HopCycle) / HopCycle;
            if (p < 0.3f)
            {
                step = 0f;
                float s = Mathf.Sin(p / 0.3f * Mathf.Pi);
                _visual.Scale = new Vector2(1f + 0.18f * s, 1f - 0.18f * s);
            }
            else
            {
                float q = (p - 0.3f) / 0.7f;
                step = speed * dt / 0.7f;
                lift = Mathf.Sin(q * Mathf.Pi) * Def.Radius * 0.9f;
                _visual.Scale = new Vector2(0.94f, 1.06f);
            }
        }

        float nextX = Position.X + Dir * step;
        var probeAt = new Vector2(nextX + Dir * 2f, Main.I.GroundY(nextX) - Def.Radius);
        var food = Main.I.UnitTouching(_probe, probeAt);
        if (food != null)
        {
            BeginAttack(food);
            return;
        }
        if (Main.I.IsBlockedByEnemy(this, nextX))
        {
            Velocity = Vector2.Zero;
            IdleMotion();
            return;
        }

        Position = new Vector2(nextX, Main.I.GroundY(nextX) - Def.Radius);
        Velocity = new Vector2(Dir * speed, 0f);
        if ((Dir > 0f && Position.X > Main.ScreenWidth + 90f) || (Dir < 0f && Position.X < -90f))
            SetDir(-Dir);

        switch (Def.Movement)
        {
            case "waddle":
                _visual.Rotation = Mathf.Sin(_t * 12f) * 0.16f;
                _visual.Position = new Vector2(0f, -Mathf.Abs(Mathf.Sin(_t * 12f)) * 4f);
                break;
            case "roll":
                _visual.Spin += Dir * step / Def.Radius;
                _visual.Position = Vector2.Zero;
                _visual.QueueRedraw();
                break;
            case "hop":
                _visual.Position = new Vector2(0f, -lift);
                _visual.Spin = _t;
                _visual.QueueRedraw();
                break;
        }
    }

    /// <summary>
    /// Flyers go for the egg and steer round the food in their way — upward first, the side once
    /// chosen kept. With no way through for <c>flyPatience</c> seconds (the core's
    /// <see cref="Flight.GivesUp"/>) they bite what blocks them (his word, 2026-10-24).
    /// </summary>
    void FlyStep(float dt)
    {
        var egg = Main.I.Egg;
        var target = egg ?? Main.I.HighestUnit();
        Vector2 goal = target?.GlobalPosition ?? new Vector2(Main.ScreenWidth / 2f, 300f);
        Vector2 toGoal = goal - Position;
        if (toGoal.LengthSquared() <= 1f)
        {
            Velocity = Vector2.Zero;
            Hover();
            return;
        }
        Vector2 straight = toGoal.Normalized();
        float step = Def.Speed * dt;

        var touched = Main.I.UnitTouching(_probe, Position + straight * step, egg);
        if (touched == null || touched == egg)
        {
            if (touched != null)
            {
                BeginAttack(touched);
                return;
            }
            _blockedT = 0f;
            _steer = 0f;
            FlyMove(straight, dt);
            return;
        }

        // Food in the way: look for a way round it while patience lasts.
        _blockedT += dt;
        if (Flight.GivesUp(GameContent.Db.Game, _blockedT))
        {
            BeginAttack(touched);
            return;
        }
        if (_steer == 0f)
            _steer = straight.X >= 0f ? -1f : 1f; // this turn points up first
        foreach (float side in new[] { _steer, -_steer })
        {
            foreach (float degrees in SteerAngles)
            {
                var direction = straight.Rotated(side * Mathf.DegToRad(degrees));
                var at = Position + direction * step;
                if (at.Y > Main.CounterTop - Def.Radius)
                    continue;
                var hit = Main.I.UnitTouching(_probe, at, egg);
                if (hit == egg && egg != null)
                {
                    BeginAttack(egg);
                    return;
                }
                if (hit != null)
                    continue;
                _steer = side;
                FlyMove(direction, dt);
                return;
            }
        }
        Velocity = Vector2.Zero;
        Hover();
    }

    void FlyMove(Vector2 direction, float dt)
    {
        Velocity = direction * Def.Speed;
        Position += Velocity * dt;
        if (Mathf.Abs(Velocity.X) > 1f)
            SetDir(Velocity.X > 0f ? 1f : -1f);
        Hover();
    }

    void IdleMotion()
    {
        if (IsFlyer)
        {
            Hover();
            return;
        }
        _visual.Position = new Vector2(0f, Mathf.Sin(_t * 4f) * 2f);
        _visual.Rotation = 0f;
    }

    void Hover()
    {
        _visual.Position = new Vector2(0f, Mathf.Sin(_t * 6f) * 6f);
        _visual.Flap = 0.25f + 0.75f * Mathf.Abs(Mathf.Sin(_t * 32f));
        _visual.QueueRedraw();
    }

    // ------------------------------------------------------------- attacking

    void BeginAttack(UnitBody food)
    {
        _attacking = true;
        AttackOrder = ++_attacksBegun;
        _blockedT = 0f;
        _steer = 0f;
        _attackT = 0f;
        _struck = false;
        _target = food;
        Velocity = Vector2.Zero;
        _visual.Rotation = 0f;
        SetDir(food.GlobalPosition.X >= Position.X ? 1f : -1f);
    }

    void StopAttack()
    {
        _attacking = false;
        _target = null;
        _visual.Scale = Vector2.One;
        _visual.Position = Vector2.Zero;
    }

    void AttackStep(float dt)
    {
        if (_target == null || !IsInstanceValid(_target) || _target.IsBroken)
        {
            StopAttack();
            return;
        }

        _attackT += dt;
        float cycle = Mathf.Max(Def.AttackCooldown, Windup + Strike + Recover);
        float lunge;
        if (_attackT < Windup)
        {
            float p = _attackT / Windup;
            lunge = -10f * EaseOut(p);
            _visual.Scale = new Vector2(1f - 0.08f * p, 1f + 0.08f * p);
        }
        else if (_attackT < Windup + Strike)
        {
            float p = (_attackT - Windup) / Strike;
            lunge = Mathf.Lerp(-10f, 16f, p);
            _visual.Scale = new Vector2(1.12f, 0.9f);
            if (!_struck && p >= 0.5f)
            {
                _struck = true;
                DoStrike();
                if (!_attacking)
                    return; // a shield threw it back
            }
        }
        else if (_attackT < Windup + Strike + Recover)
        {
            float p = (_attackT - Windup - Strike) / Recover;
            lunge = Mathf.Lerp(16f, 0f, EaseOut(p));
            _visual.Scale = new Vector2(Mathf.Lerp(1.12f, 1f, p), Mathf.Lerp(0.9f, 1f, p));
        }
        else
        {
            lunge = 0f;
            _visual.Scale = Vector2.One;
        }

        float size = Mathf.Clamp(Def.Radius / 24f, 0.7f, 2f);
        _visual.Position = AttackDirection() * lunge * size;
        if (!IsFlyer)
            _visual.Position += new Vector2(0f, -Main.I.CrowdIndex(this) * Def.Radius * 0.7f); // the heap
        if (IsFlyer)
        {
            _visual.Flap = 0.25f + 0.75f * Mathf.Abs(Mathf.Sin(_t * 32f));
            _visual.QueueRedraw();
        }

        if (_attackT >= cycle)
        {
            _attackT = 0f;
            _struck = false;
            var still = Main.I.UnitTouching(_probe, Position + AttackDirection() * 4f, IsFlyer ? Main.I.Egg : null);
            if (still == null)
                StopAttack();
            else
                _target = still;
        }
    }

    void DoStrike()
    {
        var direction = AttackDirection();
        var food = Main.I.UnitTouching(_probe, Position + direction * 6f);
        if (food == null)
            return;
        // Walkers shove forward and a little upward — that is what tips a tower over.
        var push = IsFlyer ? direction : new Vector2(Dir, -0.35f).Normalized();
        var contact = Position + direction * Def.Radius;
        var shield = food.Def.Shield;
        float strength = Shield.PushOn(shield, Def.Push);
        food.TakeHit(Def.Damage, push * strength, contact);
        Main.I.Shake(Mathf.Clamp(strength / 150f, 1f, 12f));
        Fx.Crumbs(Main.I.FxRoot, contact, Colors.White, 6, 140f);
        if (shield != null)
        {
            // The toast springs back and throws the pest off (ADR 003).
            food.SpringBack();
            KnockBack(shield.ThrowBack);
        }
    }

    // ------------------------------------------------- thrown back, sneezing

    /// <summary>Thrown back <paramref name="distance"/> pixels in a short hop; it has to come on again.</summary>
    void KnockBack(float distance)
    {
        if (distance <= 0f)
            return;
        var back = -AttackDirection();
        if (!IsFlyer)
            back = new Vector2(-Dir, 0f);
        StopAttack();
        _knockFrom = Position;
        _knockTo = Position + back * distance;
        _knockTo.X = Mathf.Clamp(_knockTo.X, -80f, Main.ScreenWidth + 80f);
        _knockDistance = distance;
        _knockT = 0f;
        _knockTime = KnockSeconds;
        Velocity = Vector2.Zero;
    }

    void KnockStep(float dt)
    {
        _knockT += dt;
        float p = Mathf.Clamp(_knockT / _knockTime, 0f, 1f);
        float e = EaseOut(p);
        if (IsFlyer)
        {
            Position = _knockFrom.Lerp(_knockTo, e);
            Hover();
        }
        else
        {
            float x = Mathf.Lerp(_knockFrom.X, _knockTo.X, e);
            Position = new Vector2(x, Main.I.GroundY(x) - Def.Radius);
            float arc = Mathf.Sin(p * Mathf.Pi) * Mathf.Min(_knockDistance * 0.4f, 40f);
            _visual.Position = new Vector2(0f, -arc);
            _visual.Rotation = Dir * 0.35f * Mathf.Sin(p * Mathf.Pi);
        }
        Velocity = Vector2.Zero;
        if (p >= 1f)
            _visual.Rotation = 0f;
    }

    /// <summary>A pepper hit: the core counts the itch and says when it sneezes.</summary>
    public void AddItch(ItchDef itch)
    {
        if (IsDead || !Effects.AddItch(itch))
            return;
        KnockBack(itch.Push);
        var tween = CreateTween();
        tween.TweenProperty(_visual, "scale", new Vector2(1.3f, 0.72f), 0.08);
        tween.TweenProperty(_visual, "scale", Vector2.One, 0.35)
            .SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
        var face = GlobalPosition + new Vector2(Dir * Def.Radius, -Def.Radius * 0.3f);
        Fx.Crumbs(Main.I.FxRoot, face, new Color(0.9f, 0.88f, 0.82f), 14, 220f);
        Fx.Number(Main.I.FxRoot, GlobalPosition + new Vector2(0f, -Def.Radius - 40f), "Hatschi!", new Color(0.85f, 0.95f, 1f), 24);
    }

    Vector2 AttackDirection()
    {
        if (IsFlyer && _target != null && IsInstanceValid(_target))
        {
            var toTarget = _target.GlobalPosition - Position;
            if (toTarget.LengthSquared() > 1f)
                return toTarget.Normalized();
        }
        return new Vector2(Dir, 0f);
    }

    static float EaseOut(float p)
    {
        float q = 1f - Mathf.Clamp(p, 0f, 1f);
        return 1f - q * q * q;
    }

    // ---------------------------------------------------------- being hit

    public void TakeDamage(float damage)
    {
        if (IsDead)
            return;
        Hp -= damage;
        _visual.Flash();
        Fx.Number(Main.I.FxRoot, GlobalPosition + new Vector2((float)GD.RandRange(-12.0, 12.0), -Def.Radius - 14f),
            Mathf.RoundToInt(damage).ToString(), Fx.DamageColor, 26);
        if (!IsFlyer && !_attacking)
            Position += new Vector2(-Dir * 5f, 0f);
        QueueRedraw();
        if (Hp <= 0f)
            Die();
    }

    void Die()
    {
        IsDead = true;
        Main.I.OnEnemyKilled(this);
        var tween = CreateTween().SetParallel(true);
        tween.TweenProperty(this, "position", Position + new Vector2(-Dir * 160f, -240f), 0.65)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(_visual, "rotation", _visual.Rotation - Dir * 9f, 0.65);
        tween.TweenProperty(this, "modulate:a", 0f, 0.45).SetDelay(0.2);
        tween.Chain().TweenCallback(Callable.From(() => QueueFree()));
    }

    void SetDir(float dir)
    {
        Dir = dir;
        _visual.SetFace(dir);
    }

    public override void _Draw()
    {
        if (IsDead || Hp >= Def.Hp)
            return;
        float width = Mathf.Max(36f, Def.Radius * 1.6f);
        var at = new Vector2(-width / 2f, -Def.Radius - 22f);
        DrawRect(new Rect2(at - new Vector2(2f, 2f), new Vector2(width + 4f, 10f)), new Color(0.15f, 0.08f, 0.04f, 0.85f));
        DrawRect(new Rect2(at, new Vector2(width * Mathf.Max(0f, Hp / Def.Hp), 6f)), new Color(0.95f, 0.3f, 0.25f));
    }
}
