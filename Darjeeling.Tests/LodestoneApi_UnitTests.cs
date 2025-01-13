using Darjeeling.Helpers.LodestoneHelpers;
using Moq;

namespace Darjeeling.Tests;

[TestFixture]
public class LodestoneApi_UnitTests
{
    private LodestoneApi _lodestoneApi;
    private Mock<HttpClient> _mockHttpClient;
    
    [SetUp]
    public void Setup()
    {
        _mockHttpClient = new Mock<HttpClient>();
        _lodestoneApi = new LodestoneApi();
    }

    [Test]
    public void Should_GenerateCharacterSearchQueryURL_()
    {
        Assert.Pass();
    }
}