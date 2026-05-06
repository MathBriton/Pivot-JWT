import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { LogOut, Plus, Pencil, Trash2, Check, ClipboardList } from 'lucide-react'
import { useAuth } from '@/contexts/AuthContext'
import { todoService } from '@/services/todo.service'
import type { TodoItem } from '@/types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Label } from '@/components/ui/label'

interface TodoForm {
  title: string
  description: string
}

export function DashboardPage() {
  const { user, logout } = useAuth()
  const [todos, setTodos] = useState<TodoItem[]>([])
  const [editingId, setEditingId] = useState<number | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const { register, handleSubmit, reset } = useForm<TodoForm>()
  const { register: registerEdit, handleSubmit: handleEditSubmit, reset: resetEdit } = useForm<TodoForm>()

  const fetchTodos = async () => {
    try {
      const data = await todoService.getAll()
      setTodos(data)
    } catch {
      setError('Falha ao carregar as tarefas.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { fetchTodos() }, [])

  const onCreate = async ({ title, description }: TodoForm) => {
    const todo = await todoService.create({ title, description })
    setTodos(prev => [todo, ...prev])
    reset()
  }

  const startEdit = (todo: TodoItem) => {
    setEditingId(todo.id)
    resetEdit({ title: todo.title, description: todo.description })
  }

  const onUpdate = async (id: number, data: TodoForm) => {
    const todo = todos.find(t => t.id === id)!
    const updated = await todoService.update(id, { ...data, isCompleted: todo.isCompleted })
    setTodos(prev => prev.map(t => (t.id === id ? updated : t)))
    setEditingId(null)
  }

  const onToggle = async (todo: TodoItem) => {
    const updated = await todoService.update(todo.id, {
      title: todo.title,
      description: todo.description,
      isCompleted: !todo.isCompleted,
    })
    setTodos(prev => prev.map(t => (t.id === todo.id ? updated : t)))
  }

  const onDelete = async (id: number) => {
    await todoService.delete(id)
    setTodos(prev => prev.filter(t => t.id !== id))
  }

  const pending = todos.filter(t => !t.isCompleted)
  const completed = todos.filter(t => t.isCompleted)

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 sticky top-0 z-10">
        <div className="max-w-4xl mx-auto px-4 py-4 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <ClipboardList className="h-6 w-6 text-blue-600" />
            <span className="font-bold text-gray-900 text-lg">Minhas Tarefas</span>
          </div>
          <div className="flex items-center gap-3">
            <div className="text-right hidden sm:block">
              <p className="text-sm font-medium text-gray-900">{user?.name}</p>
              <p className="text-xs text-gray-500">{user?.email}</p>
            </div>
            <Button variant="outline" size="sm" onClick={logout}>
              <LogOut className="h-4 w-4 mr-1" />
              Sair
            </Button>
          </div>
        </div>
      </header>

      <main className="max-w-4xl mx-auto px-4 py-8 space-y-6">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Nova Tarefa</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(onCreate)} className="space-y-3">
              <div className="space-y-1">
                <Label htmlFor="title">Título</Label>
                <Input id="title" placeholder="O que precisa ser feito?" {...register('title', { required: true })} />
              </div>
              <div className="space-y-1">
                <Label htmlFor="desc">Descrição</Label>
                <Input id="desc" placeholder="Detalhes opcionais..." {...register('description')} />
              </div>
              <Button type="submit" size="sm" className="gap-1">
                <Plus className="h-4 w-4" />
                Adicionar
              </Button>
            </form>
          </CardContent>
        </Card>

        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md text-sm">{error}</div>
        )}

        {loading ? (
          <div className="text-center py-12 text-gray-400">Carregando...</div>
        ) : (
          <>
            <section>
              <h2 className="text-sm font-semibold text-gray-500 uppercase tracking-wide mb-3">
                Pendentes ({pending.length})
              </h2>
              <div className="space-y-2">
                {pending.length === 0 && (
                  <p className="text-center text-gray-400 py-6 text-sm">Nenhuma tarefa pendente. Bom trabalho!</p>
                )}
                {pending.map(todo => (
                  <TodoCard
                    key={todo.id}
                    todo={todo}
                    isEditing={editingId === todo.id}
                    registerEdit={registerEdit}
                    handleEditSubmit={handleEditSubmit}
                    onToggle={onToggle}
                    onDelete={onDelete}
                    startEdit={startEdit}
                    onUpdate={onUpdate}
                    cancelEdit={() => setEditingId(null)}
                  />
                ))}
              </div>
            </section>

            {completed.length > 0 && (
              <section>
                <h2 className="text-sm font-semibold text-gray-500 uppercase tracking-wide mb-3">
                  Concluídas ({completed.length})
                </h2>
                <div className="space-y-2">
                  {completed.map(todo => (
                    <TodoCard
                      key={todo.id}
                      todo={todo}
                      isEditing={editingId === todo.id}
                      registerEdit={registerEdit}
                      handleEditSubmit={handleEditSubmit}
                      onToggle={onToggle}
                      onDelete={onDelete}
                      startEdit={startEdit}
                      onUpdate={onUpdate}
                      cancelEdit={() => setEditingId(null)}
                    />
                  ))}
                </div>
              </section>
            )}
          </>
        )}
      </main>
    </div>
  )
}

interface TodoCardProps {
  todo: TodoItem
  isEditing: boolean
  registerEdit: ReturnType<typeof useForm<TodoForm>>['register']
  handleEditSubmit: ReturnType<typeof useForm<TodoForm>>['handleSubmit']
  onToggle: (todo: TodoItem) => void
  onDelete: (id: number) => void
  startEdit: (todo: TodoItem) => void
  onUpdate: (id: number, data: TodoForm) => void
  cancelEdit: () => void
}

function TodoCard({ todo, isEditing, registerEdit, handleEditSubmit, onToggle, onDelete, startEdit, onUpdate, cancelEdit }: TodoCardProps) {
  return (
    <Card className={todo.isCompleted ? 'opacity-60' : ''}>
      <CardContent className="py-3 px-4">
        {isEditing ? (
          <form onSubmit={handleEditSubmit(data => onUpdate(todo.id, data))} className="space-y-2">
            <Input placeholder="Título" {...registerEdit('title', { required: true })} />
            <Input placeholder="Descrição" {...registerEdit('description')} />
            <div className="flex gap-2">
              <Button type="submit" size="sm" variant="default">Salvar</Button>
              <Button type="button" size="sm" variant="outline" onClick={cancelEdit}>Cancelar</Button>
            </div>
          </form>
        ) : (
          <div className="flex items-start gap-3">
            <button
              onClick={() => onToggle(todo)}
              className={`mt-0.5 w-5 h-5 rounded border-2 flex-shrink-0 flex items-center justify-center transition-colors ${
                todo.isCompleted ? 'bg-green-500 border-green-500 text-white' : 'border-gray-300 hover:border-blue-500'
              }`}
            >
              {todo.isCompleted && <Check className="h-3 w-3" />}
            </button>
            <div className="flex-1 min-w-0">
              <p className={`font-medium text-sm ${todo.isCompleted ? 'line-through text-gray-400' : 'text-gray-900'}`}>
                {todo.title}
              </p>
              {todo.description && (
                <p className="text-xs text-gray-500 mt-0.5 truncate">{todo.description}</p>
              )}
              <p className="text-xs text-gray-400 mt-1">
                {new Date(todo.createdAt).toLocaleDateString('pt-BR')}
              </p>
            </div>
            <div className="flex items-center gap-1 flex-shrink-0">
              {todo.isCompleted && <Badge variant="secondary" className="text-xs">Feito</Badge>}
              <Button variant="ghost" size="sm" onClick={() => startEdit(todo)} className="h-7 w-7 p-0">
                <Pencil className="h-3.5 w-3.5" />
              </Button>
              <Button variant="ghost" size="sm" onClick={() => onDelete(todo.id)} className="h-7 w-7 p-0 text-red-500 hover:text-red-700">
                <Trash2 className="h-3.5 w-3.5" />
              </Button>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
