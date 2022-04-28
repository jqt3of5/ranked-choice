namespace HelloWorld.Data
{
    public enum ElectionState
    {
        New, Started, Finished
    }
    public record CandidateDTO(string value, string candidateId);
    public record ElectionDTO(string electionId, CandidateDTO[] candidates);
    public record ElectionSettingsDTO(string electionId, bool uniqueIdsPerUser, 
        string [] userEmails, string electionName, ElectionState state);

    public record ElectionResponse(string message, bool success, object? response);
}