export interface Candidate {
    electionId : string
    candidateId : string
    value : string
}

export interface UserVote {
    electionId : string
    userId : string
    choices : Candidate[]
}