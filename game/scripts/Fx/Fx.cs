using Godot;

namespace RettetDasEi;

/// <summary>Little effects that make hits feel like hits: numbers and crumbs.</summary>
public static class Fx
{
    public static readonly Color GoldColor = new(1f, 0.83f, 0.3f);
    public static readonly Color DamageColor = new(1f, 0.95f, 0.85f);
    public static readonly Color HurtColor = new(1f, 0.4f, 0.32f);

    public static void Number(Node parent, Vector2 at, string text, Color tint, int fontSize = 28)
    {
        parent.AddChild(new FloatingText { Text = text, Tint = tint, FontSize = fontSize, Position = at });
    }

    /// <summary>A one-shot burst of little squares that fall and fade.</summary>
    public static void Crumbs(Node parent, Vector2 at, Color color, int amount, float speed = 260f)
    {
        var ramp = new Gradient();
        ramp.SetColor(0, color);
        ramp.SetColor(1, new Color(color, 0f));
        var particles = new CpuParticles2D
        {
            Position = at,
            Amount = amount,
            Lifetime = 0.7,
            OneShot = true,
            Explosiveness = 1f,
            Direction = Vector2.Up,
            Spread = 180f,
            Gravity = new Vector2(0f, 900f),
            InitialVelocityMin = speed * 0.5f,
            InitialVelocityMax = speed,
            ScaleAmountMin = 4f,
            ScaleAmountMax = 9f,
            ColorRamp = ramp,
            Emitting = false,
        };
        parent.AddChild(particles);
        particles.Emitting = true;
        particles.GetTree().CreateTimer(1.2).Timeout += () =>
        {
            if (GodotObject.IsInstanceValid(particles))
                particles.QueueFree();
        };
    }
}
