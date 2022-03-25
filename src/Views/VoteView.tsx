import React, {useEffect, useReducer, useState} from 'react';
import {Column} from '../Components/Column';
import {CardData, CardProps} from "../Components/Card";
import './VoteView.css'
import { DndProvider } from 'react-dnd';
import {HTML5Backend} from "react-dnd-html5-backend";
import {CardTableActionType, CardTableState, reduce} from "./CardTableReducer";

interface VoteViewState
{
    table: CardTableState
}

export function VoteView() {
    var [state, dispatch] = useReducer(reduce, {
        table: [[],[]]
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

    return <div className={"vote-view"}>
        <DndProvider backend={HTML5Backend}>
            <div className={"vote-view-table"}>
                <Column key={"candidates"}  name={"Candidates"} column={0}
                        canReorder={true} canEdit={false}
                        showRank={false} cards={state.table[0]}
                        dispatch={dispatch}/>
                <div>
                    <Column key={"ranked_choice"} name={"Ranked Choice"} column={1}
                            canReorder={true} canEdit={false}
                            showRank={true} cards={state.table[1]}
                            dispatch={dispatch}/>
                    <div className={"box"}>
                        <button>Submit</button>
                    </div>
                </div>
            </div>

        </DndProvider>
    </div>
}