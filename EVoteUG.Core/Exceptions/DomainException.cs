namespace EVoteUG.Core.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public class ResourceNotFoundException : DomainException
{
    public ResourceNotFoundException(string message) : base(message) { }
}

public class AlreadyVotedException : DomainException
{
    public AlreadyVotedException(string message = "You have already cast your ballot in this election.") : base(message) { }
}

public class ElectionClosedException : DomainException
{
    public ElectionClosedException(string message = "This election is not currently open for voting.") : base(message) { }
}

public class VoterNotEligibleException : DomainException
{
    public VoterNotEligibleException(string message = "You are not eligible to participate in this election.") : base(message) { }
}

public class UnauthorizedActionException : DomainException
{
    public UnauthorizedActionException(string message = "You do not have permission to perform this action.") : base(message) { }
}
