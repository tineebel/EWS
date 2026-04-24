import axios from 'axios'

export const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

export interface JsendList<T> {
  status: string
  data: {
    items: T[]
    totalRows: number
    totalPage: number
    page: number
    pageSize: number
  }
}

export interface JsendData<T> {
  status: string
  data: T
}
