namespace EVoteUG.Shared.Models;

public class Student
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;   // university ID number
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // To know that a student can have votes
    public List<Vote> Votes { get; set; } = new();
}