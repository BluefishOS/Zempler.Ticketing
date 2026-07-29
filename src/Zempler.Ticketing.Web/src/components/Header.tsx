import Link from 'next/link';
import { Ticket } from 'lucide-react';

export default function Header() {
  return (
    <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="container mx-auto px-4 h-16 flex items-center justify-between max-w-7xl">
        <Link href="/" className="flex items-center gap-2 group">
          <div className="bg-primary/10 p-2 rounded-xl group-hover:bg-primary/20 transition-colors">
            <Ticket className="h-5 w-5 text-primary" />
          </div>
          <span className="font-bold text-lg tracking-tight bg-gradient-to-r from-primary to-primary/70 bg-clip-text text-transparent">
            Zempler Ticketing
          </span>
        </Link>
        <nav className="flex items-center gap-6">
          <Link 
            href="/" 
            className="text-sm font-medium text-foreground transition-colors hover:text-primary"
          >
            Events
          </Link>
          <div className="flex items-center pl-4 border-l">
            <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center text-xs font-semibold text-primary">
              ZT
            </div>
          </div>
        </nav>
      </div>
    </header>
  );
}