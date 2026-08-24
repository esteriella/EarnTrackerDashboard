import type { ReactNode } from "react";
import type { Transaction } from "@/lib/api";

export type SourcedTransaction = Transaction & { source: string };
export type ChartPoint = { label: string; net: number; fees: number };

export const money = (amount: number, currency = "USD") => new Intl.NumberFormat("en-US", { style: "currency", currency, maximumFractionDigits: 0 }).format(amount);

export function CardHead({ title, action, onAction }: { title: string; action: ReactNode; onAction?: () => void }) {
  return <div className="card-head"><h3>{title}</h3>{onAction ? <button onClick={onAction}>{action}</button> : action}</div>;
}

export function EarningsChart({ points, currency }: { points: ChartPoint[]; currency: string }) {
  const width = 680;
  const height = 170;
  const highest = Math.max(1, ...points.flatMap((point) => [point.net, point.fees]));
  const coordinates = (key: "net" | "fees") => points.map((point, index) => {
    const x = points.length === 1 ? width / 2 : index * width / (points.length - 1);
    const y = height - point[key] / highest * (height - 12);
    return `${x.toFixed(1)},${y.toFixed(1)}`;
  });
  const netPoints = coordinates("net");
  const feePoints = coordinates("fees");
  const area = netPoints.length ? `M${netPoints.join(" L")} L${width},${height} L0,${height} Z` : "";
  const axis = [highest, highest * 2 / 3, highest / 3, 0];
  const compact = new Intl.NumberFormat("en-US", { notation: "compact", maximumFractionDigits: 1 });

  return <div className="chart"><div className="axis">{axis.map((value, index) => <span key={index}>{compact.format(value)}</span>)}</div><svg viewBox={`0 0 ${width} 190`} preserveAspectRatio="none" aria-label={`${currency} earnings chart`}><defs><linearGradient id="earnings-area" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stopColor="#7055e8" stopOpacity=".28"/><stop offset="1" stopColor="#7055e8" stopOpacity="0"/></linearGradient></defs><path className="gridline" d="M0 12H680M0 64H680M0 117H680M0 170H680"/>{area && <path className="area" d={area}/>}<polyline className="line" points={netPoints.join(" ")}/><polyline className="fee-line" points={feePoints.join(" ")}/>{netPoints.map((point,index) => { const [cx,cy] = point.split(","); return <circle className="chart-point" key={index} cx={cx} cy={cy} r="3"/>; })}</svg><div className="months">{points.map((point,index) => <span key={`${point.label}-${index}`}>{point.label}</span>)}</div></div>;
}

export function TransactionTable({ transactions }: { transactions: SourcedTransaction[] }) {
  return <div className="table-wrap"><table><thead><tr><th>Payment</th><th>Source</th><th>Date</th><th>Status</th><th>Amount</th></tr></thead><tbody>{transactions.length ? transactions.map((item) => <tr key={item.id}><td><span className="payment-icon">{item.source.slice(0, 1)}</span><span><strong>{item.description || "Payment received"}</strong><small>{item.externalId}</small></span></td><td>{item.source}</td><td>{new Intl.DateTimeFormat("en-GB", { day: "numeric", month: "short", year: "numeric" }).format(new Date(item.occurredAt))}</td><td><span className="status">● {item.status}</span></td><td><strong>{money(item.amount - item.fee, item.currency)}</strong><small className="fee">Fee {money(item.fee, item.currency)}</small></td></tr>) : <tr><td colSpan={5} className="empty">No payments yet.</td></tr>}</tbody></table></div>;
}
