import type { CreateTodoRequest, TodoItem, UpdateTodoRequest } from '@/types'
import api from './api'

export const todoService = {
  async getAll(): Promise<TodoItem[]> {
    const { data } = await api.get<TodoItem[]>('/api/todos')
    return data
  },

  async getById(id: number): Promise<TodoItem> {
    const { data } = await api.get<TodoItem>(`/api/todos/${id}`)
    return data
  },

  async create(payload: CreateTodoRequest): Promise<TodoItem> {
    const { data } = await api.post<TodoItem>('/api/todos', payload)
    return data
  },

  async update(id: number, payload: UpdateTodoRequest): Promise<TodoItem> {
    const { data } = await api.put<TodoItem>(`/api/todos/${id}`, payload)
    return data
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/api/todos/${id}`)
  },
}
