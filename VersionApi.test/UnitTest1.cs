using NUnit.Framework;
using VersionApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace VersionApi.test;

public class VersionApiControllerTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var versionapiC = new VersionApiControllers();
        versionapiC.SetInformation("Disney", "Hund", "Dalmatiner", "1.0.1");
        var result = versionapiC.GetInformation("Disney", "Hund", "Dalmatiner");

        var actualShield = GetActualShield(result);

        ShieldsIo expectedShield = new ShieldsIo("Version", "1.0.1");

        Assert.NotNull(actualShield);
        Assert.That(actualShield!.label, Is.EqualTo(expectedShield.label));
        Assert.That(actualShield.message, Is.EqualTo(expectedShield.message));

    }

    private static ShieldsIo? GetActualShield(ActionResult<ShieldsIo> result) => (result.Result as OkObjectResult)!.Value as ShieldsIo;
    
}