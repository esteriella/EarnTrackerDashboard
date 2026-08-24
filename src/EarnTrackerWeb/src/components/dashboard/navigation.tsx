"use client";

import { useState, type ReactNode } from "react";

export const navigationItems = ["Overview", "Earnings", "Transactions", "Goals", "Connections"];
export type DashboardNotification = { id: string; title: string; detail: string };

function NavigationIcon({ name }: { name: string }) {
  const paths: Record<string, ReactNode> = {
    Overview: <><rect x="3" y="3" width="7" height="7" rx="2"/><rect x="14" y="3" width="7" height="7" rx="2"/><rect x="3" y="14" width="7" height="7" rx="2"/><rect x="14" y="14" width="7" height="7" rx="2"/></>,
    Earnings: <path d="M4 19V9m6 10V5m6 14v-7m4 7V3"/>, Transactions: <path d="M7 7h13l-3-3m0 13H4l3 3"/>,
    Goals: <><circle cx="12" cy="12" r="9"/><circle cx="12" cy="12" r="4"/><path d="m15 9 6-6"/></>, Connections: <><path d="M8 12h8m-1-5 5 5-5 5M9 7 4 12l5 5"/></>,
  };
  return <svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">{paths[name]}</svg>;
}

function BellIcon() { return <svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"><path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9"/><path d="M10 21h4"/></svg>; }

function NotificationControl({ notifications, open, hasUnread, onToggle, onRead }: { notifications: DashboardNotification[]; open: boolean; hasUnread: boolean; onToggle: () => void; onRead: () => void }) {
  return <div className="notification-wrap"><button className="icon-button" aria-label="Notifications" aria-expanded={open} onClick={onToggle}><BellIcon/>{hasUnread && <span />}</button>{open && <div className="notification-menu"><div className="notification-heading"><strong>Notifications</strong>{hasUnread && <button onClick={onRead}>Mark as read</button>}</div>{notifications.length ? notifications.map((item) => <div className="notification-item" key={item.id}><strong>{item.title}</strong><p>{item.detail}</p></div>) : <p className="notification-empty">You are all caught up.</p>}</div>}</div>;
}

export function DashboardSidebar({ active, displayName, onNavigate, onSignOut }: { active: string; displayName: string; onNavigate: (item: string) => void; onSignOut: () => void }) {
  return <aside className="sidebar"><div className="brand"><span className="brand-mark">E</span><span>EarnTracker</span></div><nav aria-label="Main navigation">{navigationItems.map((item) => <button key={item} onClick={() => onNavigate(item)} className={active === item ? "active" : ""} aria-current={active === item ? "page" : undefined}><NavigationIcon name={item}/><span>{item === "Transactions" ? <><span className="desktop-label">Transactions</span><span className="mobile-label">Activity</span></> : item}</span></button>)}</nav><div className="sidebar-foot"><div className="help-card"><span>?</span><strong>Need a hand?</strong><p>Find quick answers and guides.</p><button>Visit help centre</button></div><button className="profile" onClick={onSignOut}><span className="avatar">{displayName.slice(0, 2).toUpperCase()}</span><span><strong>{displayName}</strong><small>Sign out</small></span><b>⋮</b></button></div></aside>;
}

export function DashboardHeader({ active, displayName, notifications, accountOpen, onAccountToggle, onPayment, onSignOut }: { active: string; displayName: string; notifications: DashboardNotification[]; accountOpen: boolean; onAccountToggle: () => void; onPayment: () => void; onSignOut: () => void }) {
  const [notificationsOpen, setNotificationsOpen] = useState(false);
  const [read, setRead] = useState(false);
  const hasUnread = notifications.length > 0 && !read;
  const notificationProps = { notifications, open: notificationsOpen, hasUnread, onToggle: () => setNotificationsOpen((value) => !value), onRead: () => setRead(true) };
  return <header className="app-header"><div className="header-title"><span className="mobile-brand" aria-hidden="true">E</span><div><p>Your workspace</p><h1>{active}</h1></div></div><div className="header-actions"><NotificationControl {...notificationProps}/><button className="primary payment-button" onClick={onPayment}>+ Payment</button></div><div className="mobile-tools"><NotificationControl {...notificationProps}/><div className="mobile-account"><button className="mobile-account-trigger" aria-label="Open account menu" aria-expanded={accountOpen} onClick={onAccountToggle}><span className="avatar">{displayName.slice(0, 2).toUpperCase()}</span><span className="account-chevron">⌄</span></button>{accountOpen && <div className="mobile-account-menu"><div><strong>{displayName}</strong><small>Signed in</small></div><button onClick={onPayment}>Add payment</button><button className="signout-button" onClick={onSignOut}>Sign out</button></div>}</div></div></header>;
}
