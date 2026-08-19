"use client";

import { FormEvent, useCallback, useEffect, useMemo, useState } from "react";
import { api, AuthSession, Goal, Overview, Transaction } from "@/lib/api";

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
  financialGoals: [{ id: "g1", name: "August income goal", targetAmount: 15000, currentAmount: 11842, progressPercentage: 79, currency: "USD", startDate: "2026-08-01", targetDate: "2026-08-31", status: "Active", isAchieved: false }],
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
  const [goalOpen, setGoalOpen] = useState(false);
  const [mobileAccountOpen, setMobileAccountOpen] = useState(false);

  useEffect(() => {
    const stored = localStorage.getItem("earntracker-session");
    if (!stored) return;
    try {
      const savedSession = JSON.parse(stored);
      queueMicrotask(() => setSession(savedSession));
    } catch { localStorage.removeItem("earntracker-session"); }
  }, []);

  const refreshOverview = useCallback(async () => {
    if (!session) return;
    setLoading(true);
    try {
      setOverview(await api.overview(session.token));
    } catch (error) {
      setNotice(error instanceof Error ? error.message : "Could not update the dashboard.");
    } finally {
      setLoading(false);
    }
  }, [session]);

  useEffect(() => {
    if (!session) return;
    let isCurrent = true;
    api.overview(session.token)
      .then((value) => { if (isCurrent) setOverview(value); })
      .catch((error) => { if (isCurrent) setNotice(error.message); })
      .finally(() => { if (isCurrent) setLoading(false); });
    return () => { isCurrent = false; };
  }, [session]);

  const transactions = useMemo(() => overview.incomeSources.flatMap((source) => source.transactions.map((item) => ({ ...item, source: source.name }))).sort((a, b) => +new Date(b.occurredAt) - +new Date(a.occurredAt)), [overview]);
  const total = overview.totals[0] ?? { currency: "USD", gross: 0, fees: 0, net: 0 };
  const goal = overview.financialGoals.find((item) => item.status === "Active")
    ?? overview.financialGoals.find((item) => item.status === "Achieved");
  const displayName = session?.name || "Opeyemi";

  function signOut() { localStorage.removeItem("earntracker-session"); setSession(null); setOverview(demoOverview); setMobileAccountOpen(false); setNotice("You have signed out."); }

  return <div className="app-shell">
    <aside className="sidebar">
      <div className="brand"><span className="brand-mark">E</span><span>EarnTracker</span></div>
      <nav aria-label="Main navigation">{nav.map((item) => <button key={item} onClick={() => { setActive(item); setMobileAccountOpen(false); }} className={active === item ? "active" : ""} aria-current={active === item ? "page" : undefined}><Icon name={item}/><span>{item === "Transactions" ? <><span className="desktop-label">Transactions</span><span className="mobile-label">Activity</span></> : item}</span></button>)}</nav>
      <div className="sidebar-foot">
        <div className="help-card"><span>?</span><strong>Need a hand?</strong><p>Find quick answers and guides.</p><button>Visit help centre</button></div>
        <button className="profile" onClick={() => session ? signOut() : setAuthOpen(true)}><span className="avatar">{displayName.slice(0, 2).toUpperCase()}</span><span><strong>{displayName}</strong><small>{session ? "Sign out" : "Sign in to sync"}</small></span><b>⋮</b></button>
      </div>
    </aside>

    <main className="main">
      <header className="app-header">
        <div className="header-title"><span className="mobile-brand" aria-hidden="true">E</span><div><p>{session ? "Your workspace" : "Preview workspace"}</p><h1>{active}</h1></div></div>
        <div className="header-actions"><button className="icon-button" aria-label="Notifications">♢<span /></button><button className="primary payment-button" onClick={() => session ? setVerifyOpen(true) : setAuthOpen(true)}>{session ? "+ Payment" : "Sign in"}</button></div>
        <div className="mobile-account">
          <button className="mobile-account-trigger" aria-label="Open account menu" aria-expanded={mobileAccountOpen} onClick={() => setMobileAccountOpen((open) => !open)}><span className="avatar">{displayName.slice(0, 2).toUpperCase()}</span><span className="account-chevron">⌄</span></button>
          {mobileAccountOpen && <div className="mobile-account-menu"><div><strong>{displayName}</strong><small>{session ? session.tag || "Signed in" : "Preview account"}</small></div>{session ? <><button onClick={() => { setVerifyOpen(true); setMobileAccountOpen(false); }}>Add payment</button><button className="signout-button" onClick={signOut}>Sign out</button></> : <button onClick={() => { setAuthOpen(true); setMobileAccountOpen(false); }}>Sign in</button>}</div>}
        </div>
      </header>
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

      {active !== "Overview" && <SectionView active={active} overview={overview} transactions={transactions} onVerify={() => session ? setVerifyOpen(true) : setAuthOpen(true)} onCreateGoal={() => session ? setGoalOpen(true) : setAuthOpen(true)} />}
      {loading && <div className="loading">Updating your dashboard…</div>}
    </main>
    {authOpen && <AuthModal onClose={() => setAuthOpen(false)} onSuccess={(value) => { localStorage.setItem("earntracker-session", JSON.stringify(value)); setSession(value); setAuthOpen(false); setNotice(`Welcome, ${value.name}. Your account is connected.`); }} />}
    {verifyOpen && session && <VerifyModal token={session.token} onClose={() => setVerifyOpen(false)} onPaymentRecorded={refreshOverview} />}
    {goalOpen && session && <GoalModal token={session.token} onClose={() => setGoalOpen(false)} onCreated={async () => { await refreshOverview(); setGoalOpen(false); setNotice("Your new financial goal is ready."); }} />}
  </div>;
}

function Stat({ label, value, change, good, sub, icon }: { label: string; value: string; change?: string; good?: boolean; sub?: string; icon: string }) { return <article className="stat card"><div className="stat-icon">{icon}</div><p>{label}</p><h3>{value}</h3>{change ? <small className={good ? "good" : "muted"}>↗ {change} <span>from last month</span></small> : <small className="muted">{sub}</small>}</article>; }
function CardHead({ title, action }: { title: string; action: string }) { return <div className="card-head"><h3>{title}</h3><button>{action}</button></div>; }
function Chart() { return <div className="chart"><div className="axis"><span>$3k</span><span>$2k</span><span>$1k</span><span>$0</span></div><svg viewBox="0 0 680 190" preserveAspectRatio="none" aria-label="Earnings chart"><defs><linearGradient id="area" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stopColor="#7055e8" stopOpacity=".28"/><stop offset="1" stopColor="#7055e8" stopOpacity="0"/></linearGradient></defs><path className="gridline" d="M0 20H680M0 70H680M0 120H680M0 170H680"/><path className="area" d="M0 145 C65 130 90 106 135 115 S220 145 270 95 S350 82 405 65 S490 85 540 45 S620 55 680 20 V190 H0Z"/><path className="line" d="M0 145 C65 130 90 106 135 115 S220 145 270 95 S350 82 405 65 S490 85 540 45 S620 55 680 20"/><path className="fee-line" d="M0 168 C90 158 160 166 230 152 S370 157 450 140 S570 150 680 130"/></svg><div className="months"><span>Mar</span><span>Apr</span><span>May</span><span>Jun</span><span>Jul</span><span>Aug</span></div></div>; }

function TransactionTable({ transactions }: { transactions: (Transaction & { source: string })[] }) { return <div className="table-wrap"><table><thead><tr><th>Payment</th><th>Source</th><th>Date</th><th>Status</th><th>Amount</th></tr></thead><tbody>{transactions.length ? transactions.map((item) => <tr key={item.id}><td><span className="payment-icon">{item.source.slice(0, 1)}</span><span><strong>{item.description || "Payment received"}</strong><small>{item.externalId}</small></span></td><td>{item.source}</td><td>{new Intl.DateTimeFormat("en-GB", { day: "numeric", month: "short", year: "numeric" }).format(new Date(item.occurredAt))}</td><td><span className="status">● {item.status}</span></td><td><strong>{money(item.amount - item.fee, item.currency)}</strong><small className="fee">Fee {money(item.fee, item.currency)}</small></td></tr>) : <tr><td colSpan={5} className="empty">No payments yet.</td></tr>}</tbody></table></div>; }

function SectionView({ active, overview, transactions, onVerify, onCreateGoal }: { active: string; overview: Overview; transactions: (Transaction & {source: string})[]; onVerify: () => void; onCreateGoal: () => void }) {
  if (active === "Transactions") return <section className="card page-card"><CardHead title="All transactions" action="Export CSV"/><TransactionTable transactions={transactions}/></section>;
  if (active === "Earnings") return <section className="source-grid">{overview.incomeSources.map((source) => <article className="card source-card" key={source.id}><span className="source-logo">{source.name[0]}</span><div><small>{source.provider}</small><h3>{source.name}</h3><p>{source.transactions.length} payments</p></div><strong>{money(source.transactions.reduce((sum, item) => sum + item.amount - item.fee, 0), source.currency)}</strong></article>)}</section>;
  if (active === "Goals") return <GoalsView goals={overview.financialGoals} onCreate={onCreateGoal} />;
  return <section className="connections"><div className="connection-intro demo-intro"><p className="eyebrow">QUICK PRODUCT DEMO</p><h2>See your dashboard update instantly.</h2><p>Add a clearly labelled fictional payment. No PayPal account, approval, or real money is needed.</p><button className="primary demo-cta" onClick={onVerify}>Try a demo payment</button></div><article className="card connection demo-connection"><span className="provider demo-provider">✦</span><div><h3>Demo payment</h3><p>Fast, fictional, and safe for anyone to try</p></div><span className="connected">Recommended</span></article>{["PayPal", "Paystack"].map((name) => <article className="card connection" key={name}><span className={`provider ${name.toLowerCase()}`}>{name === "PayPal" ? "P" : "P₦"}</span><div><h3>{name}</h3><p>{name === "PayPal" ? "Advanced Sandbox test with buyer approval" : "Check a transaction reference"}</p></div><span className="connection-tag">Advanced</span></article>)}</section>;
}

function GoalsView({ goals, onCreate }: { goals: Goal[]; onCreate: () => void }) {
  const activeGoals = goals.filter((goal) => goal.status === "Active");
  const achievedGoals = goals.filter((goal) => goal.status === "Achieved");
  const expiredGoals = goals.filter((goal) => goal.status === "Expired");

  const group = (title: string, items: Goal[]) => <section className="goal-group"><div className="goal-group-head"><h2>{title}</h2><span>{items.length}</span></div>{items.length ? <div className="source-grid">{items.map((item) => <article className={`card source-card goal-item ${item.status.toLowerCase()}`} key={item.id}><span className="source-logo">{item.isAchieved ? "✓" : "◎"}</span><div><small>{item.status} · {item.startDate} to {item.targetDate}</small><h3>{item.name}</h3><div className="goal-progress"><i style={{ width: `${item.progressPercentage}%` }} /></div><p>{Math.round(item.progressPercentage)}% complete</p></div><strong>{money(item.currentAmount, item.currency)} / {money(item.targetAmount, item.currency)}</strong></article>)}</div> : <div className="card goal-empty">No {title.toLowerCase()}.</div>}</section>;

  return <div className="goals-page"><div className="goals-toolbar"><div><p className="eyebrow">FINANCIAL TARGETS</p><h2>Turn every payment into progress.</h2></div><button className="primary" onClick={onCreate}>+ New goal</button></div>{group("Active goals", activeGoals)}{group("Achieved goals", achievedGoals)}{expiredGoals.length > 0 && group("Expired goals", expiredGoals)}</div>;
}

function GoalModal({ token, onClose, onCreated }: { token: string; onClose: () => void; onCreated: () => Promise<void> }) {
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const today = new Date();
  const target = new Date(today);
  target.setDate(target.getDate() + 30);
  const startDate = today.toISOString().slice(0, 10);
  const targetDate = target.toISOString().slice(0, 10);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setBusy(true); setError("");
    const data = new FormData(event.currentTarget);
    try {
      await api.createGoal({ name: String(data.get("name")), targetAmount: Number(data.get("targetAmount")), currency: String(data.get("currency")), startDate: String(data.get("startDate")), targetDate: String(data.get("targetDate")) }, token);
      await onCreated();
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "Could not create this goal.");
    } finally {
      setBusy(false);
    }
  }

  return <div className="modal-backdrop" onMouseDown={onClose}><div className="modal" onMouseDown={(event) => event.stopPropagation()}><button className="modal-close" onClick={onClose}>×</button><span className="brand-mark">◎</span><h2>Set a new goal</h2><p>Your completed earnings during this date range will update the goal automatically.</p><form onSubmit={submit}><label>Goal name<input name="name" required minLength={2} maxLength={100} placeholder="September income goal" /></label><div className="form-row"><label>Target amount<input name="targetAmount" type="number" min="0.01" max="1000000000" step="0.01" required /></label><label>Currency<input name="currency" minLength={3} maxLength={3} pattern="[A-Za-z]{3}" defaultValue="USD" required /></label></div><div className="form-row"><label>Start date<input name="startDate" type="date" defaultValue={startDate} required /></label><label>Target date<input name="targetDate" type="date" defaultValue={targetDate} min={startDate} required /></label></div>{error && <p className="form-error">{error}</p>}<button className="primary" disabled={busy}>{busy ? "Creating…" : "Create goal"}</button></form></div></div>;
}

function AuthModal({ onClose, onSuccess }: { onClose: () => void; onSuccess: (session: AuthSession) => void }) {
  const [mode, setMode] = useState<"login" | "register">("login"); const [error, setError] = useState(""); const [busy, setBusy] = useState(false);
  async function submit(event: FormEvent<HTMLFormElement>) { event.preventDefault(); setBusy(true); setError(""); const data = new FormData(event.currentTarget); try { const result = mode === "login" ? await api.login(String(data.get("email")), String(data.get("password"))) : await api.register(String(data.get("name")), String(data.get("email")), String(data.get("password"))); onSuccess(result); } catch (e) { setError(e instanceof Error ? e.message : "Please try again."); } finally { setBusy(false); } }
  return <div className="modal-backdrop" onMouseDown={onClose}><div className="modal" onMouseDown={(e) => e.stopPropagation()}><button className="modal-close" onClick={onClose}>×</button><span className="brand-mark">E</span><h2>{mode === "login" ? "Welcome back" : "Create your account"}</h2><p>{mode === "login" ? "Sign in to see your live earnings." : "Start keeping all your earnings in one view."}</p><form onSubmit={submit}>{mode === "register" && <label>Your name<input name="name" required minLength={2} placeholder="Opeyemi Ade" /></label>}<label>Email address<input name="email" type="email" required placeholder="you@example.com" /></label><label>Password<input name="password" type="password" required minLength={8} maxLength={12} placeholder="8–12 characters" /></label>{error && <p className="form-error">{error}</p>}<button className="primary" disabled={busy}>{busy ? "Please wait…" : mode === "login" ? "Sign in" : "Create account"}</button></form><button className="switch" onClick={() => { setMode(mode === "login" ? "register" : "login"); setError(""); }}>{mode === "login" ? "New here? Create an account" : "Already have an account? Sign in"}</button></div></div>;
}

function VerifyModal({ token, onClose, onPaymentRecorded }: { token: string; onClose: () => void; onPaymentRecorded: () => Promise<void> }) {
  const [advanced, setAdvanced] = useState(false);
  const [provider, setProvider] = useState<"paypal" | "paystack" | "capture">("paypal");
  const [orderId, setOrderId] = useState("");
  const [approvalUrl, setApprovalUrl] = useState("");
  const [orderStatus, setOrderStatus] = useState("");
  const [paystackReference, setPaystackReference] = useState("");
  const [paystackUrl, setPaystackUrl] = useState("");
  const [result, setResult] = useState("");
  const [busy, setBusy] = useState(false);

  function showError(error: unknown) {
    setResult(error instanceof Error ? error.message : "Could not complete this request.");
  }

  async function createDemoPayment(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setBusy(true); setResult("");
    const data = new FormData(event.currentTarget);
    try {
      await api.createDemoPayment(Number(data.get("amount")), String(data.get("currency")), String(data.get("description")), token);
      await onPaymentRecorded();
      setResult("Demo payment added. Your totals and goals have been updated.");
    } catch (error) { showError(error); } finally { setBusy(false); }
  }

  async function createOrder(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setBusy(true); setResult("");
    const data = new FormData(event.currentTarget);
    try {
      const order = await api.createPayPalOrder(Number(data.get("amount")), String(data.get("currency")), String(data.get("description")), token);
      setOrderId(order.id); setOrderStatus(order.status);
      setApprovalUrl(order.links?.find((link) => link.rel === "approve")?.href ?? "");
      setResult("Order created. Approve it with your PayPal Sandbox buyer account.");
    } catch (error) { showError(error); } finally { setBusy(false); }
  }

  async function initializePaystack(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setBusy(true); setResult("");
    const data = new FormData(event.currentTarget);
    try {
      const response = await api.initializePayStack({
        email: String(data.get("email")),
        amount: Number(data.get("amount")),
        currency: String(data.get("currency")),
        description: String(data.get("description")),
        callbackUrl: window.location.origin,
      }, token);
      setPaystackReference(response.data.reference);
      setPaystackUrl(response.data.authorization_url);
      setResult("Test checkout created. Complete it in Paystack, then return here to verify.");
    } catch (error) { showError(error); } finally { setBusy(false); }
  }

  async function verifyInitializedPaystack() {
    setBusy(true); setResult("");
    try {
      const verification = await api.verify("paystack", paystackReference, token);
      const data = verification.data as { status?: string } | undefined;
      if (data?.status !== "success") {
        setResult(`Paystack status: ${data?.status ?? "not completed"}. Complete checkout before verifying again.`);
        return;
      }
      await onPaymentRecorded();
      setResult("Paystack payment verified and added to your dashboard.");
    } catch (error) { showError(error); } finally { setBusy(false); }
  }

  async function refreshOrder() {
    setBusy(true); setResult("");
    try { const order = await api.getPayPalOrder(orderId, token); setOrderStatus(order.status); setResult(`Order status: ${order.status}.`); }
    catch (error) { showError(error); } finally { setBusy(false); }
  }

  async function captureOrder() {
    setBusy(true); setResult("");
    try { const order = await api.capturePayPalOrder(orderId, token); setOrderStatus(order.status); if (order.status === "COMPLETED") await onPaymentRecorded(); setResult(order.status === "COMPLETED" ? "Payment captured and added to your dashboard." : `Order status: ${order.status}.`); }
    catch (error) { showError(error); } finally { setBusy(false); }
  }

  async function verifyReference(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setBusy(true); setResult("");
    const reference = String(new FormData(event.currentTarget).get("reference"));
    try { await api.verify(provider === "capture" ? "paypal" : "paystack", reference, token); if (provider === "capture") await onPaymentRecorded(); setResult(provider === "capture" ? "Payment found and added to your dashboard." : "Payment found successfully."); }
    catch (error) { showError(error); } finally { setBusy(false); }
  }

  return <div className="modal-backdrop" onMouseDown={onClose}><div className="modal payment-modal" onMouseDown={(e) => e.stopPropagation()}>
    <button className="modal-close" onClick={onClose}>×</button><span className={`payment-mode-icon ${advanced ? "advanced" : ""}`}>{advanced ? "P" : "✦"}</span><h2>{advanced ? "Advanced payment test" : "Try a demo payment"}</h2><p>{advanced ? "Use provider test tools and existing payment references." : "Add fictional earnings to explore totals, transactions, and goals. No real money is involved."}</p>
    {!advanced ? <form className="demo-payment-form" onSubmit={createDemoPayment}><div className="demo-disclaimer"><strong>Demo only</strong><span>This entry will be labelled as fictional on your dashboard.</span></div><div className="form-row"><label>Amount<input name="amount" type="number" min="0.01" max="1000000" step="0.01" defaultValue="250.00" required /></label><label>Currency<input name="currency" minLength={3} maxLength={3} pattern="[A-Za-z]{3}" defaultValue="USD" required /></label></div><label>What was it for?<input name="description" maxLength={120} defaultValue="Sample freelance project" required /></label><button className="primary demo-submit" disabled={busy}>{busy ? "Adding demo…" : "Add demo payment"}</button></form> : <>
    <div className="provider-tabs"><button className={provider === "paypal" ? "selected" : ""} onClick={() => { setProvider("paypal"); setResult(""); }}>PayPal Sandbox</button><button className={provider === "paystack" ? "selected" : ""} onClick={() => { setProvider("paystack"); setResult(""); }}>Paystack</button><button className={provider === "capture" ? "selected" : ""} onClick={() => { setProvider("capture"); setResult(""); }}>PayPal capture</button></div>
    {provider === "paypal" ? <>
      {!orderId ? <form onSubmit={createOrder}><div className="form-row"><label>Amount<input name="amount" type="number" min="0.01" max="1000000" step="0.01" defaultValue="10.00" required /></label><label>Currency<input name="currency" minLength={3} maxLength={3} pattern="[A-Za-z]{3}" defaultValue="USD" required /></label></div><label>Description<input name="description" maxLength={127} defaultValue="Freelancer earnings tracker sandbox test" required /></label><button className="primary" disabled={busy}>{busy ? "Creating…" : "Create test order"}</button></form> : <div className="order-steps"><div className="order-summary"><span>Order</span><strong>{orderId}</strong><span className={`order-state ${orderStatus.toLowerCase()}`}>{orderStatus}</span></div><ol><li><strong>Approve the order</strong><span>Sign in with a PayPal Sandbox buyer account.</span>{approvalUrl && <a className="primary approval-link" href={approvalUrl} target="_blank" rel="noreferrer">Open PayPal Sandbox ↗</a>}</li><li><strong>Check approval</strong><span>Return here after approving the order.</span><button className="secondary" onClick={refreshOrder} disabled={busy}>Check order status</button></li><li><strong>Capture payment</strong><span>Capture once the order status is approved.</span><button className="primary" onClick={captureOrder} disabled={busy || orderStatus === "COMPLETED"}>{orderStatus === "COMPLETED" ? "Payment captured" : "Capture payment"}</button></li></ol><button className="switch" onClick={() => { setOrderId(""); setApprovalUrl(""); setOrderStatus(""); setResult(""); }}>Create another order</button></div>}
    </> : provider === "paystack" ? <>{!paystackReference ? <form onSubmit={initializePaystack}><label>Test buyer email<input name="email" type="email" required placeholder="buyer@example.com" /></label><div className="form-row"><label>Amount<input name="amount" type="number" min="0.01" max="1000000" step="0.01" defaultValue="1000.00" required /></label><label>Currency<input name="currency" minLength={3} maxLength={3} pattern="[A-Za-z]{3}" defaultValue="NGN" required /></label></div><label>Description<input name="description" maxLength={120} defaultValue="Paystack test payment" required /></label><button className="primary" disabled={busy}>{busy ? "Creating…" : "Create Paystack test"}</button></form> : <div className="order-steps paystack-steps"><div className="order-summary"><span>Reference</span><strong>{paystackReference}</strong><span className="order-state">TEST</span></div><ol><li><strong>Open test checkout</strong><span>Use a Paystack test card. No real money will move.</span><a className="primary approval-link" href={paystackUrl} target="_blank" rel="noreferrer">Open Paystack checkout ↗</a></li><li><strong>Verify and record</strong><span>Return after checkout succeeds, then add it to your dashboard.</span><button className="primary" onClick={verifyInitializedPaystack} disabled={busy}>{busy ? "Verifying…" : "Verify payment"}</button></li></ol><button className="switch" onClick={() => { setPaystackReference(""); setPaystackUrl(""); setResult(""); }}>Create another Paystack test</button></div>}</> : <form onSubmit={verifyReference}><label>Capture ID<input name="reference" required placeholder="e.g. 5TY..."/></label><button className="primary" disabled={busy}>{busy ? "Checking…" : "Check payment"}</button></form>}</>}
    {result && <p className="form-result">{result}</p>}
    <button className="advanced-toggle" onClick={() => { setAdvanced((value) => !value); setResult(""); }}>{advanced ? "← Back to demo payment" : "Test with PayPal Sandbox or Paystack →"}</button>
  </div></div>;
}
