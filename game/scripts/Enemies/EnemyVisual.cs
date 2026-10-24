using Godot;
using RettetDasEi.Sim;

namespace RettetDasEi;

/// <summary>
/// What a pest looks like. Walking, rolling, flying and attacking are all this node being moved,
/// turned and squashed by <see cref="Enemy"/> — no frames (ADR 001). Uses
/// art/enemies/&lt;visual&gt;.png when it exists (drawn facing left), else DrawKit's stand-in.
/// </summary>
public partial class EnemyVisual : Node2D
{
    public EnemyDef Def { get; private set; } = null!;
    public float Face { get; private set; } = -1f;
    /// <summary>Turn of a rolling pest's surface (its eyes stay level).</summary>
    public float Spin { get; set; }
    /// <summary>0..1, how far a flyer's wings are open.</summary>
    public float Flap { get; set; } = 1f;

    Sprite2D? _sprite;
    Tween? _flash;

    public void Setup(EnemyDef def)
    {
        Def = def;
        string path = $"res://art/enemies/{def.Visual}.png";
        if (ResourceLoader.Exists(path))
        {
            var texture = GD.Load<Texture2D>(path);
            float scale = def.Radius * 2.3f / Mathf.Max(texture.GetWidth(), texture.GetHeight());
            _sprite = new Sprite2D { Texture = texture, Scale = new Vector2(scale, scale) };
            AddChild(_sprite);
        }
    }

    public void SetFace(float face)
    {
        Face = face;
        if (_sprite != null)
            _sprite.FlipH = face > 0f;
        QueueRedraw();
    }

    public void Flash()
    {
        _flash?.Kill();
        Modulate = new Color(2.4f, 2.4f, 2.4f);
        _flash = CreateTween();
        _flash.TweenProperty(this, "modulate", Colors.White, 0.12);
    }

    public override void _Draw()
    {
        if (_sprite == null)
            DrawKit.DrawEnemy(this, Def, Face, Spin, Flap);
    }
}
