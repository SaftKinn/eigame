using Godot;

namespace RettetDasEi;

/// <summary>While a shooter is in the hand: how far it would reach from there, marked on the counter.</summary>
public partial class RangeOverlay : Node2D
{
    public override void _Process(double delta) => QueueRedraw();

    public override void _Draw()
    {
        var unit = Main.I.HeldUnit;
        if (unit == null || unit.Def.Attack == null)
            return;
        float range = unit.EffectiveRange;
        float x = unit.GlobalPosition.X;
        var band = new Color(1f, 0.85f, 0.2f, 0.3f);
        var edge = new Color(1f, 0.8f, 0.15f, 0.9f);
        DrawRect(new Rect2(x - range, Main.CounterTop - 4f, 2f * range, 12f), band);
        DrawLine(new Vector2(x - range, Main.CounterTop - 34f), new Vector2(x - range, Main.CounterTop + 8f), edge, 4f);
        DrawLine(new Vector2(x + range, Main.CounterTop - 34f), new Vector2(x + range, Main.CounterTop + 8f), edge, 4f);
        DrawString(ThemeDB.FallbackFont, new Vector2(x - 220f, Main.CounterTop - 44f), "Reichweite – je höher, desto weiter",
            HorizontalAlignment.Center, 440f, 22, new Color(0.35f, 0.2f, 0.05f));
    }
}
