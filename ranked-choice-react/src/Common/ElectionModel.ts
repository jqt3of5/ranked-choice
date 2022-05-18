import {ElectionDTO, ElectionResponse, ElectionSettingsDTO} from "./Data";

export async function createElection(userId : string) : Promise<ElectionResponse<string>>
{
    var response = await fetch(`${process.env.REACT_APP_API}/election`, {method:"POST", headers:{"userId":userId}})
    return await response.json()
}
export async function getElectionCandidates(electionId : string, userId : string) : Promise<ElectionResponse<ElectionDTO>>
{
    var response = await fetch(`${process.env.REACT_APP_API}/election/${electionId}/candidates`, { headers:{"userId":userId}})
    return await response.json()
}
export async function getElectionSettings(electionId : string, userId : string) : Promise<ElectionResponse<ElectionDTO>>
{
    var response = await fetch(`${process.env.REACT_APP_API}/election/${electionId}/settings`,{ headers:{"userId":userId}})
    return response.json()
}

export async function saveElectionCandidates(electionId : string, userId : string, election: ElectionDTO)
{
    await fetch(`${process.env.REACT_APP_API}/election/${electionId}/candidates`,
        {method: "POST", headers:{"Content-Type":"application/json", "userId":userId},
            body:JSON.stringify(election)})
}
export async function saveElectionSettings(electionId : string, userId : string, settings: ElectionSettingsDTO)
{
    await fetch(`${process.env.REACT_APP_API}/election/${electionId}/settings`,
        {method: "POST", headers:{"Content-Type":"application/json", "userId":userId},
            body:JSON.stringify(settings)})
}
export async function getElectionResults(electionId : string, userId : string) : Promise<ElectionResponse<ElectionDTO>>
{
    var response = await fetch(`${process.env.REACT_APP_API}/election/${electionId}/results`, {headers:{"userId":userId}})
    return await response.json()
}
export async function startElection(electionId : string, userId : string) : Promise<ElectionResponse<boolean>>
{
    var response = await fetch(`${process.env.REACT_APP_API}/election/${electionId}/start`, {method:"POST", headers:{"userId":userId}})
    return await response.json()
}
export async function endElection(electionId : string, userId : string) : Promise<ElectionResponse<boolean>>
{
    var response = await fetch(`${process.env.REACT_APP_API}/election/${electionId}/end`,{method:"POST",headers:{"userId":userId}})
    return await response.json()
}
export async function restartElection(electionId : string, userId : string) : Promise<ElectionResponse<boolean>>
{
    var response = await fetch(`${process.env.REACT_APP_API}/election/${electionId}/restart`,{method:"POST",headers:{"userId":userId}})
    return await response.json()
}