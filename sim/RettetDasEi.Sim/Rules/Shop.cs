namespace RettetDasEi.Sim;

public static class Shop
{
    /// <summary>
    /// What a piece gives back when sold: the full price while it is still in the hand since it was
    /// bought; once placed, <c>cost × sellRefund × hp / maxHp</c>, rounded half to even — as Godot's
    /// <c>Mathf.RoundToInt</c> did in the prototype (checked 2026-10-24: 2.5 → 2, 3.5 → 4).
    /// </summary>
    public static int Refund(UnitDef def, GameDef game, bool justBought, float hp, float maxHp) =>
        justBought ? def.Cost : (int)MathF.Round(def.Cost * game.SellRefund * hp / maxHp);
}
