import React, {useEffect, useReducer, useState} from 'react';
import {Column} from '../Components/Column';
import './CreateElectionView.css'
import { DndProvider } from 'react-dnd';
import {HTML5Backend} from "react-dnd-html5-backend";
import {CardTableActionType, card_table_reducer} from "./CardTableReducer";
import {Candidate, Election} from "../Common/Data";
import {CardData} from "../Components/Card";
import {getElection, saveCandidates} from "../Common/ElectionModel";
import {useCookies} from "react-cookie";
import {v4} from "uuid";
import {useParams} from "react-router-dom";
import {BiDuplicate} from "react-icons/bi";

export function CreateElectionView() {
    var [state, dispatch] = useReducer(card_table_reducer, {
        table: [[],[]]
    })

    const [cookies, setCookie] = useCookies(['userId'])
    if (cookies.userId===undefined)
    {
        setCookie('userId', v4())
    }

    let params = useParams();
    if (params.electionId===undefined)
    {
        throw "electionID cannot be undefined"
    }
    let electionId = params.electionId as string

    useEffect(() => {
        getElection(electionId, cookies.userId).then((election => {
            let candidateCardData = election.candidates.map(c => {return {id:c.candidateId, text: c.value}})

            dispatch({type:CardTableActionType.SetCards, cards: [candidateCardData]})
        }))
    },[electionId])

    useEffect(() => {
        //Seems to be really chatty. Probably should diff and check
        let candidates : Candidate[] = state.table[0].map(value => {return {electionId: electionId, candidateId: value.id, value: value.text}})
        saveCandidates(electionId, cookies.userId, candidates)
    }, [state.table])

    //TODO: Deleting then adding cards will mess up the card ids

    let electionUrl = `http://localhost:3000/vote/${electionId}`
    return <div className={"create-election-view"}>

        <DndProvider backend={HTML5Backend}>
            <div className={"create-election-view-table"}>
                <Column name={"Add your possible Selections"} column={0}
                        canReorder={true} canEdit={true}
                        showRank={false} cards={state.table[0]}
                        dispatch={dispatch}
                />
            </div>
        </DndProvider>

        <div className={"box create-election-share"}>
            <a href={electionUrl}>{electionUrl}</a>
            <BiDuplicate/>
        </div>
    </div>
}