namespace RettetDasEi.Sim;

public static class Reach
{
    /// <summary>
    /// Higher up shoots further — the core rule of the tower: an attack's range plus
    /// <c>rangePerHeight</c> for every pixel the shooter stands above the board (none below it).
    /// The shooter and the range overlay both ask this.
    /// </summary>
    public static float Of(AttackDef attack, GameDef game, float heightAboveBoard) =>
        attack.Range + Math.Max(0f, heightAboveBoard) * game.RangePerHeight;
}
