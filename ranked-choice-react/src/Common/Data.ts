export interface CandidateDTO {
    candidateId : string
    value : string
}

export interface ElectionDTO {
    electionId : string
    candidates : CandidateDTO[]
}

export enum ElectionState {
    New,
    Started,
    Finished
}

export interface ElectionSettingsDTO {
    electionId : string
    uniqueIdsPerUser : string
    uniqueIds : string[]
    userEmails : string[]
    state : ElectionState,
}

export interface VoteDTO {
    submitted : boolean
    candidates: CandidateDTO[]
}