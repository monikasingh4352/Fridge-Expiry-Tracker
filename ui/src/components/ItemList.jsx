function describeDaysLeft(daysLeft) {
  if (daysLeft < -1) return `Expired ${-daysLeft} days ago`
  if (daysLeft === -1) return 'Expired yesterday'
  if (daysLeft === 0) return 'Expires today'
  if (daysLeft === 1) return 'Expires tomorrow'
  return `${daysLeft} days left`
}

function formatDate(isoDate) {
  // isoDate is "yyyy-MM-dd" from the API; show it as e.g. "22 Nov 2026"
  const [y, m, d] = isoDate.split('-').map(Number)
  return new Date(y, m - 1, d).toLocaleDateString(undefined, {
    day: 'numeric', month: 'short', year: 'numeric'
  })
}

export default function ItemList({ items, onDelete }) {
  if (items.length === 0) {
    return <p className="empty">Your fridge is empty. Add an item above.</p>
  }

  // The API already sorts by expiry date, so the first item is the one to use first.
  return (
    <ol className="item-list">
      {items.map((item, index) => (
        <li key={item.id} className={`item item-${item.status}`}>
          <span className="rank">{index + 1}</span>
          <div className="item-main">
            <span className="item-name">
              {item.name} {item.quantity > 1 && <span className="qty">×{item.quantity}</span>}
            </span>
            <span className="item-date">
              <span className="category">{item.category}</span> · {formatDate(item.expiryDate)}
            </span>
          </div>
          <span className="days">{describeDaysLeft(item.daysLeft)}</span>
          <button className="remove" onClick={() => onDelete(item.id)}
                  aria-label={`Remove ${item.name}`}>Used / Remove</button>
        </li>
      ))}
    </ol>
  )
}
