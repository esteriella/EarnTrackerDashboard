"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { api, AuthSession, Overview, Transaction } from "@/lib/api";

const demoOverview: Overview = {
  totals: [{ currency: "USD", gross: 12450, fees: 608, net: 11842 }],
  incomeSources: [
    { id: "1", name: "Upwork", provider: "Upwork", currency: "USD", isActive: true, transactions: [
      { id: "t1", externalId: "INV-4821", amount: 2400, fee: 120, currency: "USD", status: "Completed", description: "Brand identity project", occurredAt: "2026-08-05T10:00:00Z" },
      { id: "t2", externalId: "INV-4818", amount: 1800, fee: 90, currency: "USD", status: "Completed", description: "Product design sprint", occurredAt: "2026-08-01T10:00:00Z" },
    ]},
    { id: "2", name: "PayPal", provider: "PayPal", currency: "USD", isActive: true, transactions: [
      { id: "t3", externalId: "PP-9130", amount: 950, fee: 28, currency: "USD", status: "Completed", description: "Website consultation", occurredAt: "2026-07-28T10:00:00Z" },
    ]},
    { id: "3", name: "Direct clients", provider: "Bank", currency: "USD", isActive: true, transactions: [
      { id: "t4", externalId: "BNK-207", amount: 3200, fee: 0, currency: "USD", status: "Completed", description: "Monthly retainer", occurredAt: "2026-07-24T10:00:00Z" },
    ]},
  ],
  financialGoals: [{ id: "g1", name: "August income goal", targetAmount: 15000, currentAmount: 11842, progressPercentage: 79, currency: "USD", startDate: "2026-08-01", targetDate: "2026-08-31" }],
};

const nav = ["Overview", "Earnings", "Transactions", "Goals", "Connections"];

function Icon({ name }: { name: string }) {
  const paths: Record<string, React.ReactNode> = {
    Overview: <><rect x="3" y="3" width="7" height="7" rx="2"/><rect x="14" y="3" width="7" height="7" rx="2"/><rect x="3" y="14" width="7" height="7" rx="2"/><rect x="14" y="14" width="7" height="7" rx="2"/></>,
    Earnings: <><path d="M4 19V9m6 10V5m6 14v-7m4 7V3"/></>,
    Transactions: <><path d="M7 7h13l-3-3m0 13H4l3 3"/></>, Goals: <><circle cx="12" cy="12" r="9"/><circle cx="12" cy="12" r="4"/><path d="m15 9 6-6"/></>,
    Connections: <><path d="M8 12h8m-1-5 5 5-5 5M9 7 4 12l5 5"/></>,
  };
  return <svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">{paths[name]}</svg>;
}

const money = (amount: number, currency = "USD") => new Intl.NumberFormat("en-US", { style: "currency", currency, maximumFractionDigits: 0 }).format(amount);

export function Dashboard() {
  const [session, setSession] = useState<AuthSession | null>(null);
  const [overview, setOverview] = useState<Overview>(demoOverview);
  const [active, setActive] = useState("Overview");
  const [authOpen, setAuthOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const [notice, setNotice] = useState("");
  const [verifyOpen, setVerifyOpen] = useState(false);

  useEffect(() => {
    const stored = localStorage.getItem("earntracker-session");
    if (!stored) return;
    try {
      const savedSession = JSON.parse(stored);
      queueMicrotask(() => setSession(savedSession));
    } catch { localStorage.removeItem("earntracker-session"); }
  }, []);

  useEffect(() => {
    if (!session) return;
    api.overview(session.token).then(setOverview).catch((error) => setNotice(error.message)).finally(() => setLoading(false));
  }, [session]);

  const transactions = useMemo(() => overview.incomeSources.flatMap((source) => source.transactions.map((item) => ({ ...item, source: source.name }))).sort((a, b) => +new Date(b.occurredAt) - +new Date(a.occurredAt)), [overview]);
  const total = overview.totals[0] ?? { currency: "USD", gross: 0, fees: 0, net: 0 };
  const goal = overview.financialGoals[0];
  const displayName = session?.name || "Opeyemi";

  function signOut() { localStorage.removeItem("earntracker-session"); setSession(null); setOverview(demoOverview); setNotice("You have signed out."); }

  return <div className="app-shell">
    <aside className="sidebar">
      <div className="brand"><span className="brand-mark">E</span><span>EarnTracker</span></div>
      <nav>{nav.map((item) => <button key={item} onClick={() => setActive(item)} className={active === item ? "active" : ""}><Icon name={item}/><span>{item}</span></button>)}</nav>
      <div className="sidebar-foot">
        <div className="help-card"><span>?</span><strong>Need a hand?</strong><p>Find quick answers and guides.</p><button>Visit help centre</button></div>
        <button className="profile" onClick={() => session ? signOut() : setAuthOpen(true)}><span className="avatar">{displayName.slice(0, 2).toUpperCase()}</span><span><strong>{displayName}</strong><small>{session ? "Sign out" : "Sign in to sync"}</small></span><b>⋮</b></button>
      </div>
    </aside>

    <main className="main">
      <header><div><button className="mobile-brand" aria-label="Menu">E</button><p>{session ? "Your workspace" : "Preview workspace"}</p><h1>{active}</h1></div><div className="header-actions"><button className="icon-button" aria-label="Notifications">♢<span /></button><button className="primary" onClick={() => session ? setVerifyOpen(true) : setAuthOpen(true)}>{session ? "+ Verify payment" : "Sign in"}</button></div></header>
      {notice && <div className="notice"><span>{notice}</span><button onClick={() => setNotice("")}>×</button></div>}

      {active === "Overview" && <>
        <section className="welcome"><div><p className="eyebrow">FRIDAY, 7 AUGUST</p><h2>Good evening, {displayName.split(" ")[0]} <span>✦</span></h2><p>Here&apos;s how your earnings are looking today.</p></div><div className="period"><button className="selected">This month</button><button>This year</button></div></section>
        <section className="stats">
          <Stat label="Net earnings" value={money(total.net, total.currency)} change="12.4%" good icon="↗" />
          <Stat label="Gross income" value={money(total.gross, total.currency)} change="8.2%" good icon="＋" />
          <Stat label="Fees paid" value={money(total.fees, total.currency)} change="2.1%" icon="−" />
          <Stat label="Active sources" value={String(overview.incomeSources.filter((s) => s.isActive).length)} sub="Across your accounts" icon="⌁" />
        </section>
        <section className="dashboard-grid">
          <div className="card chart-card"><CardHead title="Earnings flow" action="Last 6 months⌄"/><div className="chart-summary"><div><small>Total earned</small><strong>{money(total.gross, total.currency)}</strong></div><div className="legend"><span><i className="net-dot"/>Net earnings</span><span><i/>Fees</span></div></div><Chart /></div>
          <div className="card goal-card"><CardHead title="Monthly goal" action="View goals →"/><div className="goal-ring" style={{"--progress": `${goal?.progressPercentage ?? 0}%`} as React.CSSProperties}><div><strong>{Math.round(goal?.progressPercentage ?? 0)}%</strong><small>complete</small></div></div><h3>{goal?.name ?? "Set your first goal"}</h3><p><strong>{money(goal?.currentAmount ?? 0, goal?.currency)}</strong> of {money(goal?.targetAmount ?? 0, goal?.currency)}</p><div className="goal-note">✦ You&apos;re {money(Math.max(0, (goal?.targetAmount ?? 0) - (goal?.currentAmount ?? 0)), goal?.currency)} away</div></div>
        </section>
        <section className="card recent"><CardHead title="Recent transactions" action="View all →"/><TransactionTable transactions={transactions.slice(0, 5)}/></section>
      </>}

      {active !== "Overview" && <SectionView active={active} overview={overview} transactions={transactions} onVerify={() => session ? setVerifyOpen(true) : setAuthOpen(true)} />}
      {loading && <div className="loading">Updating your dashboard…</div>}
    </main>
    {authOpen && <AuthModal onClose={() => setAuthOpen(false)} onSuccess={(value) => { localStorage.setItem("earntracker-session", JSON.stringify(value)); setSession(value); setAuthOpen(false); setNotice(`Welcome, ${value.name}. Your account is connected.`); }} />}
    {verifyOpen && session && <VerifyModal token={session.token} onClose={() => setVerifyOpen(false)} />}
  </div>;
}

function Stat({ label, value, change, good, sub, icon }: { label: string; value: string; change?: string; good?: boolean; sub?: string; icon: string }) { return <article className="stat card"><div className="stat-icon">{icon}</div><p>{label}</p><h3>{value}</h3>{change ? <small className={good ? "good" : "muted"}>↗ {change} <span>from last month</span></small> : <small className="muted">{sub}</small>}</article>; }
function CardHead({ title, action }: { title: string; action: string }) { return <div className="card-head"><h3>{title}</h3><button>{action}</button></div>; }
function Chart() { return <div className="chart"><div className="axis"><span>$3k</span><span>$2k</span><span>$1k</span><span>$0</span></div><svg viewBox="0 0 680 190" preserveAspectRatio="none" aria-label="Earnings chart"><defs><linearGradient id="area" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stopColor="#7055e8" stopOpacity=".28"/><stop offset="1" stopColor="#7055e8" stopOpacity="0"/></linearGradient></defs><path className="gridline" d="M0 20H680M0 70H680M0 120H680M0 170H680"/><path className="area" d="M0 145 C65 130 90 106 135 115 S220 145 270 95 S350 82 405 65 S490 85 540 45 S620 55 680 20 V190 H0Z"/><path className="line" d="M0 145 C65 130 90 106 135 115 S220 145 270 95 S350 82 405 65 S490 85 540 45 S620 55 680 20"/><path className="fee-line" d="M0 168 C90 158 160 166 230 152 S370 157 450 140 S570 150 680 130"/></svg><div className="months"><span>Mar</span><span>Apr</span><span>May</span><span>Jun</span><span>Jul</span><span>Aug</span></div></div>; }

function TransactionTable({ transactions }: { transactions: (Transaction & { source: string })[] }) { return <div className="table-wrap"><table><thead><tr><th>Payment</th><th>Source</th><th>Date</th><th>Status</th><th>Amount</th></tr></thead><tbody>{transactions.length ? transactions.map((item) => <tr key={item.id}><td><span className="payment-icon">{item.source.slice(0, 1)}</span><span><strong>{item.description || "Payment received"}</strong><small>{item.externalId}</small></span></td><td>{item.source}</td><td>{new Intl.DateTimeFormat("en-GB", { day: "numeric", month: "short", year: "numeric" }).format(new Date(item.occurredAt))}</td><td><span className="status">● {item.status}</span></td><td><strong>{money(item.amount - item.fee, item.currency)}</strong><small className="fee">Fee {money(item.fee, item.currency)}</small></td></tr>) : <tr><td colSpan={5} className="empty">No payments yet.</td></tr>}</tbody></table></div>; }

function SectionView({ active, overview, transactions, onVerify }: { active: string; overview: Overview; transactions: (Transaction & {source: string})[]; onVerify: () => void }) {
  if (active === "Transactions") return <section className="card page-card"><CardHead title="All transactions" action="Export CSV"/><TransactionTable transactions={transactions}/></section>;
  if (active === "Earnings") return <section className="source-grid">{overview.incomeSources.map((source) => <article className="card source-card" key={source.id}><span className="source-logo">{source.name[0]}</span><div><small>{source.provider}</small><h3>{source.name}</h3><p>{source.transactions.length} payments</p></div><strong>{money(source.transactions.reduce((sum, item) => sum + item.amount - item.fee, 0), source.currency)}</strong></article>)}</section>;
  if (active === "Goals") return <section className="source-grid">{overview.financialGoals.map((item) => <article className="card source-card" key={item.id}><span className="source-logo">◎</span><div><small>Ends {item.targetDate}</small><h3>{item.name}</h3><p>{Math.round(item.progressPercentage)}% complete</p></div><strong>{money(item.currentAmount, item.currency)} / {money(item.targetAmount, item.currency)}</strong></article>)}</section>;
  return <section className="connections"><div className="connection-intro"><p className="eyebrow">PAYMENT CONNECTIONS</p><h2>Keep every payment in one place.</h2><p>Check a PayPal capture or Paystack payment and add it to your workflow.</p><button className="primary" onClick={onVerify}>Verify a payment</button></div>{["PayPal", "Paystack"].map((name) => <article className="card connection" key={name}><span className={`provider ${name.toLowerCase()}`}>{name === "PayPal" ? "P" : "P₦"}</span><div><h3>{name}</h3><p>Ready to verify payments</p></div><span className="connected">● Available</span></article>)}</section>;
}

function AuthModal({ onClose, onSuccess }: { onClose: () => void; onSuccess: (session: AuthSession) => void }) {
  const [mode, setMode] = useState<"login" | "register">("login"); const [error, setError] = useState(""); const [busy, setBusy] = useState(false);
  async function submit(event: FormEvent<HTMLFormElement>) { event.preventDefault(); setBusy(true); setError(""); const data = new FormData(event.currentTarget); try { const result = mode === "login" ? await api.login(String(data.get("email")), String(data.get("password"))) : await api.register(String(data.get("name")), String(data.get("email")), String(data.get("password"))); onSuccess(result); } catch (e) { setError(e instanceof Error ? e.message : "Please try again."); } finally { setBusy(false); } }
  return <div className="modal-backdrop" onMouseDown={onClose}><div className="modal" onMouseDown={(e) => e.stopPropagation()}><button className="modal-close" onClick={onClose}>×</button><span className="brand-mark">E</span><h2>{mode === "login" ? "Welcome back" : "Create your account"}</h2><p>{mode === "login" ? "Sign in to see your live earnings." : "Start keeping all your earnings in one view."}</p><form onSubmit={submit}>{mode === "register" && <label>Your name<input name="name" required minLength={2} placeholder="Opeyemi Ade" /></label>}<label>Email address<input name="email" type="email" required placeholder="you@example.com" /></label><label>Password<input name="password" type="password" required minLength={8} maxLength={12} placeholder="8–12 characters" /></label>{error && <p className="form-error">{error}</p>}<button className="primary" disabled={busy}>{busy ? "Please wait…" : mode === "login" ? "Sign in" : "Create account"}</button></form><button className="switch" onClick={() => { setMode(mode === "login" ? "register" : "login"); setError(""); }}>{mode === "login" ? "New here? Create an account" : "Already have an account? Sign in"}</button></div></div>;
}

function VerifyModal({ token, onClose }: { token: string; onClose: () => void }) { const [provider, setProvider] = useState<"paypal"|"paystack">("paypal"); const [result, setResult] = useState(""); const [busy, setBusy] = useState(false); async function submit(e: FormEvent<HTMLFormElement>) { e.preventDefault(); setBusy(true); setResult(""); const reference = String(new FormData(e.currentTarget).get("reference")); try { await api.verify(provider, reference, token); setResult("Payment verified successfully."); } catch (error) { setResult(error instanceof Error ? error.message : "Could not verify this payment."); } finally { setBusy(false); } } return <div className="modal-backdrop" onMouseDown={onClose}><div className="modal" onMouseDown={(e) => e.stopPropagation()}><button className="modal-close" onClick={onClose}>×</button><h2>Verify a payment</h2><p>Choose where the payment came from and enter its reference.</p><div className="provider-tabs"><button className={provider === "paypal" ? "selected" : ""} onClick={() => setProvider("paypal")}>PayPal</button><button className={provider === "paystack" ? "selected" : ""} onClick={() => setProvider("paystack")}>Paystack</button></div><form onSubmit={submit}><label>{provider === "paypal" ? "Capture ID" : "Transaction reference"}<input name="reference" required placeholder={provider === "paypal" ? "e.g. 5TY..." : "e.g. TRX_..."}/></label>{result && <p className="form-result">{result}</p>}<button className="primary" disabled={busy}>{busy ? "Checking…" : "Verify payment"}</button></form></div></div>; }
