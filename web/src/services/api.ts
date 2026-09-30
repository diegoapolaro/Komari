import type { components } from '../types/api'

export type CategoryResponse = components['schemas']['CategoryResponse']
export type CreateCategoryRequest = components['schemas']['CreateCategoryRequest']
export type UpdateCategoryRequest = components['schemas']['UpdateCategoryRequest']

export type ProductResponse = components['schemas']['ProductResponse']
export type CreateProductRequest = components['schemas']['CreateProductRequest']
export type UpdateProductRequest = components['schemas']['UpdateProductRequest']
export type UpdateProductAvailabilityRequest = components['schemas']['UpdateProductAvailabilityRequest']

export type TableResponse = components['schemas']['TableResponse']
export type CreateTableRequest = components['schemas']['CreateTableRequest']
export type UpdateTableRequest = components['schemas']['UpdateTableRequest']
export type UpdateTableStatusRequest = components['schemas']['UpdateTableStatusRequest']
export type InitializeTablesRequest = components['schemas']['InitializeTablesRequest']
export type TableStatusType = components['schemas']['TableStatus']
export type TableTypeType = components['schemas']['TableType']

export type BillResponse = components['schemas']['BillResponse']
export type OpenBillRequest = components['schemas']['OpenBillRequest']
export type CloseBillRequest = components['schemas']['CloseBillRequest']
export type CancelBillRequest = components['schemas']['CancelBillRequest']
export type UpdateBillDetailsRequest = components['schemas']['UpdateBillDetailsRequest']
export type BillStatusType = components['schemas']['BillStatus']

export type OrderResponse = components['schemas']['OrderResponse']
export type OrderItemResponse = components['schemas']['OrderItemResponse']
export type CreateOrderRequest = components['schemas']['CreateOrderRequest']
export type CreateOrderItemRequest = components['schemas']['CreateOrderItemRequest']
export type OrderStatusType = components['schemas']['OrderStatus']

export type CustomerResponse = components['schemas']['CustomerResponse']
export type CreateCustomerRequest = components['schemas']['CreateCustomerRequest']
export type UpdateCustomerRequest = components['schemas']['UpdateCustomerRequest']

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5105'

export interface ApiError {
  title?: string
  status?: number
  detail?: string
  errors?: Record<string, string[]>
}

async function request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const url = `${API_BASE_URL}${endpoint}`
  const response = await fetch(url, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
      ...options.headers,
    },
  })

  if (!response.ok) {
    let errorDetail = `Erro HTTP ${response.status}: ${response.statusText}`
    try {
      const errorJson = (await response.json()) as ApiError
      if (errorJson.detail) {
        errorDetail = errorJson.detail
      } else if (errorJson.title) {
        errorDetail = errorJson.title
      }
      if (errorJson.errors) {
        const fieldErrors = Object.entries(errorJson.errors)
          .map(([field, msgs]) => `${field}: ${msgs.join(', ')}`)
          .join(' | ')
        errorDetail += ` (${fieldErrors})`
      }
    } catch {
      // Se não for JSON, mantém o texto padrão
    }
    throw new Error(errorDetail)
  }

  if (response.status === 204) {
    return {} as T
  }

  return response.json() as Promise<T>
}

export const api = {
  // Health
  checkHealth: () => request<{ status: string; timestamp: string }>('/'),

  // Categories
  getCategories: () => request<CategoryResponse[]>('/api/v1/Categories'),
  getCategoryById: (id: string) => request<CategoryResponse>(`/api/v1/Categories/${id}`),
  createCategory: (data: CreateCategoryRequest) =>
    request<CategoryResponse>('/api/v1/Categories', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  updateCategory: (id: string, data: UpdateCategoryRequest) =>
    request<CategoryResponse>(`/api/v1/Categories/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),
  deleteCategory: (id: string) =>
    request<void>(`/api/v1/Categories/${id}`, {
      method: 'DELETE',
    }),

  // Products
  getProducts: (categoryId?: string) => {
    const query = categoryId ? `?categoryId=${encodeURIComponent(categoryId)}` : ''
    return request<ProductResponse[]>(`/api/v1/Products${query}`)
  },
  getProductById: (id: string) => request<ProductResponse>(`/api/v1/Products/${id}`),
  createProduct: (data: CreateProductRequest) =>
    request<ProductResponse>('/api/v1/Products', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  updateProduct: (id: string, data: UpdateProductRequest) =>
    request<ProductResponse>(`/api/v1/Products/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),
  updateProductAvailability: (id: string, data: UpdateProductAvailabilityRequest) =>
    request<ProductResponse>(`/api/v1/Products/${id}/availability`, {
      method: 'PATCH',
      body: JSON.stringify(data),
    }),
  deleteProduct: (id: string) =>
    request<void>(`/api/v1/Products/${id}`, {
      method: 'DELETE',
    }),

  // Tables
  getTables: (params?: { status?: TableStatusType; type?: TableTypeType; includeInactive?: boolean }) => {
    const searchParams = new URLSearchParams()
    if (params?.status !== undefined) searchParams.append('status', params.status.toString())
    if (params?.type !== undefined) searchParams.append('type', params.type.toString())
    if (params?.includeInactive !== undefined) searchParams.append('includeInactive', params.includeInactive.toString())
    const query = searchParams.toString() ? `?${searchParams.toString()}` : ''
    return request<TableResponse[]>(`/api/v1/Tables${query}`)
  },
  getTableById: (id: string) => request<TableResponse>(`/api/v1/Tables/${id}`),
  createTable: (data: CreateTableRequest) =>
    request<TableResponse>('/api/v1/Tables', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  updateTable: (id: string, data: UpdateTableRequest) =>
    request<TableResponse>(`/api/v1/Tables/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),
  updateTableStatus: (id: string, data: UpdateTableStatusRequest) =>
    request<TableResponse>(`/api/v1/Tables/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify(data),
    }),
  initializeTables: (data: InitializeTablesRequest) =>
    request<TableResponse[]>('/api/v1/Tables/initialize', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  deleteTable: (id: string) =>
    request<void>(`/api/v1/Tables/${id}`, {
      method: 'DELETE',
    }),

  // Bills
  getBills: (params?: { status?: BillStatusType; tableId?: string; includeInactive?: boolean }) => {
    const searchParams = new URLSearchParams()
    if (params?.status !== undefined) searchParams.append('status', params.status.toString())
    if (params?.tableId) searchParams.append('tableId', params.tableId)
    if (params?.includeInactive !== undefined) searchParams.append('includeInactive', params.includeInactive.toString())
    const query = searchParams.toString() ? `?${searchParams.toString()}` : ''
    return request<BillResponse[]>(`/api/v1/Bills${query}`)
  },
  getBillById: (id: string) => request<BillResponse>(`/api/v1/Bills/${id}`),
  getBillByNumber: (num: number) => request<BillResponse>(`/api/v1/Bills/number/${num}`),
  openBill: (data: OpenBillRequest) =>
    request<BillResponse>('/api/v1/Bills', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  requestBillClosing: (id: string) =>
    request<BillResponse>(`/api/v1/Bills/${id}/request-closing`, {
      method: 'POST',
    }),
  reopenBill: (id: string) =>
    request<BillResponse>(`/api/v1/Bills/${id}/reopen`, {
      method: 'POST',
    }),
  updateBillDetails: (id: string, data: UpdateBillDetailsRequest) =>
    request<BillResponse>(`/api/v1/Bills/${id}/details`, {
      method: 'PATCH',
      body: JSON.stringify(data),
    }),
  closeBill: (id: string, data: CloseBillRequest = {}) =>
    request<BillResponse>(`/api/v1/Bills/${id}/close`, {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  cancelBill: (id: string, data: CancelBillRequest) =>
    request<BillResponse>(`/api/v1/Bills/${id}/cancel`, {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  // Orders
  getOrders: (params?: { billId?: string; status?: OrderStatusType; includeInactive?: boolean }) => {
    const searchParams = new URLSearchParams()
    if (params?.billId) searchParams.append('billId', params.billId)
    if (params?.status !== undefined) searchParams.append('status', params.status.toString())
    if (params?.includeInactive !== undefined) searchParams.append('includeInactive', params.includeInactive.toString())
    const query = searchParams.toString() ? `?${searchParams.toString()}` : ''
    return request<OrderResponse[]>(`/api/v1/Orders${query}`)
  },
  getOrderById: (id: string) => request<OrderResponse>(`/api/v1/Orders/${id}`),
  getOrdersByBillId: (billId: string) => request<OrderResponse[]>(`/api/v1/Orders/bill/${billId}`),
  createOrder: (data: CreateOrderRequest) =>
    request<OrderResponse>('/api/v1/Orders', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  updateOrderStatus: (id: string, data: { status: OrderStatusType }) =>
    request<OrderResponse>(`/api/v1/Orders/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify(data),
    }),

  // Customers
  getCustomers: (params?: { searchTerm?: string; includeInactive?: boolean }) => {
    const searchParams = new URLSearchParams()
    if (params?.searchTerm) searchParams.append('searchTerm', params.searchTerm)
    if (params?.includeInactive !== undefined) searchParams.append('includeInactive', params.includeInactive.toString())
    const query = searchParams.toString() ? `?${searchParams.toString()}` : ''
    return request<CustomerResponse[]>(`/api/v1/Customers${query}`)
  },
  getCustomerById: (id: string) => request<CustomerResponse>(`/api/v1/Customers/${id}`),
  getCustomerByPhone: (phone: string) =>
    request<CustomerResponse>(`/api/v1/Customers/by-phone/${encodeURIComponent(phone)}`),
  createCustomer: (data: CreateCustomerRequest) =>
    request<CustomerResponse>('/api/v1/Customers', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  updateCustomer: (id: string, data: UpdateCustomerRequest) =>
    request<CustomerResponse>(`/api/v1/Customers/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),
  deleteCustomer: (id: string) =>
    request<void>(`/api/v1/Customers/${id}`, {
      method: 'DELETE',
    }),
}
