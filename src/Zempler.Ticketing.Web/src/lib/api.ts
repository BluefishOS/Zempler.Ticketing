import { IEventDto, IEventInfoDto, ITicketDto } from '@/types/ticketing';

const API_BASE = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:5001/api';

export async function getEvents(): Promise<IEventInfoDto[]> {
  const res = await fetch(`${API_BASE}/events`, { cache: 'no-store' });
  if (!res.ok) throw new Error('Failed to fetch events');
  return res.json();
}

export async function getEventById(id: string): Promise<IEventDto> {
  const res = await fetch(`${API_BASE}/events/${id}`, { cache: 'no-store' });
  if (!res.ok) throw new Error('Failed to fetch event details');
  return res.json();
}

export async function reserveTicket(eventId: string, ticketId: string, holderName: string): Promise<ITicketDto> {
  const res = await fetch(`${API_BASE}/events/${eventId}/tickets/${ticketId}/reserve`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ holderName }),
  });
  if (!res.ok) {
    const error = await res.json();
    throw new Error(error.detail || 'Failed to reserve ticket');
  }
  return res.json();
}

export async function purchaseTicket(eventId: string, ticketId: string, holderName: string): Promise<ITicketDto> {
  const res = await fetch(`${API_BASE}/events/${eventId}/tickets/${ticketId}/purchase`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ holderName }),
  });
  if (!res.ok) {
    const error = await res.json();
    throw new Error(error.detail || 'Failed to purchase ticket');
  }
  return res.json();
}