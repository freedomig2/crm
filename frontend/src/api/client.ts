import axios from 'axios'

const baseURL = import.meta.env.VITE_API_BASE_URL ?? '/'

export const api = axios.create({
  baseURL,
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('crm.accessToken')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

let isRefreshing = false
let queued: Array<(token: string) => void> = []

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config
    if (error.response?.status !== 401 || original._retry) {
      throw error
    }

    const refreshToken = localStorage.getItem('crm.refreshToken')
    if (!refreshToken) {
      throw error
    }

    if (isRefreshing) {
      return new Promise((resolve) => {
        queued.push((token: string) => {
          original.headers.Authorization = `Bearer ${token}`
          resolve(api(original))
        })
      })
    }

    isRefreshing = true
    original._retry = true

    try {
      const { data } = await axios.post(`${baseURL}api/auth/refresh-token`, {
        refreshToken,
      })

      localStorage.setItem('crm.accessToken', data.accessToken)
      localStorage.setItem('crm.refreshToken', data.refreshToken)
      if (data.user) {
        localStorage.setItem('crm.user', JSON.stringify(data.user))
      }
      if (typeof window !== 'undefined') {
        window.dispatchEvent(new Event('crm-auth-updated'))
      }
      queued.forEach((cb) => cb(data.accessToken))
      queued = []
      original.headers.Authorization = `Bearer ${data.accessToken}`
      return api(original)
    } catch (refreshError) {
      localStorage.removeItem('crm.accessToken')
      localStorage.removeItem('crm.refreshToken')
      throw refreshError
    } finally {
      isRefreshing = false
    }
  },
)
