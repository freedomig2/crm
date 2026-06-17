import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <div className="authPage">
      <div className="authCard">
        <h1>Page Not Found</h1>
        <Link to="/">Go Home</Link>
      </div>
    </div>
  )
}
