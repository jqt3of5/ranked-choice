import React, {ReactNode} from 'react';
import {DragItem, ItemTypes} from './Card'
import "./Column.css"
import "../Common/common.css"
import {useDrop} from "react-dnd";
import {CardTableAction, CardTableActionType} from "../Views/CardTableReducer";
import {IoAdd} from "react-icons/io5";

export interface ColumnProps<T> {
    canEdit: boolean
    canReorder : boolean

    name : string
    column : number
    showRank : boolean

    children : ReactNode[]

    dispatch : (action : CardTableAction<T>) => void
}

export function Column<T>(props : ColumnProps<T>) {

    const [, drop] = useDrop<DragItem, void, {isOver: boolean}>({
        accept: ItemTypes.CARD,
        // drop: () => props.moveCard(props.index),
        collect: monitor => {
            return {isOver: monitor.isOver()}
        },
        hover: (item: DragItem, monitor ) => {
            if (props.children != null && props.children.length === 0 && props.canReorder)
            {
                props.dispatch({type: CardTableActionType.MoveCard, sourceIndex:item.index, sourceColumn: item.column, destIndex: 0, destColumn: props.column})
                //Update the drag item, otherwise the system gets confused and continuously tries to update
                item.index = 0
                item.column = props.column
            }
        },
        canDrop: (item, monitor) => props.canReorder
    }, [props.column, props.children])

    return <div ref={drop} className={"card-table-column"}>
        <div className={"box"}>
            <div className={"column-header"}>
                <label>{props.name}</label>
            </div>

            { props.children.map((card, index) =>
                <div key={"div" + index} className={"column-row"}>
                    {props.showRank && <label><b>{index}</b> </label>}
                    {card}
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

        </div>
    </div>
}

