'use client';

import { useState } from 'react';
import { reserveTicket, purchaseTicket } from '@/lib/api';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter } from '@/components/ui/dialog';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { ArrowLeft, CheckCircle2, AlertCircle } from 'lucide-react';
import { IEventDto, ITicketDto } from '@/types/ticketing';

type DialogStep = 'details' | 'payment' | 'success';

export default function EventDetailClient({ event: initialEvent }: { event: IEventDto }) {
  const [event, setEvent] = useState<IEventDto>(initialEvent);
  const [selectedTicket, setSelectedTicket] = useState<ITicketDto | null>(null);
  const [dialogStep, setDialogStep] = useState<DialogStep>('details');
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  
  // Local state map to maintain holder names locally by ticket ID since API does not return it
  const [ticketHolders, setTicketHolders] = useState<Record<string, string>>({});

  // Form States
  const [holderName, setHolderName] = useState('');
  const [cardNumber, setCardNumber] = useState('');
  const [expiry, setExpiry] = useState('');
  const [cvv, setCvv] = useState('');

  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const router = useRouter();

  const handleOpenModal = (ticket: ITicketDto) => {
    if (ticket.status === 'Sold' || (ticket.status === "Reserved" && !holderName)) return; // Do nothing if sold

    setSelectedTicket(ticket);
    // Retrieve locally stored holder name if available
    setHolderName(ticketHolders[ticket.id] || '');
    setCardNumber('');
    setExpiry('');
    setCvv('');
    setErrorMessage(null);

    // If already reserved, skip name entry and go straight to payment
    if (ticket.status === 'Reserved') {
      setDialogStep('payment');
    } else {
      setDialogStep('details');
    }

    setIsDialogOpen(true);
  };

  const handleReserveAndNext = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedTicket) return;
    if (!holderName.trim()) {
      setErrorMessage('Full name is required.');
      return;
    }

    // Save holder name locally
    setTicketHolders(prev => ({ ...prev, [selectedTicket.id]: holderName }));

    // If the ticket is already reserved, skip calling reserveTicket again to prevent errors
    if (selectedTicket.status === 'Reserved') {
      setDialogStep('payment');
      return;
    }

    setLoading(true);
    setErrorMessage(null);

    try {
      const updatedTicket = await reserveTicket(event.id, selectedTicket.id, holderName);
      
      // Update local state
      const updatedTickets = event.tickets.map(t => t.id === updatedTicket.id ? updatedTicket : t);
      updateEventState(updatedTickets);
      setSelectedTicket(updatedTicket);

      // Move to payment step
      setDialogStep('payment');
    } catch (err: any) {
      setErrorMessage(err.message || 'Failed to reserve seat. It may have just been taken.');
    } finally {
      setLoading(false);
    }
  };

  const handlePayAndComplete = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedTicket) return;
    if (!cardNumber.trim() || !expiry.trim() || !cvv.trim()) {
      setErrorMessage('Please fill out all payment details.');
      return;
    }

    const currentHolder = holderName || ticketHolders[selectedTicket.id] || '';

    setLoading(true);
    setErrorMessage(null);

    try {
      const updatedTicket = await purchaseTicket(event.id, selectedTicket.id, currentHolder);
      
      // Update local state
      const updatedTickets = event.tickets.map(t => t.id === updatedTicket.id ? updatedTicket : t);
      updateEventState(updatedTickets);

      setDialogStep('success');

      setTimeout(() => {
        setIsDialogOpen(false);
        router.refresh();
      }, 2000);
    } catch (err: any) {
      setErrorMessage(err.message || 'Payment failed or ticket expired.');
    } finally {
      setLoading(false);
    }
  };

  const updateEventState = (updatedTickets: ITicketDto[]) => {
    const available = updatedTickets.filter(t => t.status === 'Available').length;
    const reserved = updatedTickets.filter(t => t.status === 'Reserved').length;
    const sold = updatedTickets.filter(t => t.status === 'Sold').length;

    setEvent(prev => ({
      ...prev,
      availableTickets: available,
      reservedTickets: reserved,
      soldTickets: sold,
      tickets: updatedTickets
    }));
  };

  const currentHolder = selectedTicket ? (holderName || ticketHolders[selectedTicket.id] || '') : '';

  return (
    <main className="container mx-auto py-4 px-4 max-w-7xl">
      <div className="mb-6">
        <Button variant="ghost" asChild className="mb-4 pl-0">
          <Link href="/">
            <ArrowLeft className="mr-2 h-4 w-4" /> Back to Events
          </Link>
        </Button>
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div>
            <h1 className="text-3xl font-extrabold tracking-tight">{event.name}</h1>
            <p className="text-muted-foreground mt-1">Date: {new Date(event.date).toLocaleDateString()}</p>
          </div>
          <div className="flex gap-2 flex-wrap">
            <Badge variant="secondary" className="bg-green-100 text-green-800">
              Available: {event.availableTickets}
            </Badge>
            <Badge variant="secondary" className="bg-yellow-100 text-yellow-800">
              Reserved: {event.reservedTickets}
            </Badge>
            <Badge variant="secondary" className="bg-red-100 text-red-800">
              Sold: {event.soldTickets}
            </Badge>
          </div>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Seat Selection</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
            {event.tickets.map((ticket) => {
              const isAvailable = ticket.status === 'Available';
              const isReserved = ticket.status === 'Reserved';
              const isSold = ticket.status === 'Sold';
              const isReservedBySomeoneElse = isReserved && !holderName;

              return (
                <div
                  key={ticket.id}
                  onClick={() => handleOpenModal(ticket)}
                  className={`border rounded-lg p-4 flex flex-col items-center justify-center transition-all ${
                    isSold || isReservedBySomeoneElse
                      ? 'bg-muted/50 border-muted opacity-60 cursor-not-allowed'
                      : 'cursor-pointer hover:shadow-md ' + (
                        isAvailable
                          ? 'bg-green-50 border-green-200 hover:bg-green-100'
                          : 'bg-yellow-50 border-yellow-200 hover:bg-yellow-100'
                      )
                  }`}
                >
                  <span className="font-bold text-lg">Seat - {ticket.seatNumber}</span>
                  { ticket.status !== "Sold" && (<span className="text-xs text-muted-foreground mt-1">£{ticket.price.toFixed(2)}</span>)}
                  <Badge 
                    variant="outline" 
                    className={`mt-2 text-[10px] ${
                      isAvailable ? 'text-green-700 border-green-300' :
                      isReserved ? 'text-yellow-700 border-yellow-300' :
                      'text-red-700 border-red-300'
                    }`}
                  >
                    {ticket.status}
                  </Badge>
                </div>
              );
            })}
          </div>
        </CardContent>
      </Card>

      {/* Multi-step Booking Dialog Modal */}
      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Seat - {selectedTicket?.seatNumber}</DialogTitle>
            <DialogDescription>
              Price: £{selectedTicket?.price.toFixed(2)}
            </DialogDescription>
          </DialogHeader>

          {errorMessage && (
            <div className="bg-red-50 border border-red-200 text-red-700 p-3 rounded-md text-sm flex items-center gap-2">
              <AlertCircle className="h-4 w-4 shrink-0" />
              <span>{errorMessage}</span>
            </div>
          )}

          {/* STEP 1: HOLDER NAME */}
          {dialogStep === 'details' && (
            <form onSubmit={handleReserveAndNext} className="space-y-4 py-2">
              <div className="space-y-2">
                <Label htmlFor="holderName">Full Name</Label>
                <Input
                  id="holderName"
                  placeholder="Enter your name"
                  value={holderName}
                  onChange={(e) => setHolderName(e.target.value)}
                  disabled={loading}
                  required
                />
              </div>
              <DialogFooter className="pt-2">
                <Button type="submit" disabled={loading} className="w-full">
                  {loading ? 'Reserving Seat...' : 'Proceed to Payment'}
                </Button>
              </DialogFooter>
            </form>
          )}

          {/* STEP 2: PAYMENT PAGE */}
          {dialogStep === 'payment' && (
            <form onSubmit={handlePayAndComplete} className="space-y-4 py-2">
              <div className="bg-muted/50 p-3 rounded-lg text-sm space-y-1">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Holder:</span>
                  <span className="font-semibold">{currentHolder}</span>
                </div>
                <div className="flex justify-between font-bold text-primary pt-1 border-t">
                  <span>Total Due:</span>
                  <span>£{selectedTicket?.price.toFixed(2)}</span>
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="cardNumber">Card Number</Label>
                <Input
                  id="cardNumber"
                  placeholder="4242 4242 4242 4242"
                  value={cardNumber}
                  onChange={(e) => setCardNumber(e.target.value)}
                  maxLength={19}
                  required
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div className="space-y-2">
                  <Label htmlFor="expiry">Expiry</Label>
                  <Input
                    id="expiry"
                    placeholder="MM/YY"
                    value={expiry}
                    onChange={(e) => setExpiry(e.target.value)}
                    maxLength={5}
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="cvv">CVV</Label>
                  <Input
                    id="cvv"
                    placeholder="123"
                    value={cvv}
                    onChange={(e) => setCvv(e.target.value)}
                    maxLength={4}
                    required
                  />
                </div>
              </div>

              <DialogFooter className="pt-2 flex gap-2">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => setDialogStep('details')}
                  disabled={loading}
                  className="flex-1"
                >
                  Back
                </Button>
                <Button type="submit" disabled={loading} className="flex-1">
                  {loading ? 'Processing Payment...' : `Pay £${selectedTicket?.price.toFixed(2)}`}
                </Button>
              </DialogFooter>
            </form>
          )}

          {/* STEP 3: SUCCESS */}
          {dialogStep === 'success' && (
            <div className="py-6 text-center space-y-4">
              <div className="mx-auto bg-green-100 text-green-700 p-3 rounded-full w-fit">
                <CheckCircle2 className="h-10 w-10" />
              </div>
              <div>
                <h3 className="text-lg font-bold">Booking Confirmed!</h3>
                <p className="text-sm text-muted-foreground mt-1">
                  Seat - {selectedTicket?.seatNumber} successfully purchased for {currentHolder}.
                </p>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </main>
  );
}