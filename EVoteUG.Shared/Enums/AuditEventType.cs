namespace EVoteUG.Shared.Enums;

public enum AuditEventType
{
    Authentication = 1,
    BallotSubmission = 2,
    ElectionCreated = 3,
    ElectionStatusChanged = 4,
    CandidateRegistered = 5,
    CandidateStatusChanged = 6,
    ResultsPublished = 7,
    VoterRollImported = 8
}
