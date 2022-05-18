import {VoteDTO, VoteResponse} from "./Data";

export async function submitVote(electionId : string, userId : string) : Promise<VoteResponse<boolean>>
{
    var response = await fetch(`${process.env.REACT_APP_API}/vote/${electionId}/submit`, {
        method:"POST",
        headers:{"userId":userId}
    })
    return await response.json()
}

export async function getVote(electionId: string, userId : string) : Promise<VoteResponse<VoteDTO>>
{
    var response = await fetch(`${process.env.REACT_APP_API}/vote/${electionId}`, {headers:{"userId":userId}})
    return await response.json()
}

export async function saveVote(electionId:string, userId: string, vote : VoteDTO) : Promise<VoteResponse<boolean>>
{
    var response = await fetch(`${process.env.REACT_APP_API}/vote/${electionId}`,
        {
            method: "POST",
            headers:{"Content-Type":"application/json", "userId":userId},
            body:JSON.stringify(vote.candidates.map(c => c.candidateId))
        }
    )
    return await response.json()
}
