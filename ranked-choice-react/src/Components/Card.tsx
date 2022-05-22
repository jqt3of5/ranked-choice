import React, {useEffect, useRef, useState} from 'react';
import './Card.css'
import '../Common/common.css'
import {useDrag, useDrop} from "react-dnd";
import {MdDeleteOutline} from "react-icons/md";
import {CardTableAction, CardTableActionType} from "../Views/CardTableReducer";

export interface CardData {
    id: string
    text : string;
}

export interface CardProps<T> {
    value: string
    id : string
    index: number
    column: number

    canReorder: boolean
    canEdit: boolean
    canDelete: boolean

    dispatch : (action : CardTableAction<T>) => void
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

export function Card<T>(props : CardProps<T>) {

    const [{editing, text}, setState] = useState({editing: false, text: ""})

    const ref = useRef<HTMLDivElement>(null)
    const textArea = useRef<HTMLTextAreaElement>(null)

    const [{handlerId, }, drag] = useDrag({
        type: ItemTypes.CARD,
        collect: monitor => {
            return {handlerId: monitor.getHandlerId(), isDragging: monitor.isDragging()}
        },
        item: monitor => {
           return {index: props.index, column: props.column, id: props.id, type: ItemTypes.CARD}
        },
        canDrag: monitor => props.canReorder && !editing
    })

    const [{isOver},drop] = useDrop<DragItem, void, {isOver : boolean}>({
        accept: ItemTypes.CARD,
        // drop: () => props.moveCard(props.index),
        collect: monitor => {
            return {isOver: monitor.isOver()}
        },
        hover(item: DragItem, monitor) {
            if (props.canReorder && item.id !== props.id)
            {
                props.dispatch({type:CardTableActionType.MoveCard,
                    sourceIndex: item.index, sourceColumn: item.column,
                    destIndex: props.index, destColumn: props.column})
                //while dragging, the item needs to update, so on subsequent calls to this method will pass the correct values to dispatch
                item.index = props.index
                item.column = props.column
                // item.id = props.card.id
            }
        },
        // canDrop: (item, monitor) => !props.canReorder
    }, [props.index, props.column])

    useEffect(() => {
        if (ref.current) {
            ref.current.setAttribute('draggable', String(!editing));
        }
        if (textArea.current && editing){
            textArea.current.focus()
            textArea.current.select()
        }
    }, [editing]);

    drag(ref)
    drop(ref)
    return <div ref={ref} className={"card"} data-handler-id={handlerId}>
        <div className={"card-content"}  style ={{opacity: isOver? 0.4 : 1, backgroundColor: isOver ? "lightgray" : "white"}}>
            {!props.canEdit && <label>{props.value}</label>}
            {props.canEdit && !editing && <label onDoubleClick={event => setState({text:props.value, editing: true})}>{props.value}</label>}

            {editing && <div className={"masked-background"} onClick={event => {
                props.dispatch({type:CardTableActionType.EditCard, value:text, index: props.index, column: props.column})
                setState({text: text, editing: false})
            }}/>}

            {editing && <textarea ref={textArea} value={text} onChange={event => setState(state => {return {...state, text: event.target.value}})}/>}

            {props.canDelete && <MdDeleteOutline className={"card-delete"} onClick={event => {
                props.dispatch({type:CardTableActionType.DeleteCard, index: props.index, column: props.column})
            }}/>}
        </div>
    </div>
}
