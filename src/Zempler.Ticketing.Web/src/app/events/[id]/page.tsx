import { getEventById } from '@/lib/api';
import { notFound } from 'next/navigation';
import EventDetailClient from './event-detail-client';

interface PageProps {
  params: Promise<{ id: string }>;
}

export default async function EventDetailPage({ params }: PageProps) {
  const resolvedParams = await params;

  let event;
  try {
    event = await getEventById(resolvedParams.id);
  } catch (error) {
    notFound();
  }

  return <EventDetailClient event={event} />;
}