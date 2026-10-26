using System;
using System.Collections.Generic;
using Godot;
using RettetDasEi.Sim;

namespace RettetDasEi;

/// <summary>Gold, wave, messages, the shop along the bottom and the end screen.</summary>
public partial class Hud : CanvasLayer
{
    const float ShopHeight = 176f;

    Label _gold = null!;
    Label _wave = null!;
    Label _speed = null!;
    Label _message = null!;
    Label _warning = null!;
    Label _sellHint = null!;
    Button _start = null!;
    PanelContainer _shop = null!;
    Control _end = null!;
    Label _endTitle = null!;
    Label _endText = null!;
    readonly List<ShopCard> _cards = new();
    float _messageTime;

    public void Build(IEnumerable<UnitDef> shopUnits, Action<UnitDef> onBuy, Action onStartWave, Action onRestart)
    {
        Layer = 10;
        var root = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
        AddChild(root);
        Anchor(root, Control.LayoutPreset.FullRect);

        // Top left: gold, wave, speed.
        var info = new VBoxContainer { Position = new Vector2(32f, 20f), MouseFilter = Control.MouseFilterEnum.Ignore };
        root.AddChild(info);
        _gold = MakeLabel(46, Fx.GoldColor);
        _wave = MakeLabel(30, Colors.White);
        _speed = MakeLabel(24, new Color(0.7f, 0.95f, 1f));
        info.AddChild(_gold);
        info.AddChild(_wave);
        info.AddChild(_speed);

        // Top centre: messages and the warning about the egg.
        _message = MakeLabel(38, Colors.White);
        _message.HorizontalAlignment = HorizontalAlignment.Center;
        root.AddChild(_message);
        Anchor(_message, Control.LayoutPreset.TopWide, 28f, 90f);

        _warning = MakeLabel(34, new Color(1f, 0.45f, 0.4f));
        _warning.HorizontalAlignment = HorizontalAlignment.Center;
        _warning.Text = "Das Ei liegt auf der Theke – leg es aufs Brett oder auf den Turm!";
        _warning.Visible = false;
        root.AddChild(_warning);
        Anchor(_warning, Control.LayoutPreset.TopWide, 96f, 150f);

        // Above the shop: how to play, and the hint while selling.
        var help = MakeLabel(18, new Color(1f, 0.96f, 0.88f));
        help.HorizontalAlignment = HorizontalAlignment.Center;
        help.Text = "Karte ins Spielfeld ziehen (Rechtsklick auf Karte: andere Form) · Essen ziehen hebt alles Verklebte mit "
            + "(Rechtsklick dabei: nur dieses Teil) · Drehen: Mausrad oder Q/E · G: Kleben aus/an\n"
            + "Verkaufen: zurück in den Laden ziehen · Regal links: Abstellplatz · Leertaste: Welle · F: schneller · R: neu starten";
        root.AddChild(help);
        Anchor(help, Control.LayoutPreset.BottomWide, -ShopHeight - 58f, -ShopHeight - 4f);

        _sellHint = MakeLabel(30, Fx.GoldColor);
        _sellHint.HorizontalAlignment = HorizontalAlignment.Center;
        _sellHint.Visible = false;
        root.AddChild(_sellHint);
        Anchor(_sellHint, Control.LayoutPreset.BottomWide, -ShopHeight - 84f, -ShopHeight - 42f);

        // The shop.
        _shop = new PanelContainer { MouseFilter = Control.MouseFilterEnum.Stop };
        var style = new StyleBoxFlat
        {
            BgColor = new Color(0.36f, 0.22f, 0.12f, 0.95f),
            BorderColor = new Color(0.22f, 0.13f, 0.07f),
            BorderWidthTop = 6,
            ContentMarginLeft = 24f,
            ContentMarginRight = 24f,
            ContentMarginTop = 14f,
            ContentMarginBottom = 14f,
        };
        _shop.AddThemeStyleboxOverride("panel", style);
        root.AddChild(_shop);
        Anchor(_shop, Control.LayoutPreset.BottomWide, -ShopHeight, 0f);

        var row = new HBoxContainer { MouseFilter = Control.MouseFilterEnum.Pass };
        row.AddThemeConstantOverride("separation", 14);
        _shop.AddChild(row);
        foreach (var def in shopUnits)
        {
            var card = new ShopCard();
            card.Setup(def);
            card.OnPick = onBuy;
            row.AddChild(card);
            _cards.Add(card);
        }
        row.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, MouseFilter = Control.MouseFilterEnum.Ignore });

        _start = new Button
        {
            Text = "Welle starten (Leertaste)",
            CustomMinimumSize = new Vector2(340f, 100f),
            FocusMode = Control.FocusModeEnum.None,
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
        };
        _start.AddThemeFontSizeOverride("font_size", 28);
        _start.Pressed += onStartWave;
        row.AddChild(_start);

        // End screen.
        _end = new ColorRect { Color = new Color(0f, 0f, 0f, 0.55f), MouseFilter = Control.MouseFilterEnum.Stop, Visible = false };
        root.AddChild(_end);
        Anchor(_end, Control.LayoutPreset.FullRect);
        var center = new CenterContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        _end.AddChild(center);
        Anchor(center, Control.LayoutPreset.FullRect);
        var box = new VBoxContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        box.AddThemeConstantOverride("separation", 26);
        center.AddChild(box);
        _endTitle = MakeLabel(88, Fx.GoldColor);
        _endTitle.HorizontalAlignment = HorizontalAlignment.Center;
        box.AddChild(_endTitle);
        _endText = MakeLabel(34, Colors.White);
        _endText.HorizontalAlignment = HorizontalAlignment.Center;
        box.AddChild(_endText);
        var again = new Button
        {
            Text = "Nochmal (R)",
            CustomMinimumSize = new Vector2(320f, 84f),
            FocusMode = Control.FocusModeEnum.None,
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
        };
        again.AddThemeFontSizeOverride("font_size", 32);
        again.Pressed += onRestart;
        box.AddChild(again);
    }

    /// <summary>
    /// Anchors and all four offsets together. SetAnchorsPreset alone keeps a control where it
    /// was (Godot recomputes the offsets), which left the whole HUD at a size of zero.
    /// </summary>
    static void Anchor(Control control, Control.LayoutPreset preset, float top = 0f, float bottom = 0f)
    {
        control.SetAnchorsAndOffsetsPreset(preset);
        control.OffsetLeft = 0f;
        control.OffsetRight = 0f;
        control.OffsetTop = top;
        control.OffsetBottom = bottom;
    }

    static Label MakeLabel(int size, Color color) => new()
    {
        MouseFilter = Control.MouseFilterEnum.Ignore,
        LabelSettings = new LabelSettings
        {
            FontSize = size,
            FontColor = color,
            OutlineSize = Math.Max(6, size / 5),
            OutlineColor = new Color(0.22f, 0.12f, 0.06f),
        },
    };

    public bool IsOverShop(Vector2 screenPosition) => _shop.GetGlobalRect().HasPoint(screenPosition);

    public void Refresh(int gold, int waveIndex, int waveCount, Phase phase, bool canStart, bool eggOnCounter, bool fast)
    {
        _gold.Text = $"Gold: {gold}";
        _wave.Text = phase == Phase.Wave
            ? $"Welle {waveIndex + 1} von {waveCount} läuft"
            : $"Nächste Welle: {Math.Min(waveIndex + 1, waveCount)} von {waveCount}";
        _speed.Text = fast ? "Tempo ×2 (F)" : "";
        _start.Disabled = !canStart;
        _start.Text = phase == Phase.Wave ? "Welle läuft …" : "Welle starten (Leertaste)";
        _warning.Visible = phase == Phase.Build && eggOnCounter;
        foreach (var card in _cards)
            card.SetAvailable(phase == Phase.Build && gold >= card.Def.Cost);
    }

    public void ShowMessage(string text, float seconds = 3f)
    {
        _message.Text = text;
        _message.Modulate = Colors.White;
        _messageTime = seconds;
    }

    public void ShowSellHint(int? refund)
    {
        _sellHint.Visible = refund.HasValue;
        if (refund.HasValue)
            _sellHint.Text = $"Loslassen = verkaufen (+{refund.Value} Gold)";
    }

    public void ShowEnd(string title, string text)
    {
        _endTitle.Text = title;
        _endText.Text = text;
        _end.Visible = true;
    }

    public override void _Process(double delta)
    {
        if (_messageTime <= 0f)
            return;
        _messageTime -= (float)delta;
        _message.Modulate = new Color(1f, 1f, 1f, Mathf.Clamp(_messageTime / 0.5f, 0f, 1f));
    }
}
