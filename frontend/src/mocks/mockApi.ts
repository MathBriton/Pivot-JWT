import MockAdapter from 'axios-mock-adapter'
import api from '@/services/api'

const MOCK_TOKEN = 'mock.jwt.token'

const MOCK_USER = {
  token: MOCK_TOKEN,
  name: 'Usuário Teste',
  email: 'teste@email.com',
  role: 'User',
}

let nextId = 4
const todos = [
  { id: 1, title: 'Aprender .NET 10 Minimal API', description: 'Estudar os novos recursos de minimal API', isCompleted: false, createdAt: new Date().toISOString(), completedAt: null, userId: 1 },
  { id: 2, title: 'Criar app React', description: 'Projeto full stack com TypeScript e Tailwind', isCompleted: false, createdAt: new Date().toISOString(), completedAt: null, userId: 1 },
  { id: 3, title: 'Configurar JWT Auth', description: 'Autenticação com JWT Bearer tokens', isCompleted: true, createdAt: new Date().toISOString(), completedAt: new Date().toISOString(), userId: 1 },
]

export function setupMocks() {
  const mock = new MockAdapter(api, { delayResponse: 300 })

  // Auth - Register
  mock.onPost('/api/auth/register').reply((config) => {
    const body = JSON.parse(config.data)
    if (!body.name || !body.email || !body.password) {
      return [400, { message: 'Campos obrigatórios faltando.' }]
    }
    if (body.password.length < 6) {
      return [400, { message: 'Senha deve ter pelo menos 6 caracteres.' }]
    }
    return [200, { ...MOCK_USER, name: body.name, email: body.email }]
  })

  // Auth - Login
  mock.onPost('/api/auth/login').reply((config) => {
    const { email, password } = JSON.parse(config.data)
    if (email === 'teste@email.com' && password === 'teste123') {
      return [200, MOCK_USER]
    }
    return [401, { message: 'Email ou senha inválidos.' }]
  })

  // Todos - List
  mock.onGet('/api/todos').reply(200, [...todos])

  // Todos - Get by id
  mock.onGet(/\/api\/todos\/\d+/).reply((config) => {
    const id = Number(config.url?.split('/').pop())
    const todo = todos.find(t => t.id === id)
    return todo ? [200, todo] : [404]
  })

  // Todos - Create
  mock.onPost('/api/todos').reply((config) => {
    const body = JSON.parse(config.data)
    const todo = {
      id: nextId++,
      title: body.title,
      description: body.description ?? '',
      isCompleted: false,
      createdAt: new Date().toISOString(),
      completedAt: null,
      userId: 1,
    }
    todos.unshift(todo)
    return [201, todo]
  })

  // Todos - Update
  mock.onPut(/\/api\/todos\/\d+/).reply((config) => {
    const id = Number(config.url?.split('/').pop())
    const idx = todos.findIndex(t => t.id === id)
    if (idx === -1) return [404]
    const body = JSON.parse(config.data)
    todos[idx] = {
      ...todos[idx],
      title: body.title,
      description: body.description,
      isCompleted: body.isCompleted,
      completedAt: body.isCompleted ? new Date().toISOString() : null,
    }
    return [200, todos[idx]]
  })

  // Todos - Delete
  mock.onDelete(/\/api\/todos\/\d+/).reply((config) => {
    const id = Number(config.url?.split('/').pop())
    const idx = todos.findIndex(t => t.id === id)
    if (idx === -1) return [404]
    todos.splice(idx, 1)
    return [204]
  })
}
