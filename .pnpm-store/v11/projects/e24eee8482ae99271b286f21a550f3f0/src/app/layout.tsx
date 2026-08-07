import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "EarnTracker — Your earnings, clearly",
  description: "See payments, fees, income sources and goals in one simple dashboard.",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="en" className="h-full antialiased">
      <body className="min-h-full flex flex-col">{children}</body>
    </html>
  );
}
