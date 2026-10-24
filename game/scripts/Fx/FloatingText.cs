using Godot;

namespace RettetDasEi;

/// <summary>A number that pops up, rises and fades: damage, gold, refunds.</summary>
public partial class FloatingText : Node2D
{
    public string Text { get; set; } = "";
    public Color Tint { get; set; } = Colors.White;
    public int FontSize { get; set; } = 28;

    public override void _Ready()
    {
        var tween = CreateTween().SetParallel(true);
        tween.TweenProperty(this, "position", Position + new Vector2(0f, -60f), 0.8)
            .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(this, "scale", Vector2.One, 0.15).From(new Vector2(1.4f, 1.4f));
        tween.TweenProperty(this, "modulate:a", 0f, 0.4).SetDelay(0.4);
        tween.Chain().TweenCallback(Callable.From(() => QueueFree()));
    }

    public override void _Draw()
    {
        var font = ThemeDB.FallbackFont;
        var at = new Vector2(-120f, 0f);
        DrawStringOutline(font, at, Text, HorizontalAlignment.Center, 240f, FontSize, 8, new Color(0.15f, 0.08f, 0.04f));
        DrawString(font, at, Text, HorizontalAlignment.Center, 240f, FontSize, Tint);
    }
}
