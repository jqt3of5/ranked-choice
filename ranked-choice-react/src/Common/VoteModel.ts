import {VoteDTO, VoteResponse} from "./Data";

export async function submitVote(electionId : string, userId : string, vote : VoteDTO) : Promise<VoteResponse<boolean>>
{
    console.log(vote)
    var response = await fetch(`${process.env.REACT_APP_API}/vote/${electionId}/submit`, {
        headers:{"userId":userId}
    })
    return await response.json()
}

export function getVote(electionId: string, userId : string) : Promise<VoteDTO>
{
    return fetch(`${process.env.REACT_APP_API}/vote/${electionId}`, {headers:{"userId":userId}})
        .then(res => res.json().then(result => result as VoteDTO))
}

export function saveVote(electionId:string, userId: string, vote : VoteDTO) : Promise<boolean>
{
    return fetch(`${process.env.REACT_APP_API}/vote/${electionId}`,
        {method: "POST", headers:{"Content-Type":"application/json", "userId":userId},
            body:JSON.stringify(vote)})
        .then(res => res.json().then(res => res as boolean))
}