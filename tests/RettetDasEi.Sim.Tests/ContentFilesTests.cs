using RettetDasEi.Sim;

namespace RettetDasEi.Sim.Tests;

/// <summary>The shipped files of game/content/ (copied next to the tests at build): a broken edit fails here.</summary>
public class ContentFilesTests
{
    static readonly string Dir = Path.Combine(AppContext.BaseDirectory, "content");

    [Fact]
    public void The_shipped_content_loads()
    {
        var db = ContentDb.Load(name => File.ReadAllText(Path.Combine(Dir, name)));

        Assert.NotEmpty(db.Units);
        Assert.NotEmpty(db.Enemies);
        Assert.NotEmpty(db.Waves);
    }

    [Fact]
    public void Every_json_file_in_the_content_folder_is_read_by_the_loader()
    {
        var onDisk = Directory.GetFiles(Dir, "*.json").Select(Path.GetFileName).OrderBy(n => n);

        Assert.Equal(ContentDb.Files.OrderBy(n => n), onDisk);
    }
}
