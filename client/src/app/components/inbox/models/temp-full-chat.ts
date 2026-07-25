import { Message } from "./message.model"
import { Tags } from "./tags"

export interface FullChat {
  id: string
  avatar: string
  name: string
  lastMessage: string
  lastUpdate: string
  tags: Tags[]
  messages: Message[]
}
