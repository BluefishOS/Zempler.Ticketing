export interface ITicketDto {
  id: string;
  eventId: string;
  seatNumber: number;
  price: number;
  status: 'Available' | 'Reserved' | 'Sold';
  reservedUntil?: string | null;
}

export interface IEventInfoDto {
  id: string;
  name: string;
  date: string;
  totalTickets: number;
  availableTickets: number;
  reservedTickets: number;
  soldTickets: number;
}

export interface IEventDto extends IEventInfoDto {
  tickets: ITicketDto[];
}