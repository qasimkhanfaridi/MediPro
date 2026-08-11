import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react'
import { apiFetch, apiUrl, parseErrorDetail } from '../api/client'
import { safeGetItem, safeRemoveItem, safeSetItem } from '../lib/safeStorage'
import {
  defaultAdminEmail,
  defaultAdminPassword,
  defaultRegisterAddress,
  defaultRegisterBusiness,
  defaultRegisterCity,
  defaultRegisterContact,
  defaultRegisterEmail,
  defaultRegisterMobile,
  defaultRegisterPassword,
} from '../api/constants'
import type {
  AuthResponse,
  CartDto,
  HealthResponse,
  ImportProductsResult,
  OrderDetailDto,
  OrderLocationOptions,
  PagedNotifications,
  PagedOrders,
  PagedProducts,
  ProductDto,
  StoreSummary,
} from '../api/types'

export type MediProContextValue = {
  health: HealthResponse | null
  healthError: string | null
  email: string
  setEmail: (v: string) => void
  password: string
  setPassword: (v: string) => void
  token: string | null
  authInfo: AuthResponse | null
  authError: string | null
  login: () => Promise<void>
  logout: () => void
  products: PagedProducts | null
  productsError: string | null
  loadProducts: (opts?: {
    search?: string
    quiet?: boolean
    manufacturer?: string
    category?: string
    salt?: string
  }) => Promise<void>
  cart: CartDto | null
  cartError: string | null
  loadCart: () => Promise<void>
  addOneToCart: (productId: string) => Promise<void>
  setCartLineQuantity: (productId: string, quantity: number) => Promise<void>
  removeCartLine: (productId: string) => Promise<void>
  submitOrder: () => Promise<void>
  orderMsg: string | null
  setOrderMsg: (v: string | null) => void
  orders: PagedOrders | null
  ordersError: string | null
  loadOrders: (opts?: {
    quiet?: boolean
    city?: string
    area?: string
    search?: string
    status?: string
    from?: string
    to?: string
    page?: number
    pageSize?: number
  }) => Promise<void>
  loadOrderDetail: (orderId: string) => Promise<OrderDetailDto | null>
  updateOrderStatus: (orderId: string, status: string, statusNotes?: string) => Promise<boolean>
  orderLocationOptions: OrderLocationOptions | null
  orderLocationError: string | null
  loadOrderLocationOptions: () => Promise<void>
  ordersDemoMsg: string | null
  ordersDemoError: string | null
  seedDemoOrders: () => Promise<void>
  notifications: PagedNotifications | null
  notifError: string | null
  loadNotifications: (opts?: { quiet?: boolean }) => Promise<void>
  markNotificationRead: (id: string) => Promise<void>
  importResult: ImportProductsResult | null
  importError: string | null
  fileInputRef: React.RefObject<HTMLInputElement | null>
  handleImportFiles: (files: FileList | null) => Promise<void>
  stores: StoreSummary[] | null
  storesError: string | null
  loadStores: (opts?: { quiet?: boolean }) => Promise<void>
  setStoreApproval: (id: string, status: 'Approved' | 'Rejected') => Promise<void>
  regEmail: string
  setRegEmail: (v: string) => void
  regPassword: string
  setRegPassword: (v: string) => void
  regBusinessName: string
  setRegBusinessName: (v: string) => void
  regAddressLine: string
  setRegAddressLine: (v: string) => void
  regCity: string
  setRegCity: (v: string) => void
  regMobile: string
  setRegMobile: (v: string) => void
  regContactName: string
  setRegContactName: (v: string) => void
  regLicenseNumber: string
  setRegLicenseNumber: (v: string) => void
  registerError: string | null
  registerMsg: string | null
  registerStore: () => Promise<void>
  busy: string | null
  canUseCart: boolean
  isAdmin: boolean
  refreshCartClick: () => Promise<void>
  stockMsg: string | null
  stockError: string | null
  adjustStock: (skuCode: string, delta: number) => Promise<void>
  catalogDemoMsg: string | null
  catalogDemoError: string | null
  seedDemoCatalog: () => Promise<void>
  storesDemoMsg: string | null
  storesDemoError: string | null
  seedDemoStores: () => Promise<void>
}

const MediProContext = createContext<MediProContextValue | null>(null)

export function useMediPro(): MediProContextValue {
  const v = useContext(MediProContext)
  if (!v) throw new Error('useMediPro must be used within MediProProvider')
  return v
}

export function MediProProvider({ children }: { children: ReactNode }) {
  const [health, setHealth] = useState<HealthResponse | null>(null)
  const [healthError, setHealthError] = useState<string | null>(null)

  const [email, setEmail] = useState(defaultAdminEmail)
  const [password, setPassword] = useState(defaultAdminPassword)
  const [token, setToken] = useState<string | null>(() =>
    safeGetItem('medipro_token'),
  )
  const [authInfo, setAuthInfo] = useState<AuthResponse | null>(() => {
    const raw = safeGetItem('medipro_auth_info')
    if (!raw) return null
    try {
      return JSON.parse(raw) as AuthResponse
    } catch {
      return null
    }
  })
  const [authError, setAuthError] = useState<string | null>(null)

  const [products, setProducts] = useState<PagedProducts | null>(null)
  const [productsError, setProductsError] = useState<string | null>(null)

  const [cart, setCart] = useState<CartDto | null>(null)
  const [cartError, setCartError] = useState<string | null>(null)

  const [orders, setOrders] = useState<PagedOrders | null>(null)
  const [ordersError, setOrdersError] = useState<string | null>(null)
  const [orderMsg, setOrderMsg] = useState<string | null>(null)
  const [orderLocationOptions, setOrderLocationOptions] =
    useState<OrderLocationOptions | null>(null)
  const [orderLocationError, setOrderLocationError] = useState<string | null>(null)
  const [ordersDemoMsg, setOrdersDemoMsg] = useState<string | null>(null)
  const [ordersDemoError, setOrdersDemoError] = useState<string | null>(null)

  const [notifications, setNotifications] = useState<PagedNotifications | null>(
    null,
  )
  const [notifError, setNotifError] = useState<string | null>(null)
  const [importResult, setImportResult] = useState<ImportProductsResult | null>(
    null,
  )
  const [importError, setImportError] = useState<string | null>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const [stores, setStores] = useState<StoreSummary[] | null>(null)
  const [storesError, setStoresError] = useState<string | null>(null)

  const [regEmail, setRegEmail] = useState(defaultRegisterEmail)
  const [regPassword, setRegPassword] = useState(defaultRegisterPassword)
  const [regBusinessName, setRegBusinessName] = useState(defaultRegisterBusiness)
  const [regAddressLine, setRegAddressLine] = useState(defaultRegisterAddress)
  const [regCity, setRegCity] = useState(defaultRegisterCity)
  const [regMobile, setRegMobile] = useState(defaultRegisterMobile)
  const [regContactName, setRegContactName] = useState(defaultRegisterContact)
  const [regLicenseNumber, setRegLicenseNumber] = useState('')
  const [registerError, setRegisterError] = useState<string | null>(null)
  const [registerMsg, setRegisterMsg] = useState<string | null>(null)

  const [stockMsg, setStockMsg] = useState<string | null>(null)
  const [stockError, setStockError] = useState<string | null>(null)

  const [catalogDemoMsg, setCatalogDemoMsg] = useState<string | null>(null)
  const [catalogDemoError, setCatalogDemoError] = useState<string | null>(null)

  const [storesDemoMsg, setStoresDemoMsg] = useState<string | null>(null)
  const [storesDemoError, setStoresDemoError] = useState<string | null>(null)

  const [busy, setBusy] = useState<string | null>(null)
  /** Last catalogue query (search + filters); used for quiet reloads after checkout. */
  const lastProductsQueryRef = useRef<{
    search?: string
    manufacturer?: string
    category?: string
    salt?: string
  }>({})
  const lastOrdersQueryRef = useRef<{
    city?: string
    area?: string
    search?: string
    status?: string
    from?: string
    to?: string
    page?: number
    pageSize?: number
  }>({ pageSize: 50 })

  const canUseCart =
    authInfo?.role === 'StoreUser' &&
    authInfo.storeApprovalStatus === 'Approved'

  const isAdmin = authInfo?.role === 'DistributorAdmin'

  const loadCart = useCallback(async () => {
    if (!token || !canUseCart) return
    setCartError(null)
    const res = await apiFetch('/api/cart', { accessToken: token })
    const text = await res.text()
    if (!res.ok) {
      setCartError(parseErrorDetail(text) || `HTTP ${res.status}`)
      setCart(null)
      return
    }
    setCart(JSON.parse(text) as CartDto)
  }, [canUseCart, token])

  useEffect(() => {
    fetch(apiUrl('/api/health'))
      .then((res) => {
        if (!res.ok) throw new Error(`HTTP ${res.status}`)
        return res.json() as Promise<HealthResponse>
      })
      .then(setHealth)
      .catch((e: unknown) =>
        setHealthError(e instanceof Error ? e.message : 'Request failed'),
      )
  }, [])

  useEffect(() => {
    if (canUseCart && token) void loadCart()
    else {
      setCart(null)
      setCartError(null)
    }
  }, [canUseCart, token, loadCart])

  const applySession = useCallback((data: AuthResponse) => {
    setAuthInfo(data)
    setToken(data.accessToken)
    safeSetItem('medipro_token', data.accessToken)
    safeSetItem('medipro_auth_info', JSON.stringify(data))
  }, [])

  const clearFeatureState = useCallback(() => {
    setProducts(null)
    setProductsError(null)
    setCart(null)
    setCartError(null)
    setOrders(null)
    setOrdersError(null)
    setOrderMsg(null)
    setStores(null)
    setStoresError(null)
    setNotifications(null)
    setNotifError(null)
    setImportResult(null)
    setImportError(null)
    setStockMsg(null)
    setStockError(null)
    setCatalogDemoMsg(null)
    setCatalogDemoError(null)
    setStoresDemoMsg(null)
    setStoresDemoError(null)
    lastProductsQueryRef.current = {}
  }, [])

  const login = useCallback(async () => {
    setAuthError(null)
    setAuthInfo(null)
    setOrderMsg(null)
    setRegisterError(null)
    setRegisterMsg(null)
    setStockMsg(null)
    setStockError(null)
    setCatalogDemoMsg(null)
    setCatalogDemoError(null)
    setStoresDemoMsg(null)
    setStoresDemoError(null)
    setBusy('login')
    try {
      const res = await fetch(apiUrl('/api/auth/login'), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      })
      const text = await res.text()
      if (!res.ok) {
        setAuthError(parseErrorDetail(text) || `HTTP ${res.status}`)
        setToken(null)
        safeRemoveItem('medipro_token')
        safeRemoveItem('medipro_auth_info')
        return
      }
      const data = JSON.parse(text) as AuthResponse
      applySession(data)
    } finally {
      setBusy(null)
    }
  }, [applySession, email, password])

  const logout = useCallback(() => {
    setToken(null)
    setAuthInfo(null)
    setProducts(null)
    setCart(null)
    setOrders(null)
    setOrderMsg(null)
    setNotifications(null)
    setNotifError(null)
    setImportResult(null)
    setImportError(null)
    setStores(null)
    setStoresError(null)
    setRegisterError(null)
    setRegisterMsg(null)
    setStockMsg(null)
    setStockError(null)
    setProductsError(null)
    setCartError(null)
    setOrdersError(null)
    setStockMsg(null)
    setStockError(null)
    setCatalogDemoMsg(null)
    setCatalogDemoError(null)
    setStoresDemoMsg(null)
    setStoresDemoError(null)
    lastProductsQueryRef.current = {}
    safeRemoveItem('medipro_token')
    safeRemoveItem('medipro_auth_info')
  }, [])

  const registerStore = useCallback(async () => {
    setRegisterError(null)
    setRegisterMsg(null)
    setAuthError(null)
    setBusy('register-store')
    const body = {
      email: regEmail.trim(),
      password: regPassword,
      businessName: regBusinessName.trim(),
      addressLine: regAddressLine.trim(),
      city: regCity.trim(),
      mobile: regMobile.trim(),
      contactName: regContactName.trim(),
      licenseNumber: regLicenseNumber.trim(),
    }
    try {
      const res = await fetch(apiUrl('/api/auth/register-store'), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
      })
      const text = await res.text()
      if (!res.ok) {
        setRegisterError(parseErrorDetail(text) || `HTTP ${res.status}`)
        return
      }
      const data = JSON.parse(text) as AuthResponse
      clearFeatureState()
      applySession(data)
      setRegisterMsg(
        'Welcome to MediPro. Your pharmacy account is active and pending approval. Your distributor will review your details — once approved, you can browse the catalog and place orders.',
      )
    } finally {
      setBusy(null)
    }
  }, [
    applySession,
    clearFeatureState,
    regAddressLine,
    regBusinessName,
    regCity,
    regContactName,
    regEmail,
    regLicenseNumber,
    regMobile,
    regPassword,
  ])

  const loadProducts = useCallback(
    async (opts?: {
      search?: string
      quiet?: boolean
      manufacturer?: string
      category?: string
      salt?: string
    }) => {
      if (!token) return
      setProductsError(null)
      const quiet = opts?.quiet === true

      const query: {
        search?: string
        manufacturer?: string
        category?: string
        salt?: string
      } = quiet
        ? { ...lastProductsQueryRef.current }
        : {
            search:
              opts?.search !== undefined ? opts.search.trim() || undefined : undefined,
            manufacturer:
              opts?.manufacturer !== undefined
                ? opts.manufacturer.trim() || undefined
                : undefined,
            category:
              opts?.category !== undefined
                ? opts.category.trim() || undefined
                : undefined,
            salt: opts?.salt !== undefined ? opts.salt.trim() || undefined : undefined,
          }

      lastProductsQueryRef.current = query

      if (!quiet) {
        setProducts(null)
        setBusy('products')
      }
      const params = new URLSearchParams({ page: '1', pageSize: '24' })
      if (query.search) params.set('search', query.search)
      if (query.manufacturer) params.set('manufacturer', query.manufacturer)
      if (query.category) params.set('category', query.category)
      if (query.salt) params.set('salt', query.salt)
      try {
        const res = await apiFetch(`/api/products?${params}`, {
          accessToken: token,
        })
        const text = await res.text()
        if (!res.ok) {
          setProductsError(parseErrorDetail(text) || `HTTP ${res.status}`)
          if (!quiet) setProducts(null)
          return
        }
        setProducts(JSON.parse(text) as PagedProducts)
      } finally {
        if (!quiet) setBusy(null)
      }
    },
    [token],
  )

  const addOneToCart = useCallback(
    async (productId: string) => {
      if (!token) return
      setCartError(null)
      setBusy(`cart-add-${productId}`)
      try {
        const res = await apiFetch('/api/cart/items', {
          method: 'POST',
          accessToken: token,
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ productId, quantity: 1 }),
        })
        const text = await res.text()
        if (!res.ok) {
          setCartError(parseErrorDetail(text) || `HTTP ${res.status}`)
          return
        }
        setCart(JSON.parse(text) as CartDto)
      } finally {
        setBusy(null)
      }
    },
    [token],
  )

  const setCartLineQuantity = useCallback(
    async (productId: string, quantity: number) => {
      if (!token) return
      setCartError(null)
      setBusy(`cart-qty-${productId}`)
      try {
        const res = await apiFetch(`/api/cart/items/${productId}`, {
          method: 'PATCH',
          accessToken: token,
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ quantity }),
        })
        const text = await res.text()
        if (!res.ok) {
          setCartError(parseErrorDetail(text) || `HTTP ${res.status}`)
          return
        }
        setCart(JSON.parse(text) as CartDto)
      } finally {
        setBusy(null)
      }
    },
    [token],
  )

  const removeCartLine = useCallback(
    async (productId: string) => {
      if (!token) return
      setCartError(null)
      setBusy(`cart-remove-${productId}`)
      try {
        const res = await apiFetch(`/api/cart/items/${productId}`, {
          method: 'DELETE',
          accessToken: token,
        })
        const text = await res.text()
        if (!res.ok) {
          setCartError(parseErrorDetail(text) || `HTTP ${res.status}`)
          return
        }
        setCart(JSON.parse(text) as CartDto)
      } finally {
        setBusy(null)
      }
    },
    [token],
  )

  const loadOrders = useCallback(
    async (opts?: {
      quiet?: boolean
      city?: string
      area?: string
      search?: string
      status?: string
      from?: string
      to?: string
      page?: number
      pageSize?: number
    }) => {
      if (!token) return
      setOrdersError(null)
      const quiet = opts?.quiet === true

      const query = quiet
        ? { ...lastOrdersQueryRef.current }
        : {
            city: opts?.city?.trim() || undefined,
            area: opts?.area?.trim() || undefined,
            search: opts?.search?.trim() || undefined,
            status: opts?.status?.trim() || undefined,
            from: opts?.from?.trim() || undefined,
            to: opts?.to?.trim() || undefined,
            page: opts?.page ?? 1,
            pageSize: opts?.pageSize ?? 50,
          }
      lastOrdersQueryRef.current = query

      if (!quiet) setBusy('orders')
      const params = new URLSearchParams({
        page: String(query.page ?? 1),
        pageSize: String(query.pageSize ?? 50),
      })
      if (query.city) params.set('city', query.city)
      if (query.area) params.set('area', query.area)
      if (query.search) params.set('search', query.search)
      if (query.status) params.set('status', query.status)
      if (query.from) params.set('from', query.from)
      if (query.to) params.set('to', query.to)
      try {
        const res = await apiFetch(`/api/orders?${params}`, {
          accessToken: token,
        })
        const text = await res.text()
        if (!res.ok) {
          setOrdersError(parseErrorDetail(text) || `HTTP ${res.status}`)
          if (!quiet) setOrders(null)
          return
        }
        setOrders(JSON.parse(text) as PagedOrders)
      } finally {
        if (!quiet) setBusy(null)
      }
    },
    [token],
  )

  const loadOrderDetail = useCallback(
    async (orderId: string): Promise<OrderDetailDto | null> => {
      if (!token) return null
      try {
        const res = await apiFetch(`/api/orders/${orderId}`, { accessToken: token })
        const text = await res.text()
        if (!res.ok) return null
        return JSON.parse(text) as OrderDetailDto
      } catch {
        return null
      }
    },
    [token],
  )

  const updateOrderStatus = useCallback(
    async (orderId: string, status: string, statusNotes?: string): Promise<boolean> => {
      if (!token || !isAdmin) return false
      setOrdersError(null)
      setBusy(`order-status-${orderId}`)
      try {
        const res = await apiFetch(`/api/orders/${orderId}/status`, {
          method: 'PATCH',
          accessToken: token,
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ status, statusNotes: statusNotes ?? null }),
        })
        const text = await res.text()
        if (!res.ok) {
          setOrdersError(parseErrorDetail(text) || `HTTP ${res.status}`)
          return false
        }
        await loadOrders({ quiet: true })
        return true
      } finally {
        setBusy(null)
      }
    },
    [isAdmin, loadOrders, token],
  )

  const submitOrder = useCallback(async () => {
    if (!token) return
    setOrderMsg(null)
    setCartError(null)
    setBusy('order-submit')
    try {
      const res = await apiFetch('/api/orders/submit', {
        method: 'POST',
        accessToken: token,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({}),
      })
      if (!res.ok) {
        const text = await res.text()
        setCartError(parseErrorDetail(text) || `HTTP ${res.status}`)
        return
      }
      await res.json().catch(() => null)
      setOrderMsg('Order submitted successfully.')
      void loadCart()
      void loadOrders({ quiet: true })
      void loadProducts({ quiet: true })
    } finally {
      setBusy(null)
    }
  }, [loadCart, loadOrders, loadProducts, token])

  const loadStores = useCallback(
    async (opts?: { quiet?: boolean }) => {
      if (!token || !isAdmin) return
      setStoresError(null)
      if (!opts?.quiet) setBusy('stores')
      try {
        const res = await apiFetch('/api/admin/stores', { accessToken: token })
        const text = await res.text()
        if (!res.ok) {
          setStoresError(text || `HTTP ${res.status}`)
          setStores(null)
          return
        }
        setStores(JSON.parse(text) as StoreSummary[])
      } finally {
        if (!opts?.quiet) setBusy(null)
      }
    },
    [isAdmin, token],
  )

  const setStoreApproval = useCallback(
    async (id: string, status: 'Approved' | 'Rejected') => {
      if (!token) return
      setStoresError(null)
      setBusy(`approve-${id}`)
      try {
        const res = await apiFetch(`/api/admin/stores/${id}/approval`, {
          method: 'PATCH',
          accessToken: token,
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ status, notes: null }),
        })
        const text = await res.text()
        if (!res.ok) {
          setStoresError(text || `HTTP ${res.status}`)
          return
        }
        await loadStores({ quiet: true })
      } finally {
        setBusy(null)
      }
    },
    [loadStores, token],
  )

  const handleImportFiles = useCallback(
    async (files: FileList | null) => {
      setImportError(null)
      setImportResult(null)
      const file = files?.[0]
      if (!file || !token) return
      setBusy('import')
      try {
        const fd = new FormData()
        fd.append('file', file)
        const res = await apiFetch('/api/admin/catalog/import', {
          method: 'POST',
          accessToken: token,
          body: fd,
        })
        const text = await res.text()
        if (!res.ok) {
          setImportError(text || `HTTP ${res.status}`)
          return
        }
        setImportResult(JSON.parse(text) as ImportProductsResult)
        void loadProducts()
      } finally {
        setBusy(null)
      }
    },
    [loadProducts, token],
  )

  const loadNotifications = useCallback(
    async (opts?: { quiet?: boolean }) => {
      if (!token || !isAdmin) return
      setNotifError(null)
      if (!opts?.quiet) setBusy('notifications')
      try {
        const res = await apiFetch(
          '/api/admin/notifications?page=1&pageSize=20',
          { accessToken: token },
        )
        const text = await res.text()
        if (!res.ok) {
          setNotifError(text || `HTTP ${res.status}`)
          setNotifications(null)
          return
        }
        setNotifications(JSON.parse(text) as PagedNotifications)
      } finally {
        if (!opts?.quiet) setBusy(null)
      }
    },
    [isAdmin, token],
  )

  const markNotificationRead = useCallback(
    async (id: string) => {
      if (!token) return
      setBusy(`notif-read-${id}`)
      try {
        const res = await apiFetch(`/api/admin/notifications/${id}/read`, {
          method: 'PATCH',
          accessToken: token,
        })
        if (res.ok) await loadNotifications({ quiet: true })
      } finally {
        setBusy(null)
      }
    },
    [loadNotifications, token],
  )

  const refreshCartClick = useCallback(async () => {
    setBusy('cart-refresh')
    try {
      await loadCart()
    } finally {
      setBusy(null)
    }
  }, [loadCart])

  const adjustStock = useCallback(
    async (skuCode: string, delta: number) => {
      if (!token || !isAdmin) return
      setStockMsg(null)
      setStockError(null)
      setBusy('stock')
      try {
        const res = await apiFetch('/api/admin/catalog/stock-adjustment', {
          method: 'POST',
          accessToken: token,
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ skuCode: skuCode.trim(), delta }),
        })
        const text = await res.text()
        if (!res.ok) {
          setStockError(parseErrorDetail(text) || `HTTP ${res.status}`)
          return
        }
        const dto = JSON.parse(text) as ProductDto
        const qty = dto.stockQuantity
        setStockMsg(
          `Updated ${dto.skuCode} — on hand is now ${qty === null ? '—' : String(qty)} units.`,
        )
        setProducts((prev) => {
          if (!prev) return prev
          const norm = (s: string) => s.trim().toLowerCase()
          const i = prev.items.findIndex(
            (p) =>
              p.id === dto.id || norm(p.skuCode) === norm(dto.skuCode),
          )
          if (i < 0) return prev
          const items = [...prev.items]
          items[i] = dto
          return { ...prev, items }
        })
      } finally {
        setBusy(null)
      }
    },
    [isAdmin, token],
  )

  const seedDemoCatalog = useCallback(async () => {
    if (!token || !isAdmin) return
    setCatalogDemoMsg(null)
    setCatalogDemoError(null)
    setBusy('demo-catalog')
    try {
      const res = await apiFetch('/api/admin/catalog/seed-demo-catalog', {
        method: 'POST',
        accessToken: token,
      })
      const text = await res.text()
      if (!res.ok) {
        setCatalogDemoError(parseErrorDetail(text) || `HTTP ${res.status}`)
        return
      }
      const j = JSON.parse(text) as { inserted?: number; message?: string }
      setCatalogDemoMsg(
        j.message ??
          (typeof j.inserted === 'number'
            ? `Inserted ${j.inserted} demo products.`
            : 'Demo catalogue request completed.'),
      )
      void loadProducts({ quiet: true })
    } finally {
      setBusy(null)
    }
  }, [isAdmin, loadProducts, token])

  const seedDemoStores = useCallback(async () => {
    if (!token || !isAdmin) return
    setStoresDemoMsg(null)
    setStoresDemoError(null)
    setBusy('demo-stores')
    try {
      const res = await apiFetch('/api/admin/stores/seed-demo', {
        method: 'POST',
        accessToken: token,
      })
      const text = await res.text()
      if (!res.ok) {
        setStoresDemoError(parseErrorDetail(text) || `HTTP ${res.status}`)
        return
      }
      const j = JSON.parse(text) as { inserted?: number; message?: string }
      setStoresDemoMsg(
        j.message ??
          (typeof j.inserted === 'number'
            ? `Created ${j.inserted} demo store account(s).`
            : 'Demo stores request completed.'),
      )
      void loadStores({ quiet: true })
    } finally {
      setBusy(null)
    }
  }, [isAdmin, loadStores, token])

  const loadOrderLocationOptions = useCallback(async () => {
    if (!token || !isAdmin) return
    setOrderLocationError(null)
    try {
      const res = await apiFetch('/api/admin/orders/location-options', {
        accessToken: token,
      })
      const text = await res.text()
      if (!res.ok) {
        setOrderLocationError(parseErrorDetail(text) || `HTTP ${res.status}`)
        setOrderLocationOptions(null)
        return
      }
      setOrderLocationOptions(JSON.parse(text) as OrderLocationOptions)
    } catch {
      setOrderLocationError('Could not load city/area options.')
      setOrderLocationOptions(null)
    }
  }, [isAdmin, token])

  const seedDemoOrders = useCallback(async () => {
    if (!token || !isAdmin) return
    setOrdersDemoMsg(null)
    setOrdersDemoError(null)
    setBusy('demo-orders')
    try {
      const res = await apiFetch('/api/admin/orders/seed-demo', {
        method: 'POST',
        accessToken: token,
      })
      const text = await res.text()
      if (!res.ok) {
        setOrdersDemoError(parseErrorDetail(text) || `HTTP ${res.status}`)
        return
      }
      const j = JSON.parse(text) as { inserted?: number; message?: string }
      setOrdersDemoMsg(
        j.message ??
          (typeof j.inserted === 'number'
            ? `Created ${j.inserted} demo order(s).`
            : 'Demo orders request completed.'),
      )
      void loadOrderLocationOptions()
      void loadOrders({ quiet: true })
    } finally {
      setBusy(null)
    }
  }, [isAdmin, loadOrderLocationOptions, loadOrders, token])

  const value = useMemo<MediProContextValue>(
    () => ({
      health,
      healthError,
      email,
      setEmail,
      password,
      setPassword,
      token,
      authInfo,
      authError,
      login,
      logout,
      products,
      productsError,
      loadProducts,
      cart,
      cartError,
      loadCart,
      addOneToCart,
      setCartLineQuantity,
      removeCartLine,
      submitOrder,
      orderMsg,
      setOrderMsg,
      orders,
      ordersError,
      loadOrders,
      loadOrderDetail,
      updateOrderStatus,
      orderLocationOptions,
      orderLocationError,
      loadOrderLocationOptions,
      ordersDemoMsg,
      ordersDemoError,
      seedDemoOrders,
      notifications,
      notifError,
      loadNotifications,
      markNotificationRead,
      importResult,
      importError,
      fileInputRef,
      handleImportFiles,
      stores,
      storesError,
      loadStores,
      setStoreApproval,
      regEmail,
      setRegEmail,
      regPassword,
      setRegPassword,
      regBusinessName,
      setRegBusinessName,
      regAddressLine,
      setRegAddressLine,
      regCity,
      setRegCity,
      regMobile,
      setRegMobile,
      regContactName,
      setRegContactName,
      regLicenseNumber,
      setRegLicenseNumber,
      registerError,
      registerMsg,
      registerStore,
      busy,
      canUseCart,
      isAdmin,
      refreshCartClick,
      stockMsg,
      stockError,
      adjustStock,
      catalogDemoMsg,
      catalogDemoError,
      seedDemoCatalog,
      storesDemoMsg,
      storesDemoError,
      seedDemoStores,
    }),
    [
      health,
      healthError,
      email,
      password,
      token,
      authInfo,
      authError,
      login,
      logout,
      products,
      productsError,
      loadProducts,
      cart,
      cartError,
      loadCart,
      addOneToCart,
      setCartLineQuantity,
      removeCartLine,
      submitOrder,
      orderMsg,
      orders,
      ordersError,
      loadOrders,
      loadOrderDetail,
      updateOrderStatus,
      orderLocationOptions,
      orderLocationError,
      loadOrderLocationOptions,
      ordersDemoMsg,
      ordersDemoError,
      seedDemoOrders,
      notifications,
      notifError,
      loadNotifications,
      markNotificationRead,
      importResult,
      importError,
      handleImportFiles,
      stores,
      storesError,
      loadStores,
      setStoreApproval,
      regEmail,
      regPassword,
      regBusinessName,
      regAddressLine,
      regCity,
      regMobile,
      regContactName,
      regLicenseNumber,
      registerError,
      registerMsg,
      registerStore,
      busy,
      canUseCart,
      isAdmin,
      refreshCartClick,
      stockMsg,
      stockError,
      adjustStock,
      catalogDemoMsg,
      catalogDemoError,
      seedDemoCatalog,
      storesDemoMsg,
      storesDemoError,
      seedDemoStores,
    ],
  )

  return (
    <MediProContext.Provider value={value}>{children}</MediProContext.Provider>
  )
}
