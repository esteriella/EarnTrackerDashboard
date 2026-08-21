"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { AuthModal } from "@/components/auth-modal";
import { readSession, saveSession } from "@/lib/session";

const benefits = [
  { icon: "↗", title: "See what you really earn", text: "Bring income and fees together so your take-home amount is always clear." },
  { icon: "◎", title: "Make progress visible", text: "Set income goals and watch every completed payment move you closer." },
  { icon: "⌁", title: "Test payments safely", text: "Try demo payments or use PayPal and Paystack test tools without real money." },
];

export function LandingPage() {
  const router = useRouter();
  const [authMode, setAuthMode] = useState<"login" | "register" | null>(null);
  const [hasSession, setHasSession] = useState(false);

  useEffect(() => { const session = readSession(); queueMicrotask(() => setHasSession(Boolean(session))); }, []);
  function openDashboard() { if (hasSession) router.push("/dashboard"); else setAuthMode("register"); }

  return <main className="landing">
    <nav className="landing-nav"><a className="brand" href="#top"><span className="brand-mark">E</span><span>EarnTracker</span></a><div><a href="#features">How it works</a><button className="landing-signin" onClick={() => hasSession ? router.push("/dashboard") : setAuthMode("login")}>{hasSession ? "Open dashboard" : "Sign in"}</button></div></nav>
    <section className="landing-hero" id="top"><div className="hero-copy"><p className="landing-kicker">YOUR INCOME, IN ONE CLEAR VIEW</p><h1>Know what you earned.<br/><span>Plan what comes next.</span></h1><p>EarnTracker brings payments, fees, income sources, and financial goals together—without making money management feel complicated.</p><div className="hero-actions"><button className="landing-primary" onClick={openDashboard}>{hasSession ? "Go to dashboard" : "Start tracking for free"} <span>→</span></button><a href="#features">See how it works</a></div><small>No card required. Try it with safe demo payments.</small></div><div className="hero-preview" aria-label="EarnTracker dashboard preview"><div className="preview-top"><span>Monthly overview</span><i>Live</i></div><div className="preview-total"><small>Net earnings</small><strong>$11,842</strong><span>↗ 12.4% this month</span></div><div className="preview-bars">{[42,64,52,78,67,92].map((height,index) => <i key={index} style={{height:`${height}%`}} />)}</div><div className="preview-goal"><span>August goal</span><strong>79%</strong><div><i /></div></div></div></section>
    <section className="landing-proof"><span>Built for freelancers and independent earners</span><div><b>PayPal</b><b>Paystack</b><b>Direct clients</b><b>Demo mode</b></div></section>
    <section className="landing-features" id="features"><div className="section-heading"><p className="landing-kicker">LESS GUESSING, MORE CLARITY</p><h2>Everything you need to understand your earnings.</h2></div><div className="benefit-grid">{benefits.map((benefit) => <article key={benefit.title}><span>{benefit.icon}</span><h3>{benefit.title}</h3><p>{benefit.text}</p></article>)}</div></section>
    <section className="landing-cta"><div><p className="landing-kicker">READY WHEN YOU ARE</p><h2>Turn your next payment into progress.</h2><p>Create an account, add a safe demo payment, and see your dashboard come to life.</p></div><button className="landing-primary" onClick={openDashboard}>{hasSession ? "Open dashboard" : "Create free account"} →</button></section>
    <footer><div className="brand"><span className="brand-mark">E</span><span>EarnTracker</span></div><p>Simple earnings tracking for independent work.</p><small>© 2026 EarnTracker</small></footer>
    {authMode && <AuthModal initialMode={authMode} onClose={() => setAuthMode(null)} onSuccess={(session) => { saveSession(session); router.push("/dashboard"); }} />}
  </main>;
}
