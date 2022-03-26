import React, {useEffect, useReducer, useState} from 'react';
import {Column} from '../Components/Column';
import './CreateElectionView.css'
import { DndProvider } from 'react-dnd';
import {HTML5Backend} from "react-dnd-html5-backend";
import {CardTableActionType, reduce} from "./CardTableReducer";
import {Candidate} from "../Common/Data";
import {CardData} from "../Components/Card";

function getCandidates(electionId : string) : Promise<CardData[]>
{
    return fetch(`https://localhost:5001/election/${electionId}`)
        .then(res => res.json().then(v => v as Candidate[]))
        .then(
            (result) => {
                //convert to cardData
                let cards = result.map(c => {return {id:c.candidateId, text: c.value}})
                return cards
            },
            (error) => {
                return []
            }
        )
}

function saveCandidates(electionId : string, cardData: CardData [])
{
    let candidates : Candidate[] = cardData.map(value => {return {electionId: electionId, candidateId: value.id, value: value.text}})
    fetch(`https://localhost:5001/election/${electionId}`,
        {method: "POST", headers:{"Content-Type":"application/json"},
            body:JSON.stringify(candidates)})
}

export function CreateElectionView() {
    var [state, dispatch] = useReducer(reduce, {
        table: [[],[]]
    })

    var electionId = "12345"

    useEffect(() => {
        getCandidates(electionId).then((cards => {
            dispatch({type: CardTableActionType.SetCards, cards: [cards]})
        }))
    })

    //TODO: Generate a cookie for ourselves
    //TODO: Get candidate list, and ranked list from server based on cookie
    //TODO: handle submit
    //TODO: Make lists readonly if previously posted

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

        <div className={"box"}>
            <button onClick={event => {
                saveCandidates(electionId, state.table[0])
            }}>Save</button>

        </div>
    </div>
}