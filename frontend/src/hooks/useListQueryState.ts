import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
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
  const defaultsRef = useRef(defaults)
  const numberKeysRef = useRef(new Set<number | string>(numberKeys as Array<string | number>))

  useEffect(() => {
    defaultsRef.current = defaults
  }, [defaults])

  useEffect(() => {
    numberKeysRef.current = new Set<number | string>(numberKeys as Array<string | number>)
  }, [numberKeys])

  const parse = useCallback(
    (raw: URLSearchParams): T => {
      const currentDefaults = defaultsRef.current
      const next = { ...currentDefaults }

      Object.keys(currentDefaults).forEach((key) => {
        const value = raw.get(key)
        if (value === null) {
          return
        }

        if (numberKeysRef.current.has(key)) {
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
    [],
  )

  const [query, setQueryState] = useState<T>(() => parse(searchParams))

  useEffect(() => {
    const next = parse(searchParams)
    setQueryState((current) => {
      const same = Object.keys(next).every((key) => current[key as keyof T] === next[key as keyof T])
      return same ? current : next
    })
  }, [parse, searchParams])

  const setQuery = useCallback(
    (updater: T | ((current: T) => T)) => {
      setQueryState((current) => {
        const next = typeof updater === 'function' ? (updater as (current: T) => T)(current) : updater
        const params = new URLSearchParams()

        Object.keys(next).forEach((key) => {
          const value = next[key as keyof T]
          const defaultValue = defaultsRef.current[key as keyof T]

          if (value === '' || value === defaultValue) {
            return
          }

          params.set(key, String(value))
        })

        setSearchParams(toRecord(params), { replace: true })
        return next
      })
    },
    [setSearchParams],
  )

  const resetQuery = useCallback(() => {
    setQueryState(defaultsRef.current)
    setSearchParams({}, { replace: true })
  }, [setSearchParams])

  return useMemo(
    () => ({ query, setQuery, resetQuery }),
    [query, resetQuery, setQuery],
  )
}
