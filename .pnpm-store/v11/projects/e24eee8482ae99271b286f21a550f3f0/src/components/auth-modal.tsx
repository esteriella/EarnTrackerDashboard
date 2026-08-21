"use client";

import { FormEvent, useState } from "react";
import { api, type AuthSession } from "@/lib/api";

type AuthModalProps = { initialMode?: "login" | "register"; onClose: () => void; onSuccess: (session: AuthSession) => void };

export function AuthModal({ initialMode = "login", onClose, onSuccess }: AuthModalProps) {
  const [mode, setMode] = useState(initialMode);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setBusy(true); setError("");
    const data = new FormData(event.currentTarget);
    try {
      const result = mode === "login"
        ? await api.login(String(data.get("email")), String(data.get("password")))
        : await api.register(String(data.get("name")), String(data.get("email")), String(data.get("password")));
      onSuccess(result);
    } catch (cause) { setError(cause instanceof Error ? cause.message : "Please try again."); }
    finally { setBusy(false); }
  }

  return <div className="modal-backdrop" onMouseDown={onClose}><div className="modal" onMouseDown={(event) => event.stopPropagation()}>
    <button className="modal-close" onClick={onClose} aria-label="Close">×</button><span className="brand-mark">E</span>
    <h2>{mode === "login" ? "Welcome back" : "Create your account"}</h2><p>{mode === "login" ? "Sign in to see your live earnings." : "Start keeping all your earnings in one view."}</p>
    <form onSubmit={submit}>{mode === "register" && <label>Your name<input name="name" required minLength={2} placeholder="Opeyemi Ade" /></label>}<label>Email address<input name="email" type="email" required placeholder="you@example.com" /></label><label>Password<input name="password" type="password" required minLength={8} maxLength={12} placeholder="8–12 characters" /></label>{error && <p className="form-error">{error}</p>}<button className="primary" disabled={busy}>{busy ? "Please wait…" : mode === "login" ? "Sign in" : "Create account"}</button></form>
    <button className="switch" onClick={() => { setMode(mode === "login" ? "register" : "login"); setError(""); }}>{mode === "login" ? "New here? Create an account" : "Already have an account? Sign in"}</button>
  </div></div>;
}
