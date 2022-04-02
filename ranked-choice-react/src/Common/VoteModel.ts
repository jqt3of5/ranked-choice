import {CandidateDTO, VoteDTO} from "./Data";

export function submitVote(electionId : string, userId : string, vote : VoteDTO) : Promise<boolean>
{
    console.log(vote)
    return fetch(`${process.env.REACT_APP_API}/election/${electionId}/vote`, {
        method: 'POST',
        headers:{"Content-Type":"application/json", "userId":userId},
        body:JSON.stringify(vote.candidates.map(c => c.candidateId))
    }).then(res => res.json().then(res => {
            if (res as boolean)
            {
                return fetch(`${process.env.REACT_APP_API}/vote/${electionId}/submit`, {
                    headers:{"userId":userId}
                }).then(res => res.json())
            }
            return false
        }))
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