"use client";

import { FormEvent, useState } from "react";
import { api, type PayStackVerification } from "@/lib/api";

type PaymentModalProps = {
  token: string;
  initialPaystackReference: string;
  onClose: () => void;
  onPaymentRecorded: (currency?: string) => Promise<void>;
};

export function GoalModal({ token, onClose, onCreated }: { token: string; onClose: () => void; onCreated: () => Promise<void> }) {
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const today = new Date();
  const target = new Date(today);
  target.setDate(target.getDate() + 30);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setBusy(true); setError("");
    const data = new FormData(event.currentTarget);
    try {
      await api.createGoal({ name: String(data.get("name")), targetAmount: Number(data.get("targetAmount")), currency: String(data.get("currency")), startDate: String(data.get("startDate")), targetDate: String(data.get("targetDate")) }, token);
      await onCreated();
    } catch (cause) { setError(cause instanceof Error ? cause.message : "Could not create this goal."); }
    finally { setBusy(false); }
  }

  return <div className="modal-backdrop" onMouseDown={onClose}><div className="modal" onMouseDown={(event) => event.stopPropagation()}><button className="modal-close" onClick={onClose}>×</button><span className="brand-mark">◎</span><h2>Set a new goal</h2><p>Your completed earnings during this date range will update the goal automatically.</p><form onSubmit={submit}><label>Goal name<input name="name" required minLength={2} maxLength={100} placeholder="September income goal" /></label><div className="form-row"><label>Target amount<input name="targetAmount" type="number" min="0.01" max="1000000000" step="0.01" required /></label><label>Currency<input name="currency" minLength={3} maxLength={3} pattern="[A-Za-z]{3}" defaultValue="USD" required /></label></div><div className="form-row"><label>Start date<input name="startDate" type="date" defaultValue={today.toISOString().slice(0,10)} required /></label><label>Target date<input name="targetDate" type="date" defaultValue={target.toISOString().slice(0,10)} min={today.toISOString().slice(0,10)} required /></label></div>{error && <p className="form-error">{error}</p>}<button className="primary" disabled={busy}>{busy ? "Creating…" : "Create goal"}</button></form></div></div>;
}

export function PaymentModal({ token, initialPaystackReference, onClose, onPaymentRecorded }: PaymentModalProps) {
  const [advanced, setAdvanced] = useState(Boolean(initialPaystackReference));
  const [provider, setProvider] = useState<"paypal" | "paystack" | "capture">(initialPaystackReference ? "paystack" : "paypal");
  const [order, setOrder] = useState({ id: "", url: "", status: "" });
  const [paystack, setPaystack] = useState({ reference: initialPaystackReference, url: "" });
  const [message, setMessage] = useState("");
  const [busy, setBusy] = useState(false);

  const fail = (cause: unknown) => setMessage(cause instanceof Error ? cause.message : "Could not complete this request.");

  async function run(task: () => Promise<void>) {
    setBusy(true); setMessage("");
    try { await task(); } catch (cause) { fail(cause); } finally { setBusy(false); }
  }

  function createDemo(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); const data = new FormData(event.currentTarget); const currency = String(data.get("currency"));
    void run(async () => { await api.createDemoPayment(Number(data.get("amount")), currency, String(data.get("description")), token); await onPaymentRecorded(currency); setMessage("Demo payment added. Your totals and goals have been updated."); });
  }

  function createPayPal(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); const data = new FormData(event.currentTarget);
    void run(async () => { const value = await api.createPayPalOrder(Number(data.get("amount")), String(data.get("currency")), String(data.get("description")), token); setOrder({ id:value.id, status:value.status, url:value.links?.find((link) => link.rel === "approve")?.href ?? "" }); setMessage("Order created. Approve it with a PayPal Sandbox buyer account."); });
  }

  function checkPayPal() { void run(async () => { const value = await api.getPayPalOrder(order.id, token); setOrder((current) => ({...current,status:value.status})); setMessage(`Order status: ${value.status}.`); }); }
  function capturePayPal() { void run(async () => { const value = await api.capturePayPalOrder(order.id, token); setOrder((current) => ({...current,status:value.status})); if (value.status === "COMPLETED") await onPaymentRecorded(value.purchase_units?.[0]?.amount?.currency_code); setMessage(value.status === "COMPLETED" ? "Payment captured and added to your dashboard." : `Order status: ${value.status}.`); }); }

  function createPaystack(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); const data = new FormData(event.currentTarget);
    void run(async () => { const value = await api.initializePayStack({ email:String(data.get("email")), amount:Number(data.get("amount")), currency:String(data.get("currency")), description:String(data.get("description")), callbackUrl:`${window.location.origin}/dashboard?payment=paystack` },token); setPaystack({reference:value.data.reference,url:value.data.authorization_url}); setMessage("Test checkout created. Complete it in Paystack, then return here to verify."); });
  }

  function verifyPaystack() {
    void run(async () => { const value = await api.verify("paystack",paystack.reference,token) as PayStackVerification; if (value.data?.status !== "success") { setMessage(`Paystack status: ${value.data?.status ?? "not completed"}. Complete checkout before verifying again.`); return; } if (!value.earntracker_recorded) { setMessage("Paystack confirmed the payment, but it was not recorded for this account. Create a new test from EarnTracker."); return; } await onPaymentRecorded(value.data.currency); setMessage("Paystack payment verified and added to your dashboard."); });
  }

  function verifyCapture(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); const reference = String(new FormData(event.currentTarget).get("reference"));
    void run(async () => { await api.verify("paypal",reference,token); await onPaymentRecorded(); setMessage("Payment found and added to your dashboard."); });
  }

  return <div className="modal-backdrop" onMouseDown={onClose}><div className="modal payment-modal" onMouseDown={(event) => event.stopPropagation()}><button className="modal-close" onClick={onClose}>×</button><span className={`payment-mode-icon ${advanced ? "advanced" : ""}`}>{advanced ? "P" : "✦"}</span><h2>{advanced ? "Advanced payment test" : "Try a demo payment"}</h2><p>{advanced ? "Use provider test tools and existing payment references." : "Add fictional earnings to explore totals, transactions, and goals. No real money is involved."}</p>
    {!advanced ? <form className="demo-payment-form" onSubmit={createDemo}><div className="demo-disclaimer"><strong>Demo only</strong><span>This entry will be labelled as fictional on your dashboard.</span></div><div className="form-row"><label>Amount<input name="amount" type="number" min="0.01" max="1000000" step="0.01" defaultValue="250.00" required /></label><label>Currency<input name="currency" minLength={3} maxLength={3} pattern="[A-Za-z]{3}" defaultValue="USD" required /></label></div><label>What was it for?<input name="description" maxLength={120} defaultValue="Sample freelance project" required /></label><button className="primary demo-submit" disabled={busy}>{busy ? "Adding demo…" : "Add demo payment"}</button></form> : <><div className="provider-tabs">{[["paypal","PayPal Sandbox"],["paystack","Paystack"],["capture","PayPal capture"]].map(([key,label]) => <button key={key} className={provider === key ? "selected" : ""} onClick={() => {setProvider(key as typeof provider);setMessage("");}}>{label}</button>)}</div>
      {provider === "paypal" ? (!order.id ? <form onSubmit={createPayPal}><div className="form-row"><label>Amount<input name="amount" type="number" min="0.01" step="0.01" defaultValue="10.00" required /></label><label>Currency<input name="currency" defaultValue="USD" required /></label></div><label>Description<input name="description" defaultValue="Freelancer earnings tracker sandbox test" required /></label><button className="primary" disabled={busy}>Create test order</button></form> : <div className="order-steps"><div className="order-summary"><span>Order</span><strong>{order.id}</strong><span className={`order-state ${order.status.toLowerCase()}`}>{order.status}</span></div><ol><li><strong>Approve the order</strong><span>Sign in with a PayPal Sandbox buyer account.</span><a className="primary approval-link" href={order.url} target="_blank" rel="noreferrer">Open PayPal Sandbox ↗</a></li><li><strong>Check approval</strong><button className="secondary" onClick={checkPayPal}>Check order status</button></li><li><strong>Capture payment</strong><button className="primary" onClick={capturePayPal} disabled={order.status === "COMPLETED"}>Capture payment</button></li></ol></div>) : provider === "paystack" ? (!paystack.reference ? <form onSubmit={createPaystack}><label>Test buyer email<input name="email" type="email" required placeholder="buyer@example.com" /></label><div className="form-row"><label>Amount<input name="amount" type="number" min="0.01" step="0.01" defaultValue="1000.00" required /></label><label>Currency<input name="currency" defaultValue="NGN" required /></label></div><label>Description<input name="description" defaultValue="Paystack test payment" required /></label><button className="primary" disabled={busy}>Create Paystack test</button></form> : <div className="order-steps"><div className="order-summary"><span>Reference</span><strong>{paystack.reference}</strong><span className="order-state">TEST</span></div><ol><li><strong>{paystack.url ? "Open test checkout" : "Checkout completed"}</strong><span>{paystack.url ? "Use a Paystack test card. No real money will move." : "Paystack returned you to EarnTracker."}</span>{paystack.url && <a className="primary approval-link" href={paystack.url} target="_blank" rel="noreferrer">Open Paystack checkout ↗</a>}</li><li><strong>Verify and record</strong><button className="primary" onClick={verifyPaystack} disabled={busy}>Verify payment</button></li></ol></div>) : <form onSubmit={verifyCapture}><label>Capture ID<input name="reference" required placeholder="e.g. 5TY..."/></label><button className="primary" disabled={busy}>Check payment</button></form>}</>}
    {message && <p className="form-result">{message}</p>}<button className="advanced-toggle" onClick={() => {setAdvanced((value) => !value);setMessage("");}}>{advanced ? "← Back to demo payment" : "Test with PayPal Sandbox or Paystack →"}</button>
  </div></div>;
}
