using System;
using System.Collections.Generic;
using System.Linq;

using VersionApi.Controllers;

namespace VersionApi.Test;

public class CodeVersionTagTest
{
    [TestCase("refs/tags/v1.3.99", 0, 0, 0)]
    [TestCase("refs/tags/whatever", 0, 0, 0)]
    [TestCase("refs/tags/1.3.99", 1, 3, 99)]
    [TestCase("refs/tags/0.1.10-20201204", 0, 1, 10)]
    public void ThatParsingWorks(string version, int major, int minor, int patch)
    {
        var sut = new CodeVersion { name = version };
        var actual = sut.GetVersion();
        Assert.That(actual.Major, Is.EqualTo(major));
        Assert.That(actual.Minor, Is.EqualTo(minor));
        Assert.That(actual.Build, Is.EqualTo(patch));
    }

    [Test]
    public void ThatFindingLatestWorks()
    {
        var sut = new List<CodeVersion>
        {
            new() { name = "refs/tags/1.2.110" },
            new() { name = "refs/tags/1.3.99" },
            new() { name = "refs/tags/0.1.10-20201204" },
            new() { name = "refs/tags/1.3.98" },
        };
        var actual = sut.Max(o => o.GetVersion());
        Assert.Multiple(() =>
        {
            Assert.That(actual.Major, Is.EqualTo(1));
            Assert.That(actual.Minor, Is.EqualTo(3));
            Assert.That(actual.Build, Is.EqualTo(99));
        });
    }
}