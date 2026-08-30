using EVoteUG.Shared.Enums;

namespace EVoteUG.Core.DTOs.Election;

public class UpdateElectionStatusDto
{
    public ElectionStatus NewStatus { get; set; }
}
