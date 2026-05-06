export interface AuthResponse {
  token: string
  name: string
  email: string
  role: string
}

export interface TodoItem {
  id: number
  title: string
  description: string
  isCompleted: boolean
  createdAt: string
  completedAt: string | null
  userId: number
}

export interface CreateTodoRequest {
  title: string
  description: string
}

export interface UpdateTodoRequest {
  title: string
  description: string
  isCompleted: boolean
}

export interface User {
  name: string
  email: string
  role: string
  token: string
}
