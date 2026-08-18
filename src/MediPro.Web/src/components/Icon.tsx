export type IconName =
  | 'home'
  | 'medicine'
  | 'cart'
  | 'orders'
  | 'pharmacy'
  | 'box'
  | 'offer'
  | 'console'
  | 'signIn'
  | 'signOut'
  | 'lock'
  | 'search'
  | 'truck'

const paths: Record<IconName, React.ReactNode> = {
  home: (
    <>
      <path d="M3.6 10.4 12 3.8l8.4 6.6" />
      <path d="M5.8 9.2V19a1.2 1.2 0 0 0 1.2 1.2h10a1.2 1.2 0 0 0 1.2-1.2V9.2" />
      <path d="M10 20.2v-5.4h4v5.4" />
    </>
  ),
  medicine: (
    <g transform="rotate(-45 12 12)">
      <rect x="3.8" y="8.5" width="16.4" height="7" rx="3.5" />
      <path d="M12 8.5v7" />
    </g>
  ),
  cart: (
    <>
      <circle cx="9.6" cy="19.8" r="1.3" />
      <circle cx="17.8" cy="19.8" r="1.3" />
      <path d="M2.8 3.8h2.3l2.6 10.9a1.5 1.5 0 0 0 1.46 1.15h8.2a1.5 1.5 0 0 0 1.46-1.16L20.4 7.4H6.1" />
    </>
  ),
  orders: (
    <>
      <path d="M6.6 2.8h10.8a1 1 0 0 1 1 1v17.4l-2.95-1.9-2.95 1.9-2.95-1.9-2.95 1.9V3.8a1 1 0 0 1 1-1z" />
      <path d="M9.2 8.2h5.6" />
      <path d="M9.2 12.2h5.6" />
    </>
  ),
  pharmacy: (
    <>
      <path d="M3.2 9.4h17.6l-1.5-5.1a1.2 1.2 0 0 0-1.15-.9H5.85a1.2 1.2 0 0 0-1.15.9z" />
      <path d="M4.8 9.4v10a1 1 0 0 0 1 1h12.4a1 1 0 0 0 1-1v-10" />
      <path d="M12 12.4v4.4" />
      <path d="M9.8 14.6h4.4" />
    </>
  ),
  box: (
    <>
      <path d="M20.6 8.4 12 3.6 3.4 8.4v7.2l8.6 4.8 8.6-4.8z" />
      <path d="M3.4 8.4 12 13.2l8.6-4.8" />
      <path d="M12 13.2v7.2" />
    </>
  ),
  offer: (
    <>
      <path d="M11.6 3.2h7.6a1.2 1.2 0 0 1 1.2 1.2v7.6a1.2 1.2 0 0 1-.35.85l-7.6 7.6a1.2 1.2 0 0 1-1.7 0l-7.6-7.6a1.2 1.2 0 0 1 0-1.7l7.6-7.6a1.2 1.2 0 0 1 .85-.35z" />
      <circle cx="16.4" cy="7.6" r="1.3" />
    </>
  ),
  console: (
    <>
      <rect x="3.4" y="3.4" width="7.2" height="7.2" rx="1.6" />
      <rect x="13.4" y="3.4" width="7.2" height="7.2" rx="1.6" />
      <rect x="3.4" y="13.4" width="7.2" height="7.2" rx="1.6" />
      <rect x="13.4" y="13.4" width="7.2" height="7.2" rx="1.6" />
    </>
  ),
  signIn: (
    <>
      <path d="M13.6 3.4h4.6a1.6 1.6 0 0 1 1.6 1.6v14a1.6 1.6 0 0 1-1.6 1.6h-4.6" />
      <path d="M9.4 8.2 13.2 12l-3.8 3.8" />
      <path d="M13.2 12H4" />
    </>
  ),
  signOut: (
    <>
      <path d="M10.4 3.4H5.8A1.6 1.6 0 0 0 4.2 5v14a1.6 1.6 0 0 0 1.6 1.6h4.6" />
      <path d="M15.6 8.2 19.4 12l-3.8 3.8" />
      <path d="M19.4 12h-9.2" />
    </>
  ),
  lock: (
    <>
      <rect x="4.6" y="10.4" width="14.8" height="10.2" rx="1.8" />
      <path d="M8.2 10.4V7.6a3.8 3.8 0 0 1 7.6 0v2.8" />
    </>
  ),
  search: (
    <>
      <circle cx="10.8" cy="10.8" r="6.6" />
      <path d="M15.6 15.6 20.4 20.4" />
    </>
  ),
  truck: (
    <>
      <path d="M2.8 6.4h10.4v10H2.8z" />
      <path d="M13.2 9.6h3.6l3.4 3.2v3.6h-7z" />
      <circle cx="7" cy="18.4" r="1.6" />
      <circle cx="16.6" cy="18.4" r="1.6" />
    </>
  ),
}

export function Icon({ name, className }: { name: IconName; className?: string }) {
  return (
    <svg
      className={className ? `icon ${className}` : 'icon'}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={1.7}
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      {paths[name]}
    </svg>
  )
}
