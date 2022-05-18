import React, {useEffect, useReducer} from 'react';
import {Column} from '../Components/Column';
import './ElectionAdminView.css'
import '../Common/common.css'
import {CardTableActionType, card_table_reducer} from "./CardTableReducer";
import {CandidateDTO, ElectionDTO} from "../Common/Data";
import {Card} from "../Components/Card";
import {useCookies} from "react-cookie";
import {v4} from "uuid";
import {useParams} from "react-router-dom";
import {BiDuplicate} from "react-icons/bi";
import {CardTable} from "../Components/Table";
import {getElectionCandidates, saveElectionCandidates, startElection} from "../Common/ElectionModel";

export function ElectionAdminView() {
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
        throw new Error("electionID cannot be undefined")
    }
    let electionId = params.electionId as string

    useEffect(() => {
        const fetchData = async () =>
        {
            var electionResponse = await getElectionCandidates(electionId, cookies.userId)
            if (electionResponse.response != null)
            {
                let candidateCardData = electionResponse.response.candidates.map(c => {return {id:c.candidateId, text: c.value}})
                dispatch({type:CardTableActionType.SetCards, cards: [candidateCardData]})
            }
        }

        fetchData().catch(e => console.log(e))
    },[electionId, cookies.userId])

    useEffect(() => {
        //Seems to be really chatty. Probably should diff and check
        let candidates : CandidateDTO[] = state.table[0].map(value => {return {electionId: electionId, candidateId: value.id, value: value.text}})
        let election :ElectionDTO = {electionId: electionId, candidates: candidates}
        saveElectionCandidates(electionId, cookies.userId, election)
    }, [state.table, cookies.userId, electionId])


    let electionUrl = `http://localhost:3000/vote/${electionId}`
    return <div className={"create-election-view"}>

        <div className={"create-election-view-settings"}/>

        <CardTable>
            <div>
                <Column name={"Add your candidates"} column={0}
                        canReorder={true} canEdit={true}
                        showRank={false}
                        dispatch={dispatch}>

                    {state.table[0].map((card, index) => {
                        return <Card key={"card" + card.id} card={card}
                                     index={index} column={0}
                                     canEdit={true} canReorder={true} canDelete={true}
                                     dispatch={dispatch}/>
                    })}

                </Column>
                <div className={"box create-election-share"}>
                    <div>
                        <a href={electionUrl}>{electionUrl}</a>
                        <BiDuplicate/>
                    </div>
                    <button onClick={() => startElection(electionId, cookies.userId)}>Start</button>
                </div>
            </div>

        </CardTable>

        <div className={"create-election-view-trail"}/>
    </div>
}