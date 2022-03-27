export interface Candidate {
    electionId : string
    candidateId : string
    value : string
}

export interface Election {
    electionId : string
    candidates : Candidate[]
}

export interface UserVote {
    submitted: boolean
    electionId : string
    userId : string
    choices : Candidate[]
}