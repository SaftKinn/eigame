using System;
using Godot;
using RettetDasEi.Sim;

namespace RettetDasEi;

/// <summary>
/// One piece of food for sale. Pressing on it puts a new one in the hand; a right click switches a
/// food with two shapes to its other one (<see cref="UnitDef.Alternate"/> — the cheese block and
/// the cheese triangle), shown small in the card's corner.
/// </summary>
public partial class ShopCard : Control
{
    public UnitDef Def { get; private set; } = null!;
    public Action<UnitDef>? OnPick { get; set; }

    bool _available = true;
    bool _hover;

    public void Setup(UnitDef def)
    {
        CustomMinimumSize = new Vector2(164f, 146f);
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.None;
        MouseDefaultCursorShape = CursorShape.PointingHand;
        Show(def);
        MouseEntered += () =>
        {
            _hover = true;
            QueueRedraw();
        };
        MouseExited += () =>
        {
            _hover = false;
            QueueRedraw();
        };
    }

    void Show(UnitDef def)
    {
        Def = def;
        TooltipText = $"{def.Name} – {def.Cost} Gold\n{def.Description}"
            + (def.Alternate is { } other ? $"\nRechtsklick: {GameContent.Db.Unit(other).Name}" : "");
        QueueRedraw();
    }

    public void SetAvailable(bool available)
    {
        if (available == _available)
            return;
        _available = available;
        QueueRedraw();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
        {
            OnPick?.Invoke(Def);
            AcceptEvent();
        }
        else if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right } && Def.Alternate is { } other)
        {
            Show(GameContent.Db.Unit(other));
            AcceptEvent();
        }
    }

    public override void _Draw()
    {
        var rect = new Rect2(Vector2.Zero, Size);
        var background = _hover && _available ? new Color(1f, 0.93f, 0.78f) : new Color(0.93f, 0.84f, 0.66f);
        DrawKit.Box(this, rect, background, new Color(0.55f, 0.36f, 0.2f), 14, 4);

        float scale = Mathf.Min(1f, Mathf.Min(120f / Def.Width, 50f / Def.Height));
        DrawKit.DrawUnit(this, Def, new Vector2(Size.X / 2f, 38f), scale, 1f, 1f);

        // Name, its job in a sentence (ADR 003: the card says what the food is for), the price.
        var font = ThemeDB.FallbackFont;
        DrawString(font, new Vector2(0f, 84f), Def.Name, HorizontalAlignment.Center, Size.X, 18, new Color(0.25f, 0.15f, 0.08f));
        DrawMultilineString(font, new Vector2(6f, 102f), Def.Description, HorizontalAlignment.Center, Size.X - 12f, 13, 2,
            new Color(0.38f, 0.25f, 0.14f));
        DrawString(font, new Vector2(0f, 139f), $"{Def.Cost} Gold", HorizontalAlignment.Center, Size.X, 17, new Color(0.62f, 0.42f, 0.04f));

        // The other shape, small in the corner: a right click switches to it (the tooltip says so).
        if (Def.Alternate is { } other)
        {
            var otherDef = GameContent.Db.Unit(other);
            float small = Mathf.Min(26f / otherDef.Width, 22f / otherDef.Height);
            DrawKit.DrawUnit(this, otherDef, new Vector2(Size.X - 20f, 18f), small, 1f, 1f);
        }

        if (!_available)
            DrawKit.Box(this, rect, new Color(0.3f, 0.25f, 0.2f, 0.5f), new Color(0f, 0f, 0f, 0f), 14, 0);
    }
}
