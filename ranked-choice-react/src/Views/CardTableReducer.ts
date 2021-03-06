import {CardData} from "../Components/Card";
export interface CardTableState<T>
{
    table: T[][]
    editCard: (value:string, card: T)=>T
}
export enum CardTableActionType {
    SetCards,
    AddCard,
    DeleteCard,
    EditCard,
    MoveCard
}
export type CardTableAction<T> =
    | {type: CardTableActionType.SetCards, cards: T[][]}
    | {type: CardTableActionType.AddCard, column:number, card: T}
    | {type: CardTableActionType.DeleteCard, index: number, column:number}
    | {type: CardTableActionType.EditCard, value: string, index: number, column: number}
    | {type: CardTableActionType.MoveCard, sourceIndex: number, sourceColumn: number, destIndex : number, destColumn: number}

export function shortId(length : number = 8) : string
{
    const chars = "abcdefghijklmnopqrstubvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
    let result = ""
    for (let i = 0; i < length; ++i)
    {
       result += chars[Math.floor(Math.random()*chars.length)]
    }

    return result
}
export function card_table_reducer<T>(state : CardTableState<T>, action : CardTableAction<T>) : CardTableState<T>
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
                            return column.concat(action.card)
                        }
                        return column
                    }
                )}
        case CardTableActionType.DeleteCard:
            return {...state, table: state.table.map((column, col) => {
                    if (action.column === col)
                    {
                        column.splice(action.index, 1)
                    }
                    return column
                })
            }
        case CardTableActionType.EditCard:
            return {...state, table: state.table.map((column, col) => {
                    return column.map((card, index) => {
                        if (action.index === index && action.column === col)
                        {
                            return state.editCard(action.value, card)
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