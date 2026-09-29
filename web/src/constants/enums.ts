import type { components } from '../types/api'

export const BillStatus = {
  Open: 1,
  Closed: 2,
  Cancelled: 3,
  Closing: 4,
} as const satisfies Record<string, components['schemas']['BillStatus']>

export const BillStatusLabels: Record<components['schemas']['BillStatus'], string> = {
  [BillStatus.Open]: 'Aberta',
  [BillStatus.Closed]: 'Fechada',
  [BillStatus.Cancelled]: 'Cancelada',
  [BillStatus.Closing]: 'Conta Solicitada',
}

export const TableStatus = {
  Available: 1,
  Occupied: 2,
  Reserved: 3,
  Closing: 4,
} as const satisfies Record<string, components['schemas']['TableStatus']>

export const TableStatusLabels: Record<components['schemas']['TableStatus'], string> = {
  [TableStatus.Available]: 'Disponível',
  [TableStatus.Occupied]: 'Ocupada',
  [TableStatus.Reserved]: 'Reservada',
  [TableStatus.Closing]: 'Conta Solicitada',
}

export const TableType = {
  DiningTable: 1,
  Counter: 2,
} as const satisfies Record<string, components['schemas']['TableType']>

export const TableTypeLabels: Record<components['schemas']['TableType'], string> = {
  [TableType.DiningTable]: 'Mesa',
  [TableType.Counter]: 'Balcão',
}
