import {CardData} from "../Components/Card";
import {CandidateDTO, ElectionDTO, ElectionSettingsDTO, VoteDTO} from "./Data";

export function getElectionCandidates(electionId : string, userId : string) : Promise<ElectionDTO>
{
    return fetch(`https://localhost:5001/election/${electionId}/candidates`)
        .then(res => res.json().then(res => res as ElectionDTO))
}
export function getElectionSettings(electionId : string, userId : string) : Promise<ElectionDTO>
{
    return fetch(`https://localhost:5001/election/${electionId}/settings`)
        .then(res => res.json().then(res => res as ElectionDTO))
}

export function saveElectionCandidates(electionId : string, userId : string, election: ElectionDTO)
{
    fetch(`https://localhost:5001/election/${electionId}/candidates`,
        {method: "POST", headers:{"Content-Type":"application/json"},
            body:JSON.stringify(election)})
}
export function saveElectionSettings(electionId : string, userId : string, settings: ElectionSettingsDTO)
{
    fetch(`https://localhost:5001/election/${electionId}/settings`,
        {method: "POST", headers:{"Content-Type":"application/json"},
            body:JSON.stringify(settings)})
}
export function getElectionResults(electionId : string, userId : string) : Promise<ElectionDTO>
{
    return fetch(`https://localhost:5001/election/${electionId}/results`)
        .then(res => res.json().then(res => res as ElectionDTO))
}
export function startElection(electionId : string, userId : string) : Promise<boolean>
{
    return fetch(`https://localhost:5001/election/${electionId}/start`)
        .then(res => res.json().then(res => res as boolean ))
}
export function endElection(electionId : string, userId : string) : Promise<boolean>
{
    return fetch(`https://localhost:5001/election/${electionId}/end`)
        .then(res => res.json().then(res => res as boolean ))
}
export function restartElection(electionId : string, userId : string) : Promise<boolean>
{
    return fetch(`https://localhost:5001/election/${electionId}/restart`)
        .then(res => res.json().then(res => res as boolean ))
}