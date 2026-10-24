namespace RettetDasEi.Sim;

// The numbers of the food's jobs (ADR 003). Godot finds who touches whom and how far apart they
// are; these say what follows (ADR 002).

public static class Bomb
{
    /// <summary>A pest whose edge lies within the radius takes the bomb's whole damage.</summary>
    public static bool Hits(BombDef bomb, float distance, float pestRadius) => distance <= bomb.Radius + pestRadius;

    /// <summary>
    /// The impulse on a piece of own food <paramref name="distance"/> from the centre: the whole
    /// push at the centre, nothing once its bounding circle is out of the radius. Never damage.
    /// </summary>
    public static float PushAt(BombDef bomb, float distance, float pieceRadius)
    {
        float reach = bomb.Radius + pieceRadius;
        return distance >= reach ? 0f : bomb.Push * (1f - distance / reach);
    }
}

public static class Glue
{
    /// <summary>A blow at least the cheese's tear impulse tears the glue of the piece it hits.</summary>
    public static bool Tears(GlueDef glue, float impulse) => impulse >= glue.TearImpulse;
}

public static class Shield
{
    /// <summary>The push a piece takes from a pest's blow: all of it, or a shield's share.</summary>
    public static float PushOn(ShieldDef? shield, float push) => shield == null ? push : push * shield.PushShare;
}

public static class Pierce
{
    /// <summary>Whether a shot that has hit <paramref name="hits"/> pests flies on.</summary>
    public static bool FliesOn(AttackDef attack, int hits) => hits < attack.Pierce;
}
