import React, {useEffect, useReducer, useState} from 'react';
import {Column} from '../Components/Column';
import './VoteView.css'
import '../Common/common.css'
import {card_table_reducer, CardTableAction, CardTableActionType} from "./CardTableReducer";
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
    allCandidates : CandidateDTO[]
    candidates: CandidateDTO[]
    choices : CandidateDTO[]
}

function vote_view_reducer(state : VoteViewState, action : CardTableAction<CandidateDTO>) : VoteViewState
{
    switch (action.type)
    {
        case CardTableActionType.SetCards:
            var candidates = action.cards[0].filter(card => action.cards[1].find(c => c.candidateId=== card.candidateId) === undefined)

            state = {...state, allCandidates: action.cards[0], candidates: candidates, choices:action.cards[1]}
            break;
    }

    var newState = card_table_reducer<CandidateDTO>({
        table: [state.candidates, state.choices],
        editCard: (value,card) => {return {...card, value: value}}
    }, action)

    candidates = state.allCandidates.filter(card => newState.table[1].find(c => c.candidateId=== card.candidateId) === undefined)
    return {...state, candidates: candidates, choices:newState.table[1]}
}

export function VoteView() {

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

    var [{isReadOnly, isSubmitted, dataRetrieved}, setState] = useState({isReadOnly: false,isSubmitted: false, dataRetrieved: false})

    var [{allCandidates, candidates, choices}, dispatch] = useReducer(vote_view_reducer,{
        allCandidates: [],
        candidates: [],
        choices: []
    })

    const fetchData = async () => {
        var voteResponse = await getVote(electionId, cookies.userId)
        var electionResponse = await getElectionCandidates(electionId, cookies.userId)

        if (electionResponse.response != null && voteResponse.response != null)
        {
            let candidateCardData = electionResponse.response.candidates
            let voteCardData = voteResponse.response.candidates

            dispatch({type: CardTableActionType.SetCards, cards:[candidateCardData, voteCardData]})
            setState({isSubmitted:voteResponse.response.submitted, isReadOnly: voteResponse.response.submitted, dataRetrieved: true})
        }
    }

    useEffect(() => {

        fetchData().catch(e => console.log(e))

    }, [])

    const saveData = async () => {
        setState(state => {return {...state, isReadOnly: true}})

        let voteDTO : VoteDTO = {submitted: false, candidates: choices }
        var vote = await saveVote(electionId, cookies.userId, voteDTO)

        setState(state => {return {...state, isReadOnly: false}})
    }

    useEffect(() => {

        if (dataRetrieved)
        {
            saveData().catch(e => console.log(e))
        }

    }, [choices])

    //TODO: If no election exists, show error

    return <div className={"vote-view"}>
        <div className={"vote-view-header primary"}>
            {!isSubmitted && <button onClick={() =>
                submitVote(electionId, cookies.userId)
                .then(response =>
                    setState(state => {return {...state, isSubmitted:true, isReadOnly:true}}))}>
                Submit Vote
            </button>}
            {isSubmitted && <label>Submitted!</label>}

        </div>
        <CardTable>
           <Column canEdit={false} canReorder={!isReadOnly} name={"Candidates"} column={0} showRank={false} dispatch={dispatch}>
               {candidates.map((card, index) => {
                    return <Card key={"card" + card.candidateId} id={card.candidateId} value={card.value}
                                 index={index} column={0}
                                 canEdit={false} canReorder={!isReadOnly} canDelete={false}
                                 dispatch={dispatch}/>
               })}
           </Column>
            <Column canEdit={false} canReorder={!isReadOnly} name={"Ranked Choices"} column={1} showRank={true} dispatch={dispatch}>
                {choices.map((card, index) => {
                    return <Card key={"card" + card.candidateId} id={card.candidateId} value={card.value}
                                 index={index} column={1}
                                 canEdit={false} canReorder={!isReadOnly} canDelete={!isReadOnly}
                                 dispatch={dispatch}/>
                })}
            </Column>
        </CardTable>
    </div>
}