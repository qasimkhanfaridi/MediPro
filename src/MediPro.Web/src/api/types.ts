export type HealthResponse = {
  status: string
  service: string
  timestampUtc: string
}

export type AuthResponse = {
  accessToken: string
  expiresAtUtc: string
  role: string
  storeApprovalStatus: string | null
}

export type ProductDto = {
  id: string
  skuCode: string
  name: string
  pack: string
  manufacturer: string
  saltComposition: string
  category: string | null
  tradePrice: number
  mrp: number | null
  isActive: boolean
  stockQuantity: number | null
  inStock?: boolean
  bonusLabel?: string | null
  bonusTitle?: string | null
  bonusBuyQuantity?: number | null
  bonusFreeQuantity?: number | null
  bonusBannerText?: string | null
}

export type PagedProducts = {
  page: number
  pageSize: number
  totalCount: number
  items: ProductDto[]
}

export type CatalogFilterOptions = {
  manufacturers: string[]
  categories: string[]
}

export type LowStockItem = {
  skuCode: string
  name: string
  stockQuantity: number
}

export type LowStockList = {
  threshold: number
  totalMatching: number
  items: LowStockItem[]
}

export type CartLineDto = {
  lineId: string
  productId: string
  skuCode: string
  name: string
  pack: string
  tradePrice: number
  quantity: number
  lineTotal: number
}

export type CartDto = {
  cartId: string
  lines: CartLineDto[]
  subtotal: number
}

export type OrderSummaryDto = {
  id: string
  storeId: string
  storeName: string
  storeCity: string
  storeArea: string
  storeMobile: string
  status: string
  totalAmount: number
  currency: string
  submittedAtUtc: string
}

export type OrderLocationOptions = {
  cities: string[]
  areasByCity: Record<string, string[]>
}

export type BonusSchemeDto = {
  id: string
  title: string
  label: string
  manufacturer: string | null
  productId: string | null
  productName: string | null
  buyQuantity: number
  bonusQuantity: number
  bannerText: string | null
  isActive: boolean
  sortOrder: number
  validFromUtc: string | null
  validToUtc: string | null
  createdAtUtc: string
}

export type BonusSchemeSummary = {
  id: string
  title: string
  label: string
  buyQuantity: number
  bonusQuantity: number
  manufacturer: string | null
  productId: string | null
  productName: string | null
  bannerText: string
  sortOrder: number
}

export type OrderLineDto = {
  productId: string
  productNameSnapshot: string
  packSnapshot: string
  unitPriceSnapshot: number
  quantity: number
  lineTotal: number
}

export type OrderDetailDto = {
  id: string
  storeId: string
  storeName: string
  status: string
  statusNotes: string | null
  totalAmount: number
  currency: string
  submittedAtUtc: string
  notes: string | null
  lines: OrderLineDto[]
}

export type PagedOrders = {
  page: number
  pageSize: number
  totalCount: number
  items: OrderSummaryDto[]
}

export type AdminNotificationDto = {
  id: string
  type: string
  title: string
  body: string
  relatedOrderId: string | null
  relatedStoreId: string | null
  createdAtUtc: string
  isRead: boolean
}

export type PagedNotifications = {
  page: number
  pageSize: number
  totalCount: number
  items: AdminNotificationDto[]
}

export type ImportProductsResult = {
  totalRowsAttempted: number
  insertedCount: number
  skippedOrFailedCount: number
  errors: { rowNumber: number; message: string }[]
}

export type StoreSummary = {
  id: string
  businessName: string
  city: string
  area: string
  mobile: string
  licenseNumber: string | null
  approvalStatus: string
  approvalNotes: string | null
  createdAtUtc: string
}
