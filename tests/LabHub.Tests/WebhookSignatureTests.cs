using System.Security.Cryptography;
using System.Text;
using LabHub.Web.Security;
using Xunit;

namespace LabHub.Tests;

public class WebhookSignatureTests
{
    private const string Secret = "test-secret";

    private static string Sign(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return "sha256=" + Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }

    [Fact]
    public void ValidSignaturePasses()
    {
        var payload = """{"ref":"refs/heads/main"}""";

        Assert.True(WebhookSignature.IsValid(payload, Sign(payload, Secret), Secret));
    }

    [Fact]
    public void WrongSecretFails()
    {
        var payload = "{}";

        Assert.False(WebhookSignature.IsValid(payload, Sign(payload, "другой"), Secret));
    }

    [Fact]
    public void TamperedPayloadFails()
    {
        Assert.False(WebhookSignature.IsValid("{\"a\":2}", Sign("{\"a\":1}", Secret), Secret));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("sha1=abc")]
    [InlineData("sha256=не-hex")]
    [InlineData("sha256=")]
    public void MissingOrMalformedHeaderFails(string? header)
    {
        Assert.False(WebhookSignature.IsValid("{}", header, Secret));
    }

    [Fact]
    public void EmptySecretAlwaysFails()
    {
        var payload = "{}";

        Assert.False(WebhookSignature.IsValid(payload, Sign(payload, ""), ""));
    }
}
