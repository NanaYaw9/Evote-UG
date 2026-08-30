namespace EVoteUG.Shared.Models;

public class Student
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;   // University of Ghana Student ID (e.g., 10987654)
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;       // University student webmail (@st.ug.edu.gh)
    public string PasswordHash { get; set; } = string.Empty;
    public string College { get; set; } = string.Empty;     // e.g. College of Basic and Applied Sciences
    public string Faculty { get; set; } = string.Empty;     // e.g. Faculty of Science
    public string Department { get; set; } = string.Empty;  // e.g. Department of Computer Science
    public string HallOfResidence { get; set; } = string.Empty; // e.g. Commonwealth Hall, Volta Hall
    public int Level { get; set; } = 100;                   // 100, 200, 300, 400, Postgrad
    public bool IsVerified { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public List<VoterParticipation> Participations { get; set; } = new();
    public List<VoteReceipt> Receipts { get; set; } = new();
}
