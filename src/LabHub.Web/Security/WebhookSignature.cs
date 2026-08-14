using System.Security.Cryptography;
using System.Text;

namespace LabHub.Web.Security;

public static class WebhookSignature
{
    private const string Prefix = "sha256=";

    public static bool IsValid(string payload, string? header, string? secret)
    {
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(header)) return false;
        if (!header.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)) return false;
        if (!TryFromHex(header[Prefix.Length..], out var given)) return false;
        var expected = Compute(payload, secret);
        return CryptographicOperations.FixedTimeEquals(given, expected);
    }

    private static byte[] Compute(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
    }

    private static bool TryFromHex(string hex, out byte[] bytes)
    {
        try
        {
            bytes = Convert.FromHexString(hex);
            return true;
        }
        catch (FormatException)
        {
            bytes = [];
            return false;
        }
    }
}
