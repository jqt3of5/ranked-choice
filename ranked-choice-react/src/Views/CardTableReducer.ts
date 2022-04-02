import {CardData} from "../Components/Card";
export interface CardTableState
{
    table: CardData[][]
}
export enum CardTableActionType {
    SetCards,
    AddCard,
    DeleteCard,
    EditCard,
    MoveCard
}
export type CardTableAction =
    | {type: CardTableActionType.SetCards, cards: CardData[][]}
    | {type: CardTableActionType.AddCard, column:number}
    | {type: CardTableActionType.DeleteCard, index: number, column:number}
    | {type: CardTableActionType.EditCard, card:CardData, index: number, column: number}
    | {type: CardTableActionType.MoveCard, sourceIndex: number, sourceColumn: number, destIndex : number, destColumn: number}

function shortId(length : number = 8) : string
{
    const chars = "abcdefghijklmnopqrstubvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
    let result = ""
    for (let i = 0; i < length; ++i)
    {
       result += chars[Math.floor(Math.random()*chars.length)]
    }

    return result
}
export function card_table_reducer(state : CardTableState, action : CardTableAction) : CardTableState
{
    switch(action.type)
    {
        case CardTableActionType.SetCards:
            return {...state, table:action.cards}
        case CardTableActionType.AddCard:
            const total = state.table.map(value => value.length).reduce((previous, current) => previous + current)

            return {...state, table: state.table.map((column, col) => {
                        if (col === action.column)
                        {
                            return column.concat({text:"Card " + total, id:shortId()})
                        }
                        return column
                    }
                )}
        case CardTableActionType.DeleteCard:
            return {...state, table: state.table.map((column, col) => {
                    if (action.column == col)
                    {
                        column.splice(action.index, 1)
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
            let removed = state.table.map((cd, col) => {
                let column = cd.map(a => a)

                if (col === action.sourceColumn)
                {
                    column.splice(action.sourceIndex, 1)
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