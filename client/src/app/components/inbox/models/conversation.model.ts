import { Message } from "./message.model"

export interface Conversation {
  chat_id: string
  avatar: string
  name: string
  content: Message[]
  contactId: string
  createdAt: string
}
