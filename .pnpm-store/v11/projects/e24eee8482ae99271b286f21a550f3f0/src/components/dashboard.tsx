"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { api, type AuthSession, type Overview } from "@/lib/api";
import { clearSession, readSession } from "@/lib/session";
import { DashboardHeader, DashboardSidebar, type DashboardNotification } from "@/components/dashboard/navigation";
import { GoalModal, PaymentModal } from "@/components/dashboard/modals";
import { DashboardSection, OverviewView } from "@/components/dashboard/views";

const emptyOverview: Overview = { totals: [], incomeSources: [], financialGoals: [] };

export function Dashboard() {
  const router = useRouter();
  const [session, setSession] = useState<AuthSession | null | undefined>();
  const [overview, setOverview] = useState(emptyOverview);
  const [active, setActive] = useState("Overview");
  const [selectedCurrency, setSelectedCurrency] = useState("USD");
  const [notice, setNotice] = useState("");
  const [loading, setLoading] = useState(false);
  const [accountOpen, setAccountOpen] = useState(false);
  const [paymentOpen, setPaymentOpen] = useState(false);
  const [goalOpen, setGoalOpen] = useState(false);
  const [returnedPaystackReference, setReturnedPaystackReference] = useState("");

  useEffect(() => {
    const savedSession = readSession();
    if (!savedSession) { router.replace("/"); return; }
    const query = new URLSearchParams(window.location.search);
    const reference = query.get("reference") ?? query.get("trxref");
    queueMicrotask(() => {
      setSession(savedSession);
      if (query.get("payment") === "paystack" && reference) {
        setReturnedPaystackReference(reference);
        setActive("Connections");
        setPaymentOpen(true);
      }
    });
    api.overview(savedSession.token)
      .then((value) => setOverview(value))
      .catch((cause) => setNotice(cause instanceof Error ? cause.message : "Could not update the dashboard."));
  }, [router]);

  const refreshOverview = useCallback(async (preferredCurrency?: string) => {
    if (!session) return;
    setLoading(true);
    try {
      setOverview(await api.overview(session.token));
      if (preferredCurrency) setSelectedCurrency(preferredCurrency.toUpperCase());
    } catch (cause) {
      setNotice(cause instanceof Error ? cause.message : "Could not update the dashboard.");
    } finally { setLoading(false); }
  }, [session]);

  const transactions = useMemo(() => overview.incomeSources
    .flatMap((source) => source.transactions.map((transaction) => ({ ...transaction, source: source.name })))
    .sort((left, right) => +new Date(right.occurredAt) - +new Date(left.occurredAt)), [overview]);
  const total = overview.totals.find((item) => item.currency === selectedCurrency) ?? overview.totals[0] ?? { currency: "USD", gross: 0, fees: 0, net: 0 };
  const goal = overview.financialGoals.find((item) => item.status === "Active") ?? overview.financialGoals.find((item) => item.status === "Achieved");
  const notifications = useMemo<DashboardNotification[]>(() => {
    const items: DashboardNotification[] = [];
    const latest = transactions[0];
    if (latest) items.push({ id: `transaction-${latest.id}`, title: "Latest payment", detail: `${latest.description || latest.source} was recorded in ${latest.currency}.` });
    const achievedGoal = overview.financialGoals.find((item) => item.status === "Achieved");
    if (achievedGoal) items.push({ id: `goal-${achievedGoal.id}`, title: "Goal achieved", detail: `${achievedGoal.name} has reached its target.` });
    const activeGoal = overview.financialGoals.find((item) => item.status === "Active" && item.targetAmount > 0 && item.currentAmount / item.targetAmount >= 0.75);
    if (activeGoal) items.push({ id: `goal-progress-${activeGoal.id}`, title: "Goal almost complete", detail: `${activeGoal.name} is at ${Math.min(100, Math.round(activeGoal.currentAmount / activeGoal.targetAmount * 100))}%.` });
    return items.slice(0, 3);
  }, [overview.financialGoals, transactions]);

  function signOut() { clearSession(); setSession(null); router.replace("/"); }
  function closePayment() { setPaymentOpen(false); setReturnedPaystackReference(""); if (window.location.search) window.history.replaceState({}, "", "/dashboard"); }

  if (!session) return <div className="dashboard-loading">Opening your dashboard…</div>;

  return <div className="app-shell">
    <DashboardSidebar active={active} displayName={session.name} onNavigate={(item) => { setActive(item); setAccountOpen(false); }} onSignOut={signOut}/>
    <main className="main">
      <DashboardHeader active={active} displayName={session.name} notifications={notifications} accountOpen={accountOpen} onAccountToggle={() => setAccountOpen((value) => !value)} onPayment={() => { setPaymentOpen(true); setAccountOpen(false); }} onSignOut={signOut}/>
      {notice && <div className="notice"><span>{notice}</span><button onClick={() => setNotice("")}>×</button></div>}
      {active === "Overview"
        ? <OverviewView displayName={session.name} overview={overview} total={total} goal={goal} transactions={transactions} onCurrency={setSelectedCurrency} onViewGoals={() => setActive("Goals")} onViewTransactions={() => setActive("Transactions")}/>
        : <DashboardSection active={active} overview={overview} transactions={transactions} onVerify={() => setPaymentOpen(true)} onCreateGoal={() => setGoalOpen(true)}/>
      }
      {loading && <div className="loading">Updating your dashboard…</div>}
    </main>
    {paymentOpen && <PaymentModal token={session.token} initialPaystackReference={returnedPaystackReference} onClose={closePayment} onPaymentRecorded={refreshOverview}/>}
    {goalOpen && <GoalModal token={session.token} onClose={() => setGoalOpen(false)} onCreated={async () => { await refreshOverview(); setGoalOpen(false); setNotice("Your new financial goal is ready."); }}/>}
  </div>;
}
