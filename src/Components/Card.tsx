import React, {useRef, useState} from 'react';
import './Card.css'
import {useDrag, useDrop} from "react-dnd";
import {MdDeleteOutline} from "react-icons/md";
import {CardTableAction, CardTableActionType, CardTableState} from "../Views/CardTableReducer";

export interface CardData {
    id: string
    text : string;
}

export interface CardProps {
    card: CardData

    index: number
    column: number

    showRank : boolean
    canReorder: boolean
    canEdit: boolean

    dispatch : (action : CardTableAction) => void
}

export const ItemTypes = {
    CARD: 'card',
}

export interface DragItem {
    index: number
    column : number
    id: string
    type: string
}

export function Card(props : CardProps) {

    const [{editing}, setState] = useState({editing: false})

    const ref = useRef<HTMLDivElement>(null)

    const [{isDragging}, drag] = useDrag({
        type: ItemTypes.CARD,
        collect: monitor => {
            return {isDragging: monitor.isDragging()}
        },
        item: monitor => {
           return {index: props.index, column: props.column, id: props.card.id, type: ItemTypes.CARD}
        },
        canDrag: monitor => props.canReorder && !editing
    })

    const [{isOver}, drop] = useDrop<DragItem, void, {isOver: boolean}>({
        accept: ItemTypes.CARD,
        // drop: () => props.moveCard(props.index),
        collect: monitor => {
            return {isOver: monitor.isOver()}
        },
        hover: (item: DragItem, monitor ) => {
            if (props.canReorder)
            {
                props.dispatch({type:CardTableActionType.MoveCard, sourceIndex: item.index, sourceColumn: item.column, destIndex: props.index, destColumn: props.column})

                //Update the drag item, otherwise the system gets confused and continuously tries to update
                item.index = props.index
                item.column = props.column
            }
        },
        // canDrop: (item, monitor) => !props.canReorder
    }, [props.index, props.column])

    drag(drop(ref))
    return <div ref={ref} className={"card"} style={{opacity: isOver? 0.5 : 1}}>

        {props.canEdit && !editing && <label onDoubleClick={event => setState({editing: true})}>{props.card.text}</label>}
        {props.canEdit && editing && <input onBlur={event => {
            props.dispatch({type:CardTableActionType.EditCard, card:{...props.card, text:event.target.value}, index: props.index, column: props.column})

            setState({editing: false})
            }} defaultValue={props.card.text}/>
        }

        {!props.canEdit && <label>{props.card.text}</label>}

        {props.canEdit && <MdDeleteOutline onClick={event => {
            props.dispatch({type:CardTableActionType.DeleteCard, index: props.index, column: props.column})
        }}/>}
    </div>
}
