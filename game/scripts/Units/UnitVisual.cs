using Godot;
using RettetDasEi.Sim;

namespace RettetDasEi;

/// <summary>
/// What a piece of food looks like. A child of its body, so a shot can squash and tilt the
/// picture without moving the physics. Uses art/units/&lt;visual&gt;.png when it exists
/// (drawn facing right), else DrawKit's stand-in.
/// </summary>
public partial class UnitVisual : Node2D
{
    public UnitDef Def { get; private set; } = null!;
    public float Face { get; private set; } = 1f;
    public float HpRatio { get; private set; } = 1f;

    Sprite2D? _sprite;
    Tween? _flash;

    public void Setup(UnitDef def)
    {
        Def = def;
        string path = $"res://art/units/{def.Visual}.png";
        if (ResourceLoader.Exists(path))
        {
            var texture = GD.Load<Texture2D>(path);
            float scale = Mathf.Min(def.Width / texture.GetWidth(), def.Height / texture.GetHeight());
            _sprite = new Sprite2D { Texture = texture, Scale = new Vector2(scale, scale) };
            AddChild(_sprite);
        }
    }

    public void SetFace(float face)
    {
        if (face == Face)
            return;
        Face = face;
        if (_sprite != null)
            _sprite.FlipH = face < 0f;
        QueueRedraw();
    }

    public void SetHpRatio(float ratio)
    {
        HpRatio = ratio;
        QueueRedraw();
    }

    /// <summary>A short white flash when hit.</summary>
    public void Flash()
    {
        _flash?.Kill();
        Modulate = new Color(2.4f, 2.4f, 2.4f);
        _flash = CreateTween();
        _flash.TweenProperty(this, "modulate", Colors.White, 0.15);
    }

    public override void _Draw()
    {
        if (_sprite == null)
            DrawKit.DrawUnit(this, Def, Vector2.Zero, 1f, Face, HpRatio);
    }
}
