using Godot;
using RettetDasEi.Sim;

namespace RettetDasEi;

/// <summary>
/// A mustard puddle on the ground (ADR 003). Walkers that stand in it are reported to their
/// <see cref="PestEffects"/> by <see cref="Main.ApplyPuddles"/>; the core decides how slow.
/// </summary>
public partial class Puddle : Node2D
{
    public PuddleDef Def { get; private set; } = null!;
    public bool Done => _left <= 0f;

    float _left;
    float _age;

    public void Setup(PuddleDef def, Vector2 at)
    {
        Def = def;
        _left = def.Seconds;
        Position = at;
    }

    public override void _PhysicsProcess(double delta)
    {
        _left -= (float)delta;
        _age += (float)delta;
        if (Done)
            QueueFree();
    }

    public override void _Process(double delta) => QueueRedraw();

    public override void _Draw()
    {
        float grow = Mathf.Clamp(_age / 0.2f, 0f, 1f);
        float fade = Mathf.Clamp(_left / 0.8f, 0f, 1f);
        DrawSetTransform(Vector2.Zero, 0f, new Vector2(grow, 0.22f * grow));
        DrawCircle(Vector2.Zero, Def.Radius, new Color(0.86f, 0.62f, 0.08f, 0.85f * fade));
        DrawCircle(new Vector2(-Def.Radius * 0.2f, -Def.Radius * 0.15f), Def.Radius * 0.7f, new Color(0.97f, 0.78f, 0.2f, 0.9f * fade));
        DrawCircle(new Vector2(Def.Radius * 0.35f, -Def.Radius * 0.3f), Def.Radius * 0.15f, new Color(1f, 0.95f, 0.7f, 0.8f * fade));
        DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
    }
}
