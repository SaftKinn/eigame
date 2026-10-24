using Godot;

namespace RettetDasEi;

/// <summary>A small health bar that floats above a piece of food and never turns with it.</summary>
public partial class HpBar : Node2D
{
    const float Width = 56f;
    const float Height = 8f;

    float _ratio = 1f;

    public void SetRatio(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0f, 1f);
        if (Mathf.IsEqualApprox(ratio, _ratio))
            return;
        _ratio = ratio;
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(-Width / 2f - 2f, -Height / 2f - 2f, Width + 4f, Height + 4f), new Color(0.15f, 0.08f, 0.04f, 0.85f));
        var color = _ratio > 0.5f ? new Color(0.45f, 0.85f, 0.3f)
            : _ratio > 0.25f ? new Color(1f, 0.75f, 0.2f)
            : new Color(1f, 0.3f, 0.25f);
        DrawRect(new Rect2(-Width / 2f, -Height / 2f, Width * _ratio, Height), color);
    }
}
