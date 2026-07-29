import type { Metadata } from 'main';
import { Inter } from 'next/font/google';
import Header from '@/components/Header';
import './globals.css';

const inter = Inter({ subsets: ['latin'] });

export const metadata: Metadata = {
  title: 'Zempler Ticketing',
  description: 'Browse and book event tickets easily.',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body className={`${inter.className} min-h-screen bg-background flex flex-col antialiased`}>
        <Header />
        <div className="flex-1 flex flex-col ">
          {children}
        </div>
      </body>
    </html>
  );
}