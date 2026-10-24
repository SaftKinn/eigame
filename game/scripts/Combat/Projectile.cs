using System.Collections.Generic;
using Godot;
using RettetDasEi.Sim;

namespace RettetDasEi;

/// <summary>
/// A pickle slice, a pepper burst, a seed or a blob of mustard. Straight shots aim a little
/// ahead of their target and fly on through as many pests as they pierce; lobbed ones pick an
/// arc, splash where they land and may leave a puddle. A hit with an itch tickles (ADR 003).
/// </summary>
public partial class Projectile : Node2D
{
    const float HitRadius = 10f;

    AttackDef _attack = null!;
    Vector2 _velocity;
    bool _lob;
    float _life = 3f;
    float _spin;
    bool _done;
    readonly List<Enemy> _hit = new();

    public void Setup(AttackDef attack, Vector2 from, Enemy target)
    {
        _attack = attack;
        _lob = attack.Projectile == "lob";
        Position = from;
        Vector2 aim = target.GlobalPosition;
        if (_lob)
        {
            float g = GameContent.Db.Game.LobGravity;
            float time = Mathf.Clamp(from.DistanceTo(aim) / 550f, 0.55f, 1.3f);
            Vector2 goal = aim + target.Velocity * time;
            if (!target.IsFlyer)
                goal.Y = Main.I.GroundY(goal.X) - 4f;
            _velocity = new Vector2((goal.X - from.X) / time, (goal.Y - from.Y - 0.5f * g * time * time) / time);
        }
        else
        {
            float time = from.DistanceTo(aim) / attack.Speed;
            Vector2 goal = aim + target.Velocity * time;
            _velocity = (goal - from).Normalized() * attack.Speed;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_done)
            return;
        float dt = (float)delta;
        if (_lob)
            _velocity.Y += GameContent.Db.Game.LobGravity * dt;
        Position += _velocity * dt;
        _spin += dt * 14f;
        _life -= dt;
        QueueRedraw();

        Enemy? hit = null;
        foreach (var enemy in Main.I.Enemies)
        {
            if (!enemy.IsDead && !_hit.Contains(enemy) && enemy.GlobalPosition.DistanceTo(GlobalPosition) <= enemy.Def.Radius + HitRadius)
            {
                hit = enemy;
                break;
            }
        }
        if (hit != null)
        {
            if (_lob || _attack.Splash > 0f)
            {
                Impact(hit);
                return;
            }
            // A straight shot: this pest is hit, and the core says whether the shot flies on.
            _hit.Add(hit);
            Strike(hit);
            Fx.Crumbs(Main.I.FxRoot, GlobalPosition, DrawKit.ProjectileColor(_attack.Visual), 5, 150f);
            if (!Pierce.FliesOn(_attack, _hit.Count))
            {
                _done = true;
                QueueFree();
                return;
            }
        }
        if (_lob && Position.Y >= Main.I.GroundY(Position.X))
        {
            Impact(null);
            return;
        }
        if (_life <= 0f || Position.Y > 1300f || Position.X < -200f || Position.X > Main.ScreenWidth + 200f)
        {
            _done = true;
            QueueFree();
        }
    }

    void Impact(Enemy? direct)
    {
        _done = true;
        var color = DrawKit.ProjectileColor(_attack.Visual);
        if (_attack.Splash > 0f)
        {
            // Collect first: a kill takes the enemy out of the list we are reading.
            var victims = new List<Enemy>();
            foreach (var enemy in Main.I.Enemies)
                if (!enemy.IsDead && enemy.GlobalPosition.DistanceTo(GlobalPosition) <= _attack.Splash + enemy.Def.Radius)
                    victims.Add(enemy);
            foreach (var enemy in victims)
                Strike(enemy);
            Fx.Crumbs(Main.I.FxRoot, GlobalPosition, color, 16, 260f);
            Main.I.Shake(3f);
        }
        else if (direct != null)
        {
            Strike(direct);
            Fx.Crumbs(Main.I.FxRoot, GlobalPosition, color, 5, 150f);
        }
        if (_attack.Puddle != null)
            Main.I.AddPuddle(_attack.Puddle, GlobalPosition.X);
        QueueFree();
    }

    /// <summary>The damage, then the itch if this attack has one and the pest is still alive.</summary>
    void Strike(Enemy enemy)
    {
        enemy.TakeDamage(_attack.Damage);
        if (_attack.Itch != null && !enemy.IsDead)
            enemy.AddItch(_attack.Itch);
    }

    public override void _Draw()
    {
        DrawSetTransform(Vector2.Zero, _attack.Visual == "seed" ? _velocity.Angle() : 0f, Vector2.One);
        DrawKit.DrawProjectile(this, _attack.Visual, _spin);
        DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
    }
}
