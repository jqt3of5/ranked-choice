import {CardData} from "../Components/Card";
import {Candidate, Election, UserVote} from "./Data";

export function getElection(electionId : string, userId : string) : Promise<Election>
{
    return fetch(`https://localhost:5001/election/${electionId}`)
        .then(res => res.json().then(res => res as Election))
}

export function saveCandidates(electionId : string, userId : string, candidates: Candidate[])
{
    let election : Election = {electionId: electionId, candidates:candidates}

    fetch(`https://localhost:5001/election/${electionId}`,
        {method: "POST", headers:{"Content-Type":"application/json"},
            body:JSON.stringify(election)})
}

export function getVote(electionId: string, userId : string) : Promise<UserVote>
{
    return fetch(`https://localhost:5001/vote/${electionId}/${userId}`)
        .then(res => res.json().then(result => result as UserVote))
}

export function saveVote(electionId:string, userId: string, choices : Candidate[]) : Promise<UserVote>
{
    let election : UserVote = {electionId: electionId,userId:userId, choices:choices, submitted:false}

    return fetch(`https://localhost:5001/vote/${electionId}`,
        {method: "POST", headers:{"Content-Type":"application/json"},
            body:JSON.stringify(election)})
        .then(res => res.json().then(res => res as UserVote))
}

export function submitVote(electionId:string, userId: string, choices: Candidate[])
{
    let election : UserVote = {electionId: electionId,userId:userId, choices:choices, submitted: true}

    fetch(`https://localhost:5001/vote/${electionId}/vote`,
        {method: "POST", headers:{"Content-Type":"application/json"},
            body:JSON.stringify(election)})
}