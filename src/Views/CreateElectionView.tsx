import React, {useEffect, useReducer, useState} from 'react';
import {Column} from '../Components/Column';
import {CardData, CardProps} from "../Components/Card";
import './CreateElectionView.css'
import { DndProvider } from 'react-dnd';
import {HTML5Backend} from "react-dnd-html5-backend";
import {CardTableActionType, reduce} from "./CardTableReducer";

export function CreateElectionView() {
    var [state, dispatch] = useReducer(reduce, {
        table: [[{text:"A", id:"1"}, {text:"B", id:"2"},{text:"C", id:"3"},{text:"D", id:"4"}]]
    })

    useEffect(() => {
        fetch("http://localhost:80/election/12345")
            .then(res => res.json())
            .then(
                (result) => {
                    dispatch({type:CardTableActionType.SetCandidates, candidates: result, choices:[]})
                },
                (error) => {

                }
            )
    }, [])
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
                        dispatch={dispatch}/>
            </div>
        </DndProvider>

        <div className={"box"}>
            <button>Submit</button>
        </div>
    </div>
}