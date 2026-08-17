# EarnTracker Web

The web dashboard for EarnTracker. It uses Next.js and connects to the EarnTracker API for accounts, earnings, goals, PayPal Sandbox orders, and Paystack payment checks.

## Run locally

Requirements:

- Node.js 20.9 or newer
- pnpm 11
- EarnTracker API running locally

Create `.env.local` from `.env.example`, then run:

```bash
pnpm install --frozen-lockfile
pnpm dev
```

Open `http://localhost:3000`.

## Checks

```bash
pnpm lint
pnpm build
```

The frontend workflow runs both checks when frontend files change in a pull request or are pushed to `main`.

## Deploy to Vercel

Import the repository into Vercel and use these project settings:

- Framework: Next.js
- Root Directory: `src/EarnTrackerWeb`
- Install Command: `pnpm install --frozen-lockfile`
- Build Command: `pnpm build`
- Output Directory: leave empty

Add this environment variable for Production, Preview, and Development:

```text
NEXT_PUBLIC_API_URL=https://your-api-domain.example.com
```

The value must be the public HTTPS address of the deployed EarnTracker API. Do not use `localhost` in Vercel.

The API must also allow the Vercel site in its `AllowedOrigins` setting. Add the exact production address, for example:

```json
"AllowedOrigins": ["https://earn-tracker.example.com"]
```

Vercel's Git connection can create preview deployments for branches and production deployments from `main`. The GitHub workflow remains the quality check; it does not need Vercel account secrets.
