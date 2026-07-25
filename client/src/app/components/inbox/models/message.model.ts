export interface Message {
  id: number
  text: string
  time: string
  send: boolean
  received?: boolean
}
