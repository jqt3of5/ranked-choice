namespace HelloWorld.Data
{
    public record VoteDTO(bool submitted, CandidateDTO[] candidates);
}