using System.Security.Cryptography;
using System.Text;

namespace EVoteUG.Infrastructure.Security;

public static class ReceiptHasher
{
    public static string GenerateReceiptHash(int studentId, int electionId, DateTime timestamp)
    {
        var rawData = $"{studentId}:{electionId}:{timestamp.Ticks}:{Guid.NewGuid():N}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(hashBytes).ToUpperInvariant();
    }
}
