export interface TicketCard {
  conversationId: string
  conversationType: string | null
  owner: string | null

  ticketId: string
  subject: string
  status: string

  contactId: string
  contactDisplayName: string
  contactPhone: string | null
  contactProfilePic: string | null

  tags: TagItem[]

  lastMessageContent: string | null
  lastMessageCreatedAt: string | null

  read: boolean
}

export interface TagItem {
  id: string
  name: string
  colorCode: string
}
