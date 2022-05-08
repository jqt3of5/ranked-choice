namespace HelloWorld.Data
{
    public record VoteDTO(bool submitted, CandidateDTO [] candidateIds);
    
    public record VoteResponse(string message, bool success, object? response);
}