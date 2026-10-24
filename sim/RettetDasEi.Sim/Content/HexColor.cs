namespace RettetDasEi.Sim;

/// <summary>
/// The colour check of content without Godot: what Godot's <c>Color.HtmlIsValid</c> accepts — an
/// optional '#', then 3, 4, 6 or 8 hex digits (checked against GodotSharp 4.6.2 on 2026-10-24).
/// </summary>
public static class HexColor
{
    public static bool IsValid(string text)
    {
        string hex = text.StartsWith('#') ? text[1..] : text;
        if (hex.Length is not (3 or 4 or 6 or 8))
            return false;
        foreach (char c in hex)
            if (!char.IsAsciiHexDigit(c))
                return false;
        return true;
    }
}
