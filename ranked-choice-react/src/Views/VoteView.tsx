import React, {useEffect, useReducer, useState} from 'react';
import {Column} from '../Components/Column';
import './VoteView.css'
import '../Common/common.css'
import { DndProvider } from 'react-dnd';
import {HTML5Backend} from "react-dnd-html5-backend";
import {CardTableActionType, CardTableState, card_table_reducer} from "./CardTableReducer";
import {getElection, getVote, saveVote, submitVote} from "../Common/ElectionModel";
import {Candidate, UserVote} from "../Common/Data";
import {useCookies} from "react-cookie";
import {v4} from "uuid";
import {useParams} from "react-router-dom";

interface VoteViewState
{
    isReadOnly : boolean
    candidates: Candidate[]
    choices: Candidate[]
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
                    getElection(electionId, cookies.userId).then(
                        (election) => {
                            setState(state => {
                                return {...state, isReadOnly:vote.submitted, candidates:election.candidates, choices:vote.choices}
                            })

                            let candidateCardData = election.candidates.map(c => {return {id:c.candidateId, text: c.value}})
                            let voteCardData = vote.choices.map(c => {return {id:c.candidateId, text: c.value}})

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

        let cands: Candidate[] = tableState.table[0].map(value => {return {electionId: electionId, candidateId: value.id, value: value.text}})
        let chois : Candidate[] = tableState.table[1].map(value => {return {electionId: electionId, candidateId: value.id, value: value.text}})

        saveVote(electionId, cookies.userId, chois).then(
            (vote) => {
                setState(state => {
                    return {...state, choices:vote.choices, candidates: cands}
                })
            },
            (error) => {}
        )
    }, [tableState.table])

    //TODO: If no election exists, show error

    return <div className={"vote-view"}>
        <DndProvider backend={HTML5Backend}>
            <div className={"vote-view-table"}>
                <Column key={"candidates"}  name={"Candidates"} column={0}
                        canReorder={!isReadOnly} canEdit={false}
                        showRank={false} cards={tableState.table[0]}
                        dispatch={dispatch}/>
                <div>
                    <Column key={"ranked_choice"} name={"Ranked Choice"} column={1}
                            canReorder={!isReadOnly} canEdit={false}
                            showRank={true} cards={tableState.table[1]}
                            dispatch={dispatch}/>
                    <div className={"box"}>
                        <button onClick={event => {
                            submitVote(electionId, cookies.userId, choices)
                            setState(state => {
                                return {...state, isReadOnly:true}
                            })
                        }}>Submit</button>
                    </div>
                </div>
            </div>

        </DndProvider>
    </div>
}