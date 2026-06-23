import { useCallback, useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'

type UseListQueryOptions<T extends Record<string, string | number>> = {
  defaults: T
  numberKeys?: Array<keyof T>
}

const toRecord = (params: URLSearchParams): Record<string, string> => {
  const next: Record<string, string> = {}
  params.forEach((value, key) => {
    next[key] = value
  })
  return next
}

export function useListQueryState<T extends Record<string, string | number>>({
  defaults,
  numberKeys = [],
}: UseListQueryOptions<T>) {
  const [searchParams, setSearchParams] = useSearchParams()

  const parse = useCallback(
    (raw: URLSearchParams): T => {
      const next = { ...defaults }

      Object.keys(defaults).forEach((key) => {
        const value = raw.get(key)
        if (value === null) {
          return
        }

        if (numberKeys.includes(key as keyof T)) {
          const parsed = Number(value)
          if (!Number.isNaN(parsed)) {
            ;(next[key as keyof T] as number) = parsed
          }
          return
        }

        ;(next[key as keyof T] as string) = value
      })

      return next
    },
    [defaults, numberKeys],
  )

  const [query, setQueryState] = useState<T>(() => parse(searchParams))

  useEffect(() => {
    setQueryState(parse(searchParams))
  }, [parse, searchParams])

  const setQuery = useCallback(
    (updater: T | ((current: T) => T)) => {
      setQueryState((current) => {
        const next = typeof updater === 'function' ? (updater as (current: T) => T)(current) : updater
        const params = new URLSearchParams()

        Object.keys(next).forEach((key) => {
          const value = next[key as keyof T]
          const defaultValue = defaults[key as keyof T]

          if (value === '' || value === defaultValue) {
            return
          }

          params.set(key, String(value))
        })

        setSearchParams(toRecord(params), { replace: true })
        return next
      })
    },
    [defaults, setSearchParams],
  )

  const resetQuery = useCallback(() => {
    setQueryState(defaults)
    setSearchParams({}, { replace: true })
  }, [defaults, setSearchParams])

  return useMemo(
    () => ({ query, setQuery, resetQuery }),
    [query, resetQuery, setQuery],
  )
}
