using Vdcrpt.Next;

namespace Vdcrpt.Next.Tests;

public class EffectContextTests : IDisposable
{
    private string _testDir;
    private EffectContext _ctx;

    public EffectContextTests()
    {
        _testDir = $"test_{Guid.NewGuid()}";

        var cacheDir = Path.Combine(_testDir, "cache");
        var scratchDir = Path.Combine(_testDir, "scratch");

        Directory.CreateDirectory(cacheDir);
        Directory.CreateDirectory(scratchDir);

        _ctx = new EffectContext(cacheDir, scratchDir);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Directory.Delete(_testDir, true);
    }

    [Fact]
    public void GetScratchFile_WithoutExtension_ReturnsUniqueFile()
    {
        Assert.NotEqual(_ctx.GetScratchFile(), _ctx.GetScratchFile());
    }

    [Fact]
    public void GetScratchFile_WithoutExtension_HasNoExtension()
    {
        Assert.Equal(string.Empty, Path.GetExtension(_ctx.GetScratchFile()));
    }

    [Fact]
    public void GetScratchFile_WithExtension_ReturnsUniqueFile()
    {
        Assert.NotEqual(_ctx.GetScratchFile("txt"), _ctx.GetScratchFile("txt"));
    }

    [Fact]
    public void GetScratchFile_WithExtension_ContainsExtension()
    {
        Assert.Equal(".txt", Path.GetExtension(_ctx.GetScratchFile("txt")));
    }

    [Fact]
    public void GetCacheFile_SingleKey_ReturnsSamePathForSameKey()
    {
        var (path1, _) = _ctx.GetCachedFile("asdf");
        var (path2, _) = _ctx.GetCachedFile("asdf");

        Assert.Equal(path1, path2);
    }

    [Fact]
    public void GetCacheFile_SingleKey_ReturnsDifferentPathForDifferentKey()
    {
        var (path1, _) = _ctx.GetCachedFile("asdf");
        var (path2, _) = _ctx.GetCachedFile("qwer");

        Assert.NotEqual(path1, path2);
    }

    [Fact]
    public void GetCacheFile_SingleKey_DoesNotExistUntilWritten()
    {
        var (path, exists1) = _ctx.GetCachedFile("asdf");
        Assert.False(exists1);

        var (_, exists2) = _ctx.GetCachedFile("asdf");
        Assert.False(exists2);

        File.WriteAllText(path, "hello");

        var (_, exists3) = _ctx.GetCachedFile("asdf");
        Assert.True(exists3);
    }

    [Fact]
    public void GetCacheFile_SingleKey_DoesNotAcceptEmptyKey()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            _ctx.GetCachedFile("");
        });
    }

    [Fact]
    public void GetCacheFile_SingleKey_RemovesIllegalCharacters()
    {
        var (path, _) = _ctx.GetCachedFile("/\\$@\"!.,+");

        // Should not throw exception
        File.WriteAllText(path, "hello");
    }

    [Fact]
    public void GetCacheFile_CompoundKey_ReturnsSamePathForSameKey()
    {
        var (path1, _) = _ctx.GetCachedFile(new string[] { "a", "b" });
        var (path2, _) = _ctx.GetCachedFile(new string[] { "a", "b" });

        Assert.Equal(path1, path2);
    }

    [Fact]
    public void GetCacheFile_CompoundKey_ReturnsDifferentPathForDifferentKey()
    {
        var (path1, _) = _ctx.GetCachedFile(new string[] { "a", "b" });
        var (path2, _) = _ctx.GetCachedFile(new string[] { "a", "c" });
        var (path3, _) = _ctx.GetCachedFile(new string[] { "b", "c" });
        var (path4, _) = _ctx.GetCachedFile(new string[] { "a" });

        Assert.True(new HashSet<string>() { path1, path2, path3, path4 }.Count == 4);
    }

    [Fact]
    public void GetCacheFile_CompoundKey_DoesNotExistUntilWritten()
    {
        var (path, exists1) = _ctx.GetCachedFile(new string[] { "a", "b" });
        Assert.False(exists1);

        var (_, exists2) = _ctx.GetCachedFile(new string[] { "a", "b" });
        Assert.False(exists2);

        File.WriteAllText(path, "hello");

        var (_, exists3) = _ctx.GetCachedFile(new string[] { "a", "b" });
        Assert.True(exists3);
    }

    [Fact]
    public void GetCacheFile_CompoundKey_DoesNotAcceptNullKey()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            #pragma warning disable 8600, 8625
            _ctx.GetCachedFile((string[])null);
            #pragma warning restore 8600, 8625
        });
    }

    [Fact]
    public void GetCacheFile_CompoundKey_DoesNotAcceptEmptyKeyComponents()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            _ctx.GetCachedFile(new string[] { "a", "" });
        });
    }

    [Fact]
    public void GetCacheFile_CompoundKey_RemovesIllegalCharacters()
    {
        var (path, _) = _ctx.GetCachedFile(new string[] { "/\\$@\"!.,+", "a", "/\\$@\"!.,+" });

        // Should not throw exception
        File.WriteAllText(path, "hello");
    }
}
