import React, {useEffect, useReducer} from 'react';
import {Column} from '../Components/Column';
import './ElectionAdminView.css'
import '../Common/common.css'
import {CardTableActionType, card_table_reducer, CardTableAction, shortId} from "./CardTableReducer";
import {CandidateDTO, ElectionDTO} from "../Common/Data";
import {Card} from "../Components/Card";
import {useCookies} from "react-cookie";
import {v4} from "uuid";
import {useParams} from "react-router-dom";
import {BiDuplicate} from "react-icons/bi";
import {CardTable} from "../Components/Table";
import {getElectionCandidates, saveElectionCandidates, startElection} from "../Common/ElectionModel";
import {IoAdd} from "react-icons/io5";

interface AdminViewState
{
   candidates : CandidateDTO[]
}

function admin_view_reducer(state :AdminViewState, action : CardTableAction<CandidateDTO>) : AdminViewState
{
    var newState = card_table_reducer({
        table:[state.candidates],
        editCard: (value,card) => {return {...card, value: value}}
    }, action)

    return {...state, candidates: newState.table[0]}
}

export function ElectionAdminView() {
    var [state, dispatch] = useReducer(admin_view_reducer, {
        candidates:[]
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
                let candidateCardData = electionResponse.response.candidates
                dispatch({type:CardTableActionType.SetCards, cards: [candidateCardData]})
            }
        }

        fetchData().catch(e => console.log(e))
    },[electionId, cookies.userId])

    useEffect(() => {
        //Seems to be really chatty. Probably should diff and check
        let election :ElectionDTO = {electionId: electionId, candidates: state.candidates}
        saveElectionCandidates(electionId, cookies.userId, election)
    }, [state.candidates, cookies.userId, electionId])


    let electionUrl = `http://localhost:3000/vote/${electionId}`
    return <div className={"create-election-view"}>

        <div className={"create-election-view-settings"}/>

        <CardTable>
            <div>
                <Column name={"Add your candidates"} column={0}
                        canReorder={true} canEdit={true}
                        showRank={false}
                        dispatch={dispatch}>

                    {state.candidates.map((card, index) => {
                        return <Card key={"card" + card.candidateId} id={card.candidateId} value={card.value}
                                     index={index} column={0}
                                     canEdit={true} canReorder={true} canDelete={true}
                                     dispatch={dispatch}/>
                    })}

                    <div className={"add-new-card secondary"} onClick={event =>
                        dispatch({type:CardTableActionType.AddCard, column:0, card: {candidateId: shortId(), value:"Card"}})}>
                        <IoAdd/>Add new card
                    </div>

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