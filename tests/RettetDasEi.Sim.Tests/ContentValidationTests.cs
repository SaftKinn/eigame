using RettetDasEi.Sim;

namespace RettetDasEi.Sim.Tests;

/// <summary>Bad content is refused at load with a message naming the file and the entry — never a silent default.</summary>
public class ContentValidationTests
{
    static InvalidOperationException Refused(Func<ContentDb> load) => Assert.Throws<InvalidOperationException>(load);

    [Fact]
    public void The_test_content_itself_is_valid()
    {
        var db = TestContent.Load();

        Assert.Equal(8, db.Units.Count);
        Assert.Equal(2, db.Waves.Count);
        Assert.Equal("egg", db.Unit("egg").Id);
    }

    [Theory]
    [InlineData("game", "\"startGold\": 100", "\"startGold\": -1", "game.json: startGold")]
    [InlineData("game", "\"rangePerHeight\": 0.5", "\"rangePerHeight\": -0.5", "game.json: rangePerHeight")]
    [InlineData("game", "\"lobGravity\": 980", "\"lobGravity\": 0", "game.json: lobGravity")]
    [InlineData("game", "\"sellRefund\": 0.5", "\"sellRefund\": 1.5", "game.json: sellRefund")]
    [InlineData("game", "\"eggUnit\": \"egg\"", "\"eggUnit\": \"chick\"", "game.json: eggUnit 'chick'")]
    [InlineData("units", "\"shape\": \"box\"", "\"shape\": \"star\"", "units.json 'block': shape")]
    [InlineData("units", "\"width\": 100", "\"width\": 0", "units.json 'block': width and height")]
    [InlineData("units", "\"mass\": 2", "\"mass\": 0", "units.json 'block': mass")]
    [InlineData("units", "\"hp\": 100", "\"hp\": 0", "units.json 'block': hp")]
    [InlineData("units", "\"cost\": 20", "\"cost\": -5", "units.json 'block': cost")]
    [InlineData("units", "\"fill\": \"#F2C94C\"", "\"fill\": \"gelb\"", "units.json 'block': fill 'gelb'")]
    [InlineData("units", "\"accent\": \"#C99A1E\"", "\"accent\": \"#C99A1\"", "units.json 'block': accent")]
    [InlineData("units", "\"projectile\": \"straight\"", "\"projectile\": \"laser\"", "units.json 'shooter': attack.projectile")]
    [InlineData("units", "\"damage\": 10", "\"damage\": 0", "units.json 'shooter': attack damage")]
    [InlineData("units", "\"speed\": 900", "\"speed\": 0", "units.json 'shooter': a straight shot needs a speed")]
    [InlineData("units", "\"splash\": 0", "\"splash\": -1", "units.json 'shooter': splash and recoil")]
    [InlineData("units", "\"pierce\": 3", "\"pierce\": 0", "units.json 'shooter': attack.pierce must be 1 or more")]
    [InlineData("units", "\"recoil\": 15, \"pierce\": 1", "\"recoil\": 15, \"pierce\": 2", "units.json 'lobber': only a straight shot pierces")]
    [InlineData("units", "\"radius\": 70, \"seconds\": 4", "\"radius\": 0, \"seconds\": 4", "units.json 'lobber': puddle radius and seconds")]
    [InlineData("units", "\"slow\": 0.4", "\"slow\": 1.5", "units.json 'lobber': puddle slow")]
    [InlineData("units", "\"slow\": 0.4", "\"slow\": 0", "units.json 'lobber': puddle slow")]
    [InlineData("units", "\"hits\": 3", "\"hits\": 0", "units.json 'pepper': itch hits")]
    [InlineData("units", "\"pause\": 1.5", "\"pause\": 0", "units.json 'pepper': itch pause")]
    [InlineData("units", "\"push\": 30", "\"push\": -1", "units.json 'pepper': itch push")]
    [InlineData("units", "\"damage\": 50", "\"damage\": 0", "units.json 'bomb': bomb damage and radius")]
    [InlineData("units", "\"push\": 400", "\"push\": -5", "units.json 'bomb': bomb push")]
    [InlineData("units", "\"tearImpulse\": 400", "\"tearImpulse\": 0", "units.json 'sticky': glue tearImpulse and restSpeed")]
    [InlineData("units", "\"regrabSeconds\": 1", "\"regrabSeconds\": -1", "units.json 'sticky': glue regrabSeconds")]
    [InlineData("units", "\"pushShare\": 0.25", "\"pushShare\": 1.2", "units.json 'wall': shield pushShare")]
    [InlineData("units", "\"throwBack\": 90", "\"throwBack\": -1", "units.json 'wall': shield throwBack")]
    [InlineData("units", "\"id\": \"block\"", "\"id\": \"shooter\"", "units.json: two units share an id")]
    [InlineData("units", "\"inShop\": false", "\"inShop\": true", "units.json: the egg must not be in the shop")]
    [InlineData("enemies", "\"movement\": \"waddle\"", "\"movement\": \"swim\"", "enemies.json 'ant': movement")]
    [InlineData("enemies", "\"radius\": 20", "\"radius\": 0", "enemies.json 'ant': radius, hp and speed")]
    [InlineData("enemies", "\"attackCooldown\": 1, \"gold\": 4", "\"attackCooldown\": 0, \"gold\": 4", "enemies.json 'ant': attackCooldown")]
    [InlineData("enemies", "\"fill\": \"#4A2E22\"", "\"fill\": \"braun\"", "enemies.json 'ant': fill or accent")]
    [InlineData("enemies", "\"id\": \"fly\"", "\"id\": \"ant\"", "enemies.json: two enemies share an id")]
    [InlineData("waves", "\"enemy\": \"fly\"", "\"enemy\": \"wasp\"", "waves.json wave 2: unknown enemy 'wasp'")]
    [InlineData("waves", "\"count\": 3", "\"count\": 0", "waves.json wave 1: count must be above 0")]
    [InlineData("waves", "\"side\": \"left\"", "\"side\": \"top\"", "waves.json wave 2: side")]
    public void A_bad_value_is_refused_naming_file_and_entry(string file, string from, string to, string expected)
    {
        var ex = Refused(() => LoadWith(file, from, to));

        Assert.Contains("Content is broken", ex.Message);
        Assert.Contains(expected, ex.Message);
    }

    [Fact]
    public void A_bomb_does_not_shoot()
    {
        string units = TestContent.Units.Replace(
            "\"bomb\": { \"damage\": 50",
            "\"attack\": { \"projectile\": \"straight\", \"visual\": \"seed\", \"damage\": 9, \"cooldown\": 1, \"range\": 300, \"speed\": 900, \"splash\": 0, \"recoil\": 0, \"pierce\": 1 },\n\"bomb\": { \"damage\": 50");

        Assert.Contains("units.json 'bomb': a bomb does not shoot", Refused(() => TestContent.Load(units: units)).Message);
    }

    [Fact]
    public void Only_a_lob_leaves_a_puddle()
    {
        string units = TestContent.Units.Replace(
            "\"recoil\": 20, \"pierce\": 3 }",
            "\"recoil\": 20, \"pierce\": 3, \"puddle\": { \"radius\": 70, \"seconds\": 4, \"slow\": 0.4 } }");

        Assert.Contains("units.json 'shooter': only a lob leaves a puddle", Refused(() => TestContent.Load(units: units)).Message);
    }

    [Fact]
    public void Every_problem_is_reported_at_once()
    {
        string units = TestContent.Units.Replace("\"mass\": 2", "\"mass\": 0").Replace("\"cost\": 30", "\"cost\": -1");

        var ex = Refused(() => TestContent.Load(units: units));

        Assert.Contains("'block': mass", ex.Message);
        Assert.Contains("'shooter': cost", ex.Message);
    }

    [Fact]
    public void An_unknown_property_is_refused()
    {
        string game = TestContent.Game.Replace("\"startGold\": 100", "\"startGold\": 100, \"startGlod\": 5");

        var ex = Refused(() => TestContent.Load(game: game));

        Assert.StartsWith("game.json:", ex.Message);
        Assert.Contains("startGlod", ex.Message);
    }

    [Fact]
    public void A_missing_field_is_refused()
    {
        // The last field taken out, and the comma before it.
        string game = TestContent.Game.Replace("\"sellRefund\": 0.5,", "\"sellRefund\": 0.5").Replace("\"lobGravity\": 980", "");

        var ex = Refused(() => TestContent.Load(game: game));

        // System.Text.Json names the C# property ("LobGravity"), not the JSON spelling.
        Assert.StartsWith("game.json:", ex.Message);
        Assert.Contains("LobGravity", ex.Message);
    }

    [Fact]
    public void Waves_without_groups_and_no_waves_at_all_are_refused()
    {
        Assert.Contains("waves.json wave 1: no groups", Refused(() => TestContent.Load(waves: "[ { \"reward\": 0, \"groups\": [] } ]")).Message);
        Assert.Contains("waves.json: no waves", Refused(() => TestContent.Load(waves: "[]")).Message);
    }

    [Theory]
    [InlineData("#F2C94C", true)]
    [InlineData("F2C94C", true)]
    [InlineData("#FFF", true)]
    [InlineData("#FFFF", true)]
    [InlineData("#F2C94C80", true)]
    [InlineData("#ffffff", true)]
    [InlineData("#F2C94", false)]
    [InlineData("#GGGGGG", false)]
    [InlineData("red", false)]
    [InlineData("", false)]
    [InlineData("#", false)]
    [InlineData(" #FFF", false)]
    public void A_colour_is_what_Godot_accepts(string text, bool valid) => Assert.Equal(valid, HexColor.IsValid(text));

    static ContentDb LoadWith(string file, string from, string to)
    {
        string Patch(string text)
        {
            Assert.Contains(from, text); // the test's own guard: the text to replace is really there
            return text.Replace(from, to);
        }
        return file switch
        {
            "game" => TestContent.Load(game: Patch(TestContent.Game)),
            "units" => TestContent.Load(units: Patch(TestContent.Units)),
            "enemies" => TestContent.Load(enemies: Patch(TestContent.Enemies)),
            "waves" => TestContent.Load(waves: Patch(TestContent.Waves)),
            _ => throw new ArgumentException(file),
        };
    }
}
