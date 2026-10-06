using Cms.Application.Common;

namespace Cms.Tests;

public class SlugTests
{
    [Theory]
    [InlineData("Hello, World!", "hello-world")]
    [InlineData("  Crème brûlée — recipe  ", "creme-brulee-recipe")]
    [InlineData("Ääkköset & Öljy", "aakkoset-oljy")]
    [InlineData("---", "")]
    [InlineData("", "")]
    public void From_normalizes_text(string input, string expected) =>
        Assert.Equal(expected, Slug.From(input));

    [Theory]
    [InlineData("good-slug-1", true)]
    [InlineData("Bad", false)]
    [InlineData("double--hyphen", false)]
    [InlineData("-leading", false)]
    [InlineData("", false)]
    public void IsValid_checks_format(string slug, bool valid) =>
        Assert.Equal(valid, Slug.IsValid(slug));

    [Fact]
    public void From_truncates_to_max_length() =>
        Assert.True(Slug.From(new string('a', 500)).Length <= Slug.MaxLength);
}
