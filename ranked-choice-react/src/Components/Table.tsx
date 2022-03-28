import {ReactNode} from "react";
import {HTML5Backend} from "react-dnd-html5-backend";
import {DndProvider} from "react-dnd";
import './Table.css'

export interface CardTableProps {
    children : ReactNode
}

export function CardTable(props : CardTableProps) {

    return <div className={"card-table"}>
        <DndProvider backend={HTML5Backend}>
            {props.children}
        </DndProvider>
    </div>
}
