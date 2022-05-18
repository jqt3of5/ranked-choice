import React, {useEffect, useReducer, useState} from 'react';
import {Column} from '../Components/Column';
import './VoteView.css'
import '../Common/common.css'
import {CardTableActionType, card_table_reducer} from "./CardTableReducer";
import {CandidateDTO, VoteDTO} from "../Common/Data";
import {useCookies} from "react-cookie";
import {v4} from "uuid";
import {useParams} from "react-router-dom";
import {getVote, saveVote, submitVote} from "../Common/VoteModel";
import {getElectionCandidates} from "../Common/ElectionModel";
import {CardTable} from "../Components/Table";
import {Card} from "../Components/Card";

interface VoteViewState
{
    isReadOnly : boolean
    candidates: CandidateDTO[]
    choices : CandidateDTO[]
}

export function VoteView() {
    var [tableState, dispatch] = useReducer(card_table_reducer, {table: [[],[]]})

    let params = useParams();

    if (params.electionId===undefined)
    {
        throw new Error("electionID cannot be undefined")
    }
    let electionId = params.electionId as string

    var [{isReadOnly}, setState] = useState<VoteViewState>({
        isReadOnly: false,
        candidates: [],
        choices: []
    })

    const [cookies, setCookie] = useCookies(['userId'])
    if (cookies.userId===undefined)
    {
        setCookie('userId', v4())
    }

    useEffect(() => {
        const fetchData = async () => {
            var voteResponse = await getVote(electionId, cookies.userId)
            var electionResponse = await getElectionCandidates(electionId, cookies.userId)

            if (electionResponse.response != null && voteResponse.response != null)
            {
                let candidateCardData = electionResponse.response.candidates.map(c => {return {id:c.candidateId, text: c.value}})
                let voteCardData = voteResponse.response.candidates.map(c => {return {id:c.candidateId, text: c.value}})

                candidateCardData = candidateCardData.filter(card => voteCardData.find(c => c.id === card.id) === undefined)

                setState(state => {
                    return {...state,
                        isReadOnly:voteResponse?.response?.submitted ?? false,
                        candidates: electionResponse?.response?.candidates ?? [],
                        choices:voteResponse?.response?.candidates ?? [] }
                })

                dispatch({type:CardTableActionType.SetCards, cards: [candidateCardData, voteCardData]})
            }
        }

        fetchData().catch(e => console.log(e))

    }, [cookies.userId, electionId])

    useEffect(() => {

        const saveData = async () => {
            let cands: CandidateDTO[] = tableState.table[0].map(value => {return {electionId: electionId, candidateId: value.id, value: value.text}})
            let chois : CandidateDTO[] = tableState.table[1].map(value => {return {electionId: electionId, candidateId: value.id, value: value.text}})

            cands = cands.filter(card => chois.find(c => c.candidateId === card.candidateId) === undefined)

            let voteDTO : VoteDTO = {submitted: false, candidates: chois}
            var vote = await saveVote(electionId, cookies.userId, voteDTO)
            if (vote.success)
            {
                setState(state => {
                    return {...state, choices:chois, candidates: cands}
                })
            }
        }

        saveData().catch(e => console.log(e))

    }, [tableState.table, cookies.userId, electionId])

    //TODO: If no election exists, show error

    return <div className={"vote-view"}>
        <div className={"vote-view-header primary"}>
            <button onClick={() => submitVote(electionId, cookies.userId)}>Submit Vote</button>
        </div>
        <CardTable>
           <Column canEdit={false} canReorder={true} name={"Candidates"} column={0} showRank={false} dispatch={dispatch}>
               {tableState.table[0].map((card, index) => {
                    return <Card key={"card" + card.id} card={card}
                                 index={index} column={0}
                                 canEdit={false} canReorder={!isReadOnly} canDelete={false}
                                 dispatch={dispatch}/>
               })}
           </Column>
            <Column canEdit={false} canReorder={true} name={"Ranked Choices"} column={1} showRank={true} dispatch={dispatch}>
                {tableState.table[1].map((card, index) => {
                    return <Card key={"card" + card.id} card={card}
                                 index={index} column={1}
                                 canEdit={false} canReorder={!isReadOnly} canDelete={!isReadOnly}
                                 dispatch={dispatch}/>
                })}
            </Column>
        </CardTable>
    </div>
}