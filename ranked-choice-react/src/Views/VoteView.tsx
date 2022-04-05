import React, {useEffect, useReducer, useState} from 'react';
import {Column} from '../Components/Column';
import './VoteView.css'
import '../Common/common.css'
import {CardTableActionType, card_table_reducer} from "./CardTableReducer";
import {CandidateDTO, VoteDTO} from "../Common/Data";
import {useCookies} from "react-cookie";
import {v4} from "uuid";
import {useParams} from "react-router-dom";
import {getVote, saveVote} from "../Common/VoteModel";
import {getElectionCandidates} from "../Common/ElectionModel";
import {CardTable} from "../Components/Table";
import {Card} from "../Components/Card";

interface VoteViewState
{
    isReadOnly : boolean
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

                            candidateCardData = candidateCardData.filter(card => voteCardData.find(c => c.id === card.id) === undefined)

                            setState(state => {return {...state, isReadOnly: vote.submitted}})
                            dispatch({type:CardTableActionType.SetCards, cards: [candidateCardData, voteCardData]})
                        },
                        (error) => {
                            console.log(error)
                        })
                },
                (error) => {
                    console.log(error)
                }
            )
    }, [cookies.userId, electionId])

    useEffect(() => {

        let cands: CandidateDTO[] = tableState.table[0].map(value => {return {electionId: electionId, candidateId: value.id, value: value.text}})
        let chois : CandidateDTO[] = tableState.table[1].map(value => {return {electionId: electionId, candidateId: value.id, value: value.text}})

        cands = cands.filter(card => chois.find(c => c.candidateId === card.candidateId) === undefined)

        let vote : VoteDTO = {submitted: false, candidates: chois}
        saveVote(electionId, cookies.userId, vote).then(
            (vote) => {
                if (vote)
                {
                    setState(state => {
                        return {...state, choices:chois, candidates: cands}
                    })
                }
            },
            (error) => {
                console.log(error)
            }
        )
    }, [tableState.table, cookies.userId, electionId])

    //TODO: If no election exists, show error

    return <div className={"vote-view"}>
        <div className={"vote-view-header primary"}>
            <button>Submit Vote</button>
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