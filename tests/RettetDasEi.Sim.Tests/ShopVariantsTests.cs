using RettetDasEi.Sim;

namespace RettetDasEi.Sim.Tests;

/// <summary>
/// Two shapes of one food share a shop card (his word, 2026-10-26: a right click on the cheese
/// card switches it to a cheese triangle): each names the other as its <c>alternate</c>.
/// </summary>
public class ShopVariantsTests
{
    const string Wedge = """
          {
            "id": "wedge", "name": "Keil", "description": "Klebt auch.", "inShop": true, "cost": 15,
            "shape": "triangle", "width": 110, "height": 95, "mass": 2.5, "friction": 1, "hp": 100,
            "fill": "#F2C94C", "accent": "#C99A1E", "visual": "cheese_triangle", "alternate": "sticky",
            "glue": { "tearImpulse": 400, "restSpeed": 20, "regrabSeconds": 1 }
          }
        """;

    /// <summary>The test units with the sticky block naming <paramref name="stickyAlternate"/> and the wedge added.</summary>
    static string UnitsWithWedge(string stickyAlternate = "wedge", string wedge = Wedge)
    {
        string units = TestContent.Units.Replace("\"visual\": \"cheese\",", $"\"visual\": \"cheese\", \"alternate\": \"{stickyAlternate}\",");
        int end = units.LastIndexOf(']');
        return units[..end].TrimEnd() + ",\n" + wedge + "\n]";
    }

    [Fact]
    public void A_pair_of_alternates_is_one_card_in_the_shop_the_first_in_the_file()
    {
        var db = TestContent.Load(units: UnitsWithWedge());

        Assert.Contains(db.ShopUnits, u => u.Id == "sticky");
        Assert.DoesNotContain(db.ShopUnits, u => u.Id == "wedge");
        Assert.Equal("wedge", db.Unit("sticky").Alternate);
        Assert.Equal("sticky", db.Unit("wedge").Alternate);
    }

    [Fact]
    public void Without_alternates_the_shop_is_every_unit_in_the_shop()
    {
        var db = TestContent.Load();

        Assert.Equal(db.Units.Where(u => u.InShop).Select(u => u.Id), db.ShopUnits.Select(u => u.Id));
    }

    [Fact]
    public void A_triangle_is_a_shape()
    {
        var db = TestContent.Load(units: UnitsWithWedge());

        Assert.Equal("triangle", db.Unit("wedge").Shape);
    }

    [Fact]
    public void An_alternate_that_is_no_unit_is_refused()
    {
        var error = Assert.Throws<InvalidOperationException>(() => TestContent.Load(units: UnitsWithWedge(stickyAlternate: "brie")));

        Assert.Contains("units.json 'sticky': alternate 'brie' is not in units.json", error.Message);
    }

    [Fact]
    public void An_alternate_that_does_not_name_it_back_is_refused()
    {
        var wedge = Wedge.Replace("\"alternate\": \"sticky\",", "\"alternate\": \"wall\",");

        var error = Assert.Throws<InvalidOperationException>(() => TestContent.Load(units: UnitsWithWedge(wedge: wedge)));

        Assert.Contains("units.json 'sticky': its alternate 'wedge' must name 'sticky' back", error.Message);
    }

    [Fact]
    public void An_alternate_out_of_the_shop_is_refused()
    {
        var wedge = Wedge.Replace("\"inShop\": true", "\"inShop\": false");

        var error = Assert.Throws<InvalidOperationException>(() => TestContent.Load(units: UnitsWithWedge(wedge: wedge)));

        Assert.Contains("units.json 'wedge': an alternate must be in the shop", error.Message);
    }
}
