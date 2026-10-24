using Godot;

namespace RettetDasEi;

/// <summary>The kitchen behind everything: tiled wall, window, shelf, counter and the cutting board.</summary>
public partial class KitchenBackdrop : Node2D
{
    public override void _Draw()
    {
        const float w = Main.ScreenWidth;
        const float counter = Main.CounterTop;

        // Wall, with tiles behind the counter.
        DrawRect(new Rect2(0f, 0f, w, counter), Color.FromHtml("#F6E6CC"));
        DrawRect(new Rect2(0f, 520f, w, counter - 520f), Color.FromHtml("#F1DCBA"));
        var grout = Color.FromHtml("#E2C79E");
        int row = 0;
        for (float y = 520f; y < counter; y += 68f, row++)
        {
            DrawLine(new Vector2(0f, y), new Vector2(w, y), grout, 3f);
            for (float x = row % 2 == 0 ? 0f : 34f; x < w; x += 68f)
                DrawLine(new Vector2(x, y), new Vector2(x, Mathf.Min(y + 68f, counter)), grout, 3f);
        }

        // Window.
        DrawKit.Box(this, new Rect2(200f, 80f, 440f, 300f), Color.FromHtml("#BFE3F2"), Color.FromHtml("#FFFFFF"), 6, 14);
        DrawLine(new Vector2(420f, 90f), new Vector2(420f, 370f), Colors.White, 10f);
        DrawLine(new Vector2(210f, 230f), new Vector2(630f, 230f), Colors.White, 10f);
        DrawCircle(new Vector2(320f, 160f), 34f, new Color(1f, 0.93f, 0.6f));

        // Clock.
        var clock = new Vector2(960f, 170f);
        DrawCircle(clock, 62f, Colors.White);
        DrawArc(clock, 62f, 0f, Mathf.Tau, 48, Color.FromHtml("#C98B4F"), 8f, true);
        DrawLine(clock, clock + new Vector2(0f, -40f), DrawKit.Ink, 5f, true);
        DrawLine(clock, clock + new Vector2(28f, 10f), DrawKit.Ink, 5f, true);

        // Shelf with jars.
        DrawRect(new Rect2(1320f, 300f, 460f, 18f), Color.FromHtml("#B07A45"));
        string[] jars = { "#E9A23B", "#9CC56A", "#D9604C", "#7FB5D9", "#C9A0DC" };
        for (int i = 0; i < jars.Length; i++)
        {
            float x = 1345f + i * 86f;
            float h = 70f + (i % 3) * 18f;
            DrawKit.Box(this, new Rect2(x, 300f - h, 64f, h), Color.FromHtml(jars[i]), Color.FromHtml("#8A5530"), 10, 3);
            DrawRect(new Rect2(x + 6f, 300f - h - 10f, 52f, 12f), Color.FromHtml("#8A5530"));
        }

        // Counter top and front.
        DrawRect(new Rect2(0f, counter, w, 26f), Color.FromHtml("#C98B4F"));
        DrawLine(new Vector2(0f, counter + 1f), new Vector2(w, counter + 1f), Color.FromHtml("#E3A869"), 4f);
        DrawRect(new Rect2(0f, counter + 26f, w, 400f), Color.FromHtml("#9A5F35"));
        for (float x = 40f; x < w; x += 320f)
        {
            DrawRect(new Rect2(x, counter + 50f, 280f, 170f), Color.FromHtml("#84502B"), false, 4f);
            DrawCircle(new Vector2(x + 140f, counter + 70f), 7f, Color.FromHtml("#E3C08A"));
        }

        // Cutting board: where the egg is safe.
        DrawKit.Box(this, new Rect2(Main.BoardLeft, Main.BoardTop, Main.BoardRight - Main.BoardLeft, counter - Main.BoardTop + 4f),
            Color.FromHtml("#E8BB84"), Color.FromHtml("#B98550"), 10, 3);
        DrawCircle(new Vector2(Main.BoardRight - 26f, Main.BoardTop + 15f), 7f, Color.FromHtml("#9A5F35"));
    }
}
