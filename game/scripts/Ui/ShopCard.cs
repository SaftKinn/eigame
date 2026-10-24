using System;
using Godot;

namespace RettetDasEi;

/// <summary>One piece of food for sale. Pressing on it puts a new one in the hand.</summary>
public partial class ShopCard : Control
{
    public UnitDef Def { get; private set; } = null!;
    public Action<UnitDef>? OnPick { get; set; }

    bool _available = true;
    bool _hover;

    public void Setup(UnitDef def)
    {
        Def = def;
        CustomMinimumSize = new Vector2(164f, 146f);
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.None;
        MouseDefaultCursorShape = CursorShape.PointingHand;
        TooltipText = $"{def.Name} – {def.Cost} Gold\n{def.Description}";
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
    }

    public override void _Draw()
    {
        var rect = new Rect2(Vector2.Zero, Size);
        var background = _hover && _available ? new Color(1f, 0.93f, 0.78f) : new Color(0.93f, 0.84f, 0.66f);
        DrawKit.Box(this, rect, background, new Color(0.55f, 0.36f, 0.2f), 14, 4);

        float scale = Mathf.Min(1f, Mathf.Min(120f / Def.Width, 72f / Def.Height));
        DrawKit.DrawUnit(this, Def, new Vector2(Size.X / 2f, 54f), scale, 1f, 1f);

        var font = ThemeDB.FallbackFont;
        DrawString(font, new Vector2(0f, 114f), Def.Name, HorizontalAlignment.Center, Size.X, 19, new Color(0.25f, 0.15f, 0.08f));
        DrawString(font, new Vector2(0f, 137f), $"{Def.Cost} Gold", HorizontalAlignment.Center, Size.X, 19, new Color(0.62f, 0.42f, 0.04f));

        if (!_available)
            DrawKit.Box(this, rect, new Color(0.3f, 0.25f, 0.2f, 0.5f), new Color(0f, 0f, 0f, 0f), 14, 0);
    }
}
