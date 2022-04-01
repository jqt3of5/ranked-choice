import React, {useEffect, useReducer, useState} from 'react';
import {Column} from '../Components/Column';
import './VoteView.css'
import '../Common/common.css'
import { DndProvider } from 'react-dnd';
import {HTML5Backend} from "react-dnd-html5-backend";
import {CardTableActionType, CardTableState, card_table_reducer} from "./CardTableReducer";
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
    choices: CandidateDTO[]
}

export function VoteView() {
    var [tableState, dispatch] = useReducer(card_table_reducer, {table: [[],[]]})

    let params = useParams();

    if (params.electionId===undefined)
    {
        throw "electionID cannot be undefined"
    }
    let electionId = params.electionId as string

    var [{isReadOnly, candidates, choices}, setState] = useState<VoteViewState>({
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
        getVote(electionId, cookies.userId).then(
            (vote) => {
                    getElectionCandidates(electionId, cookies.userId).then(
                        (election) => {
                            setState(state => {
                                return {...state, isReadOnly:vote.submitted, candidates:election.candidates, choices:vote.candidates}
                            })

                            let candidateCardData = election.candidates.map(c => {return {id:c.candidateId, text: c.value}})
                            let voteCardData = vote.candidates.map(c => {return {id:c.candidateId, text: c.value}})

                            candidateCardData = candidateCardData.filter(card => voteCardData.find(c => c.id == card.id) == undefined)

                            //TODO: Diff candidate/choice lists
                            dispatch({type:CardTableActionType.SetCards, cards: [candidateCardData, voteCardData]})
                        },
                        (error) => {

                        })
                },
                (error) => {

                }
            )
    }, [cookies.userId])

    useEffect(() => {

        let cands: CandidateDTO[] = tableState.table[0].map(value => {return {electionId: electionId, candidateId: value.id, value: value.text}})
        let chois : CandidateDTO[] = tableState.table[1].map(value => {return {electionId: electionId, candidateId: value.id, value: value.text}})

        cands = cands.filter(card => chois.find(c => c.candidateId == card.candidateId) == undefined)

        let vote : VoteDTO = {submitted: false, candidates: chois}
        saveVote(electionId, cookies.userId, vote).then(
            (vote) => {
                setState(state => {
                    return {...state, choices:chois, candidates: cands}
                })
            },
            (error) => {}
        )
    }, [tableState.table])

    //TODO: If no election exists, show error

    return <div className={"vote-view"}>
        <CardTable>
           <Column canEdit={false} canReorder={true} name={"Candidates"} column={0} showRank={false} dispatch={dispatch}>
               {tableState.table[0].map((card, index) => {
                    return <Card key={"card" + card.id} card={card}
                                 index={index} column={0}
                                 canEdit={false} canReorder={true} canDelete={false}
                                 dispatch={dispatch}/>
               })}
           </Column>
            <div>
                <Column canEdit={false} canReorder={true} name={"Ranked Choices"} column={1} showRank={true} dispatch={dispatch}>
                    {tableState.table[1].map((card, index) => {
                        return <Card key={"card" + card.id} card={card}
                                     index={index} column={1}
                                     canEdit={false} canReorder={true} canDelete={true}
                                     dispatch={dispatch}/>
                    })}

                </Column>
                <div className={"box"}>
                    <button onClick={event => {
                        let vote : VoteDTO = {candidates: choices, submitted:true}
                        submitVote(electionId, cookies.userId, vote)
                        setState(state => {
                            return {...state, isReadOnly:true}
                        })
                    }}>Submit</button>
                </div>
            </div>
        </CardTable>
    </div>
}