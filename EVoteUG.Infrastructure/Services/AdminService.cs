using System.Globalization;
using System.Security.Cryptography;
using EVoteUG.Core.DTOs.Admin;
using EVoteUG.Core.Interfaces;
using EVoteUG.Infrastructure.Data;
using EVoteUG.Infrastructure.Security;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Models;
using EVoteUG.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace EVoteUG.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly EVoteUGDbContext _context;
    private readonly IAuditService _auditService;

    public AdminService(EVoteUGDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<ApiResponse<DashboardSummaryDto>> GetDashboardSummaryAsync()
    {
        var totalStudents = await _context.Students.CountAsync();
        var totalElections = await _context.Elections.CountAsync();
        var activeElections = await _context.Elections.CountAsync(e => e.Status == ElectionStatus.Active);
        var totalBallotsCast = await _context.VoterParticipations.CountAsync();
        var totalApprovedCandidates = await _context.Candidates.CountAsync(c => c.Status == CandidateStatus.Approved);

        var summary = new DashboardSummaryDto
        {
            TotalStudentsRegistered = totalStudents,
            TotalElections = totalElections,
            ActiveElections = activeElections,
            TotalBallotsCastAllTime = totalBallotsCast,
            TotalApprovedCandidates = totalApprovedCandidates
        };

        return ApiResponse<DashboardSummaryDto>.Ok(summary, "Dashboard metrics retrieved successfully.");
    }

    public async Task<ApiResponse<VoterImportResultDto>> ImportVoterRollCsvAsync(Stream csvStream, int adminId)
    {
        if (csvStream == null || csvStream.Length == 0)
            return ApiResponse<VoterImportResultDto>.Fail("CSV file stream is empty.");

        var result = new VoterImportResultDto();
        var studentsToInsert = new List<Student>();

        // Cache existing StudentIds and Emails in memory to avoid duplicate key conflicts
        var existingStudentIds = (await _context.Students
            .Select(s => s.StudentId)
            .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingEmails = (await _context.Students
            .Select(s => s.Email)
            .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        using var reader = new StreamReader(csvStream);
        var headerLine = await reader.ReadLineAsync();

        if (string.IsNullOrWhiteSpace(headerLine))
            return ApiResponse<VoterImportResultDto>.Fail("CSV file has no header row.");

        var lineNumber = 1;

        while (!reader.EndOfStream)
        {
            lineNumber++;
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            result.TotalRecordsProcessed++;

            var columns = ParseCsvLine(line);
            if (columns.Length < 3)
            {
                result.TotalSkipped++;
                result.Errors.Add($"Line {lineNumber}: Insufficient columns. Minimum required: StudentId, FullName, Email.");
                continue;
            }

            var studentId = columns[0].Trim();
            var fullName = columns[1].Trim();
            var email = columns[2].Trim();
            var college = columns.Length > 3 ? columns[3].Trim() : string.Empty;
            var faculty = columns.Length > 4 ? columns[4].Trim() : string.Empty;
            var department = columns.Length > 5 ? columns[5].Trim() : string.Empty;
            var hallOfResidence = columns.Length > 6 ? columns[6].Trim() : string.Empty;
            var levelStr = columns.Length > 7 ? columns[7].Trim() : "100";

            if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
            {
                result.TotalSkipped++;
                result.Errors.Add($"Line {lineNumber}: StudentId, FullName, and Email cannot be empty.");
                continue;
            }

            if (existingStudentIds.Contains(studentId))
            {
                result.TotalSkipped++;
                result.Errors.Add($"Line {lineNumber}: Student ID '{studentId}' already exists in registry. Skipped.");
                continue;
            }

            if (existingEmails.Contains(email))
            {
                result.TotalSkipped++;
                result.Errors.Add($"Line {lineNumber}: Email '{email}' already registered. Skipped.");
                continue;
            }

            int.TryParse(levelStr, out var level);
            if (level <= 0) level = 100;

            // Generate initial secure student password (e.g. Student@ + last 4 digits or random PIN)
            var initialPassword = studentId.Length >= 4
                ? $"Student@{studentId[^4..]}"
                : "Student@1234";

            var student = new Student
            {
                StudentId = studentId,
                FullName = fullName,
                Email = email,
                PasswordHash = PasswordHasher.HashPassword(initialPassword),
                College = college,
                Faculty = faculty,
                Department = department,
                HallOfResidence = hallOfResidence,
                Level = level,
                IsVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            studentsToInsert.Add(student);
            existingStudentIds.Add(studentId);
            existingEmails.Add(email);
            result.TotalImported++;
        }

        if (studentsToInsert.Count > 0)
        {
            await _context.Students.AddRangeAsync(studentsToInsert);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                adminId,
                AuditEventType.VoterRollImported,
                $"Imported {result.TotalImported} student voter records via CSV",
                "Student",
                null,
                new { TotalImported = result.TotalImported, TotalSkipped = result.TotalSkipped });
        }

        return ApiResponse<VoterImportResultDto>.Ok(
            result, 
            $"Voter roll import completed. {result.TotalImported} imported, {result.TotalSkipped} skipped.");
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var inQuotes = false;
        var currentField = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField.ToString().Trim());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        result.Add(currentField.ToString().Trim());
        return result.ToArray();
    }
}
