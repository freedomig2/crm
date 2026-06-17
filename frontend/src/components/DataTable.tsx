type Column<T> = {
  key: keyof T
  title: string
}

export function DataTable<T extends Record<string, unknown>>({
  rows,
  columns,
}: {
  rows: T[]
  columns: Column<T>[]
}) {
  return (
    <table className="crm-table">
      <thead>
        <tr>
          {columns.map((column) => (
            <th key={String(column.key)}>{column.title}</th>
          ))}
        </tr>
      </thead>
      <tbody>
        {rows.map((row, idx) => (
          <tr key={idx}>
            {columns.map((column) => (
              <td key={String(column.key)}>{String(row[column.key] ?? '')}</td>
            ))}
          </tr>
        ))}
      </tbody>
    </table>
  )
}
