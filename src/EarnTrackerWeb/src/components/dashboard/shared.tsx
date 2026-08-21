import type { Transaction } from "@/lib/api";

export type SourcedTransaction = Transaction & { source: string };

export const money = (amount: number, currency = "USD") => new Intl.NumberFormat("en-US", { style: "currency", currency, maximumFractionDigits: 0 }).format(amount);

export function CardHead({ title, action }: { title: string; action: string }) {
  return <div className="card-head"><h3>{title}</h3><button>{action}</button></div>;
}

export function EarningsChart() {
  return <div className="chart"><div className="axis"><span>$3k</span><span>$2k</span><span>$1k</span><span>$0</span></div><svg viewBox="0 0 680 190" preserveAspectRatio="none" aria-label="Earnings chart"><defs><linearGradient id="area" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stopColor="#7055e8" stopOpacity=".28"/><stop offset="1" stopColor="#7055e8" stopOpacity="0"/></linearGradient></defs><path className="gridline" d="M0 20H680M0 70H680M0 120H680M0 170H680"/><path className="area" d="M0 145 C65 130 90 106 135 115 S220 145 270 95 S350 82 405 65 S490 85 540 45 S620 55 680 20 V190 H0Z"/><path className="line" d="M0 145 C65 130 90 106 135 115 S220 145 270 95 S350 82 405 65 S490 85 540 45 S620 55 680 20"/><path className="fee-line" d="M0 168 C90 158 160 166 230 152 S370 157 450 140 S570 150 680 130"/></svg><div className="months"><span>Mar</span><span>Apr</span><span>May</span><span>Jun</span><span>Jul</span><span>Aug</span></div></div>;
}

export function TransactionTable({ transactions }: { transactions: SourcedTransaction[] }) {
  return <div className="table-wrap"><table><thead><tr><th>Payment</th><th>Source</th><th>Date</th><th>Status</th><th>Amount</th></tr></thead><tbody>{transactions.length ? transactions.map((item) => <tr key={item.id}><td><span className="payment-icon">{item.source.slice(0, 1)}</span><span><strong>{item.description || "Payment received"}</strong><small>{item.externalId}</small></span></td><td>{item.source}</td><td>{new Intl.DateTimeFormat("en-GB", { day: "numeric", month: "short", year: "numeric" }).format(new Date(item.occurredAt))}</td><td><span className="status">● {item.status}</span></td><td><strong>{money(item.amount - item.fee, item.currency)}</strong><small className="fee">Fee {money(item.fee, item.currency)}</small></td></tr>) : <tr><td colSpan={5} className="empty">No payments yet.</td></tr>}</tbody></table></div>;
}
