import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "EarnTracker — Know what you earned",
  description: "Bring payments, fees, income sources, and financial goals into one clear view.",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="en" className="h-full antialiased">
      <body className="min-h-full flex flex-col">{children}</body>
    </html>
  );
}
