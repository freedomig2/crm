export function SearchBox({
  value,
  onChange,
  placeholder = 'Search...'
}: {
  value: string
  onChange: (value: string) => void
  placeholder?: string
}) {
  return <input value={value} onChange={(e) => onChange(e.target.value)} placeholder={placeholder} className="search-box" />
}
