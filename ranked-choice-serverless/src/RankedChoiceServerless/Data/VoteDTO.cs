namespace HelloWorld.Data
{
    public record VoteDTO(bool submitted, CandidateDTO [] candidates);
    
    public record VoteResponse(string message, bool success, object? response);
}