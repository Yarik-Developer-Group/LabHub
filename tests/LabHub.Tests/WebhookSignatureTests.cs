using System.Security.Cryptography;
using System.Text;
using LabHub.Web.Security;

namespace LabHub.Tests;

public class WebhookSignatureTests
{
    private const string Secret = "test-secret";

    private static string Sign(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return "sha256=" + Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }

    [Test]
    public void ValidSignaturePasses()
    {
        var payload = """{"ref":"refs/heads/main"}""";
        Check.True(WebhookSignature.IsValid(payload, Sign(payload, Secret), Secret), "valid signature");
    }

    [Test]
    public void WrongSecretFails()
    {
        var payload = "{}";
        Check.False(WebhookSignature.IsValid(payload, Sign(payload, "другой"), Secret), "wrong secret");
    }

    [Test]
    public void TamperedPayloadFails()
    {
        Check.False(WebhookSignature.IsValid("{\"a\":2}", Sign("{\"a\":1}", Secret), Secret), "tampered payload");
    }

    [Test]
    public void MissingOrMalformedHeaderFails()
    {
        Check.False(WebhookSignature.IsValid("{}", null, Secret), "null header");
        Check.False(WebhookSignature.IsValid("{}", "", Secret), "empty header");
        Check.False(WebhookSignature.IsValid("{}", "sha1=abc", Secret), "wrong algo prefix");
        Check.False(WebhookSignature.IsValid("{}", "sha256=не-hex", Secret), "not hex");
    }

    [Test]
    public void EmptySecretAlwaysFails()
    {
        var payload = "{}";
        Check.False(WebhookSignature.IsValid(payload, Sign(payload, ""), ""), "empty secret");
    }
}
