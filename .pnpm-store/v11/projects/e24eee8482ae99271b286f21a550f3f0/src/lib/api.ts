export type AuthSession = {
  tag: string;
  name: string;
  token: string;
  refreshToken: string;
};

export type Transaction = {
  id: string;
  externalId: string;
  amount: number;
  fee: number;
  currency: string;
  status: string;
  description?: string;
  occurredAt: string;
};

export type IncomeSource = {
  id: string;
  name: string;
  provider: string;
  currency: string;
  isActive: boolean;
  transactions: Transaction[];
};

export type Goal = {
  id: string;
  name: string;
  targetAmount: number;
  currentAmount: number;
  progressPercentage: number;
  currency: string;
  startDate: string;
  targetDate: string;
};

export type Overview = {
  totals: { currency: string; gross: number; fees: number; net: number }[];
  incomeSources: IncomeSource[];
  financialGoals: Goal[];
};

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5048";

async function request<T>(path: string, options: RequestInit = {}, token?: string): Promise<T> {
  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  });

  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new Error(body?.detail ?? body?.title ?? "Something went wrong. Please try again.");
  }
  return response.json();
}

export const api = {
  login: (email: string, password: string) =>
    request<AuthSession>("/api/auth/login", { method: "POST", body: JSON.stringify({ email, password }) }),
  register: (name: string, email: string, password: string) =>
    request<AuthSession>("/api/auth/register", { method: "POST", body: JSON.stringify({ name, email, password }) }),
  overview: (token: string) => request<Overview>("/api/library/overview", {}, token),
  verify: (provider: "paypal" | "paystack", reference: string, token: string) =>
    request<Record<string, unknown>>(
      provider === "paypal"
        ? `/api/integrations/paypal/captures/${encodeURIComponent(reference)}`
        : `/api/integrations/paystack/transactions/${encodeURIComponent(reference)}`,
      {}, token,
    ),
};
