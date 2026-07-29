import { getEvents } from '@/lib/api';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import Link from 'next/link';
import { CalendarDays, Ticket, ArrowRight } from 'lucide-react';

export default async function EventsPage() {
  let events = [];
  try {
    events = await getEvents();
  } catch (error) {
    return (
      <div className="flex h-screen items-center justify-center p-4">
        <div className="bg-red-50 border border-red-200 text-red-700 px-6 py-4 rounded-xl shadow-sm text-center">
          <p className="font-semibold">Could not connect to the ticketing system.</p>
          <p className="text-sm text-red-500 mt-1">Please try again later.</p>
        </div>
      </div>
    );
  }

  return (
    <main className="container mx-auto py-4 px-4 max-w-7xl">      
      <p className="text-muted-foreground text-lg mb-4">
        Explore upcoming active events and secure your seats instantly.
      </p>     

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {events.map((event: any) => {
          const totalCapacity = event.availableTickets + event.reservedTickets + event.soldTickets;

          return (
            <Link 
              key={event.id} 
              href={`/events/${event.id}`}
              className="group block focus:outline-none"
            >
              <Card className="h-full flex flex-col justify-between border transition-all duration-300 hover:shadow-xl hover:border-primary/50 group-hover:-translate-y-1 bg-card/50 backdrop-blur-sm">
                <CardHeader className="space-y-3">
                  <div className="flex justify-between items-start gap-2">
                    <CardTitle className="text-xl font-bold group-hover:text-primary transition-colors line-clamp-1">
                      {event.name}
                    </CardTitle>
                    <Badge variant="outline" className="shrink-0 flex items-center gap-1 font-normal text-xs py-1">
                      <CalendarDays className="h-3.5 w-3.5 text-muted-foreground" />
                      {new Date(event.date).toLocaleDateString()}
                    </Badge>
                  </div>
                  <CardDescription className="flex items-center gap-1.5 text-xs text-muted-foreground">
                    <Ticket className="h-3.5 w-3.5" />
                    Total Capacity: {totalCapacity} seats
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="flex flex-wrap gap-2 text-xs">
                    <Badge variant="secondary" className="bg-green-50 text-green-700 border-green-200 shadow-none">
                      Available: {event.availableTickets}
                    </Badge>
                    <Badge variant="secondary" className="bg-yellow-50 text-yellow-700 border-yellow-200 shadow-none">
                      Reserved: {event.reservedTickets}
                    </Badge>
                    <Badge variant="secondary" className="bg-red-50 text-red-700 border-red-200 shadow-none">
                      Sold: {event.soldTickets}
                    </Badge>
                  </div>
                  <div className="flex items-center justify-between pt-2 text-sm font-medium text-primary opacity-0 group-hover:opacity-100 transition-opacity">
                    <span>View & Select Seats</span>
                    <ArrowRight className="h-4 w-4 transform group-hover:translate-x-1 transition-transform" />
                  </div>
                </CardContent>
              </Card>
            </Link>
          );
        })}
      </div>
    </main>
  );
}