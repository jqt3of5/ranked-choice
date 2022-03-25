import {CardData} from "../Components/Card";
export interface CardTableState
{
    table: CardData[][]
}
export enum CardTableActionType {
    SetCandidates,
    AddCard,
    DeleteCard,
    EditCard,
    MoveCard
}
export type CardTableAction =
    | {type: CardTableActionType.SetCandidates, candidates : CardData[], choices:CardData[]}
    | {type: CardTableActionType.AddCard, column:number}
    | {type: CardTableActionType.DeleteCard, index: number, column:number}
    | {type: CardTableActionType.EditCard, card:CardData, index: number, column: number}
    | {type: CardTableActionType.MoveCard, sourceIndex: number, sourceColumn: number, destIndex : number, destColumn: number}

export function reduce(state : CardTableState, action : CardTableAction) : CardTableState
{
    switch(action.type)
    {
        case CardTableActionType.SetCandidates:
            return {...state, table:[action.candidates,action.choices]}
        case CardTableActionType.AddCard:
            const total = state.table.map(value => value.length).reduce((previous, current) => previous + current)

            return {...state, table: state.table.map((column, col) => {
                        if (col === action.column)
                        {
                            return column.concat({text:"New Card", id:"card"+total})
                        }
                        return column
                    }
                )}
        case CardTableActionType.DeleteCard:
            return {...state, table: state.table.map((column, col) => {
                    if (action.column == col)
                    {
                        return column.filter((card, index) => {
                            return index !== action.index
                        })
                    }
                    return column
                })
            }
        case CardTableActionType.EditCard:
            return {...state, table: state.table.map((column, col) => {
                    return column.map((card, index) => {
                        if (action.index == index && action.column == col)
                        {
                            return action.card
                        }
                        return card
                    })
                })
            }
        case CardTableActionType.MoveCard:
            let c = state.table[action.sourceColumn][action.sourceIndex]
            let removed = state.table.map((column, col) => {

                if (col === action.sourceColumn)
                {
                    column = column.filter((card, index) => {
                        return index !== action.sourceIndex
                    })
                }

                if (col === action.destColumn)
                {
                    column.splice(action.destIndex, 0, c)
                }
                return column
            })
            return {...state, table:removed}
    }
}