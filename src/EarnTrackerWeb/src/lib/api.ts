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
  status: "Active" | "Achieved" | "Expired";
  isAchieved: boolean;
};

export type Overview = {
  totals: { currency: string; gross: number; fees: number; net: number }[];
  incomeSources: IncomeSource[];
  financialGoals: Goal[];
};

export type PayPalOrder = {
  id: string;
  status: string;
  links?: { href: string; rel: string; method?: string }[];
  purchase_units?: Array<{
    description?: string;
    amount?: { currency_code: string; value: string };
    payments?: { captures?: Array<{ id: string; status: string }> };
  }>;
};

// Keep browser requests on the frontend origin. The server bridge forwards
// them to Render, avoiding browser CORS and mixed-content failures.
const API_URL = "/api/backend";

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
    const validationMessage = body?.errors
      ? Object.values(body.errors).flat().filter((item): item is string => typeof item === "string").join(" ")
      : "";
    const fallback = response.status === 401
      ? "Your session has expired. Please sign in again."
      : response.status === 404
        ? "This API endpoint is not available. Stop and restart the backend, then try again."
        : `Request failed with status ${response.status}. Please try again.`;
    throw new Error(body?.detail || validationMessage || body?.title || fallback);
  }
  return response.json();
}

export const api = {
  login: (email: string, password: string) =>
    request<AuthSession>("/api/auth/login", { method: "POST", body: JSON.stringify({ email, password }) }),
  register: (name: string, email: string, password: string) =>
    request<AuthSession>("/api/auth/register", { method: "POST", body: JSON.stringify({ name, email, password }) }),
  overview: (token: string) => request<Overview>("/api/library/overview", {}, token),
  createGoal: (goal: { name: string; targetAmount: number; currency: string; startDate: string; targetDate: string }, token: string) =>
    request<Goal>("/api/library/goals", {
      method: "POST",
      body: JSON.stringify(goal),
    }, token),
  createDemoPayment: (amount: number, currency: string, description: string, token: string) =>
    request<Transaction & { isDemo: boolean }>("/api/integrations/demo/payments", {
      method: "POST",
      body: JSON.stringify({ amount, currency, description }),
    }, token),
  createPayPalOrder: (amount: number, currency: string, description: string, token: string) =>
    request<PayPalOrder>("/api/integrations/paypal/orders", {
      method: "POST",
      body: JSON.stringify({ amount, currency, description }),
    }, token),
  getPayPalOrder: (orderId: string, token: string) =>
    request<PayPalOrder>(`/api/integrations/paypal/orders/${encodeURIComponent(orderId)}`, {}, token),
  capturePayPalOrder: (orderId: string, token: string) =>
    request<PayPalOrder>(`/api/integrations/paypal/orders/${encodeURIComponent(orderId)}/capture`, {
      method: "POST",
    }, token),
  verify: (provider: "paypal" | "paystack", reference: string, token: string) =>
    request<Record<string, unknown>>(
      provider === "paypal"
        ? `/api/integrations/paypal/captures/${encodeURIComponent(reference)}`
        : `/api/integrations/paystack/transactions/${encodeURIComponent(reference)}`,
      {}, token,
    ),
};
