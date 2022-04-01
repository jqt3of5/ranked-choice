import {CandidateDTO, VoteDTO} from "./Data";

export function submitVote(electionId : string, userId : string, vote : VoteDTO) : Promise<boolean>
{
    console.log(vote)
    return fetch(`https://localhost:5001/election/${electionId}/vote`, {
        method: 'POST',
        headers:{"Content-Type":"application/json", "userId":userId},
        body:JSON.stringify(vote.candidates)
    })
        .then(res => res.json().then(res => {
            if (res as boolean)
            {
                return fetch(`https://localhost:5001/vote/${electionId}/submit`)
                    .then(res => res.json())
            }
            return false
        }))
}

export function getVote(electionId: string, userId : string) : Promise<VoteDTO>
{
    return fetch(`https://localhost:5001/vote/${electionId}`, {headers:{"userId":userId}})
        .then(res => res.json().then(result => result as VoteDTO))
}

export function saveVote(electionId:string, userId: string, vote : VoteDTO) : Promise<boolean>
{
    return fetch(`https://localhost:5001/vote/${electionId}`,
        {method: "POST", headers:{"Content-Type":"application/json", "userId":userId},
            body:JSON.stringify(vote)})
        .then(res => res.json().then(res => res as boolean))
}