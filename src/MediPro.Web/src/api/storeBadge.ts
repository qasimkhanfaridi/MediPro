export function storeBadgeClass(status: string): string {
  switch ((status ?? '').toLowerCase()) {
    case 'approved':
      return 'badge badge-approved'
    case 'pending':
      return 'badge badge-pending'
    case 'rejected':
      return 'badge badge-rejected'
    case 'suspended':
      return 'badge badge-suspended'
    default:
      return 'badge'
  }
}

/** Match API / JSON casing for approval workflow (buttons, labels). */
export function isStorePendingApproval(status: string | undefined | null): boolean {
  return (status ?? '').toLowerCase() === 'pending'
}
