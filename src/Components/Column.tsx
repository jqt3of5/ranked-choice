import React, {useRef} from 'react';
import {Card, CardData, DragItem, ItemTypes} from './Card'
import "./Column.css"
import {useDrag, useDrop} from "react-dnd";
import {CardTableAction, CardTableActionType, CardTableState} from "../Views/CardTableReducer";
import {IoAdd} from "react-icons/io5";

export interface ColumnProps {
    canEdit: boolean
    canReorder : boolean

    name : string
    column : number
    showRank : boolean
    cards : CardData[]

    dispatch : (action : CardTableAction) => void
}

export function Column(props : ColumnProps) {

    const [{isOver}, drop] = useDrop<DragItem, void, {isOver: boolean}>({
        accept: ItemTypes.CARD,
        // drop: () => props.moveCard(props.index),
        collect: monitor => {
            return {isOver: monitor.isOver()}
        },
        hover: (item: DragItem, monitor ) => {
            if (props.cards.length === 0 && props.canReorder)
            {
                props.dispatch({type: CardTableActionType.MoveCard, sourceIndex:item.index, sourceColumn: item.column, destIndex: 0, destColumn: props.column})
                //Update the drag item, otherwise the system gets confused and continuously tries to update
                item.index = 0
                item.column = props.column
            }
        },
        canDrop: (item, monitor) => props.canReorder
    }, [props.column, props.cards])

    return <div ref={drop} className={"column"}>
        <div className={"column-header"}>
            <label>{props.name}</label>
        </div>

        { props.cards.map((card, index) =>
            <div className={"column-row"}>
                {props.showRank && <label><b>{index}</b> </label>}
                <Card key={card.id} card={card}
                      index={index} column={props.column}
                      canEdit={props.canEdit} canReorder={props.canReorder}
                      showRank={props.showRank}
                      dispatch={props.dispatch}/>
            </div>
        )}

        {/*If we are showing the rank, then we also want to show 5 placeholders*/}
        {/*props.showRank && props.cards.length < 5 && [...Array(5 - props.cards.length)].map((value, index) =>
            <Card key={value} card={{text: "", id:"placeholder"+value}}
                  index={index + props.cards.length} column={props.column}
                  canEdit={false} canReorder={false}
                  showRank={props.showRank}
                  />
            )
        */}

        {props.canEdit && <div className={"addNewCard"} onClick={event => props.dispatch({type:CardTableActionType.AddCard, column:props.column})}><IoAdd/>Add new card</div>}
    </div>
}

