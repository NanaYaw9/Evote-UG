namespace EVoteUG.Shared.Models;

/// <summary>
/// Cryptographic proof given to the voter confirming participation.
/// </summary>
public class VoteReceipt
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public int ElectionId { get; set; }
    public Election? Election { get; set; }
    public string ReceiptHash { get; set; } = string.Empty; // SHA-256 digital receipt
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
}
