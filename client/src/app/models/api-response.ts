export interface ApiResponse {
  code: string
  message: string
  errors?: Record<string, string[]>
}
