import {ElectionDTO, ElectionSettingsDTO} from "./Data";

export function createElection(userId : string) : Promise<string>
{
    return fetch(`${process.env.REACT_APP_API}/election/`, {method:"POST"})
        .then(res => res.json().then(res => res as string))
}
export function getElectionCandidates(electionId : string, userId : string) : Promise<ElectionDTO>
{
    return fetch(`${process.env.REACT_APP_API}/election/${electionId}/candidates`)
        .then(res => res.json().then(res => res as ElectionDTO))
}
export function getElectionSettings(electionId : string, userId : string) : Promise<ElectionDTO>
{
    return fetch(`${process.env.REACT_APP_API}/election/${electionId}/settings`)
        .then(res => res.json().then(res => res as ElectionDTO))
}

export function saveElectionCandidates(electionId : string, userId : string, election: ElectionDTO)
{
    fetch(`${process.env.REACT_APP_API}/election/${electionId}/candidates`,
        {method: "POST", headers:{"Content-Type":"application/json"},
            body:JSON.stringify(election)})
}
export function saveElectionSettings(electionId : string, userId : string, settings: ElectionSettingsDTO)
{
    fetch(`${process.env.REACT_APP_API}/election/${electionId}/settings`,
        {method: "POST", headers:{"Content-Type":"application/json"},
            body:JSON.stringify(settings)})
}
export function getElectionResults(electionId : string, userId : string) : Promise<ElectionDTO>
{
    return fetch(`${process.env.REACT_APP_API}/election/${electionId}/results`)
        .then(res => res.json().then(res => res as ElectionDTO))
}
export function startElection(electionId : string, userId : string) : Promise<boolean>
{
    return fetch(`${process.env.REACT_APP_API}/election/${electionId}/start`)
        .then(res => res.json().then(res => res as boolean ))
}
export function endElection(electionId : string, userId : string) : Promise<boolean>
{
    return fetch(`${process.env.REACT_APP_API}/election/${electionId}/end`)
        .then(res => res.json().then(res => res as boolean ))
}
export function restartElection(electionId : string, userId : string) : Promise<boolean>
{
    return fetch(`${process.env.REACT_APP_API}/election/${electionId}/restart`)
        .then(res => res.json().then(res => res as boolean ))
}