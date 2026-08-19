# EarnTracker API Reference

Local base URL: `http://localhost:5048`

Scalar: `http://localhost:5048/scalar/v1`

Except for health, registration, login, and refresh, routes require:

```http
Authorization: Bearer ACCESS_TOKEN
```

In Scalar, choose **Authentication** → **Bearer** and paste only the EarnTracker access token.

## Routes

| Method | Route | Auth | Purpose |
| --- | --- | --- | --- |
| GET | `/health` | No | Health status |
| POST | `/api/auth/register` | No | Register and issue tokens |
| POST | `/api/auth/login` | No | Log in and issue tokens |
| POST | `/api/auth/refresh` | No | Rotate token pair |
| GET | `/api/auth/me` | Yes | Current user |
| GET | `/api/library/overview` | Yes | Dashboard data |
| POST | `/api/library/goals` | Yes | Create goal |
| POST | `/api/integrations/paypal/orders` | Yes | Create Sandbox order |
| GET | `/api/integrations/paypal/orders/{orderId}` | Yes | Get order/import captures |
| POST | `/api/integrations/paypal/orders/{orderId}/capture` | Yes | Capture and record earning |
| GET | `/api/integrations/paypal/captures/{captureId}` | Yes | Get and record capture |
| POST | `/api/integrations/paystack/transactions` | Yes | Initialize Paystack test checkout |
| GET | `/api/integrations/paystack/transactions/{reference}` | Yes | Verify and record Paystack payment |

## Health

### `GET /health`

```json
{
  "status": "Healthy",
  "service": "EarnTrackerApi",
  "timestamp": "2026-08-17T12:00:00+00:00"
}
```

## Authentication

### `POST /api/auth/register`

```json
{
  "name": "Ada Lovelace",
  "email": "ada@example.com",
  "password": "Earn#2026"
}
```

`201 Created`:

```json
{
  "tag": "USER_GUID",
  "name": "Ada Lovelace",
  "token": "JWT_ACCESS_TOKEN",
  "refreshToken": "RAW_REFRESH_TOKEN"
}
```

Passwords require 8–12 characters, uppercase, lowercase, a number, and a special character.

### `POST /api/auth/login`

```json
{
  "email": "ada@example.com",
  "password": "Earn#2026"
}
```

Returns `200 OK` with the registration token shape.

### `POST /api/auth/refresh`

```json
{
  "refreshToken": "REFRESH_TOKEN_FROM_LOGIN"
}
```

Successful rotation revokes the supplied token. Store both replacement tokens.

### `GET /api/auth/me`

```json
{
  "id": "USER_GUID",
  "email": "ada@example.com",
  "displayName": "Ada Lovelace",
  "createdAt": "2026-08-17T12:00:00+00:00"
}
```

## Dashboard and goals

### `GET /api/library/overview`

```json
{
  "totals": [
    { "currency": "USD", "gross": 100, "fees": 3.49, "net": 96.51 }
  ],
  "incomeSources": [
    {
      "id": "SOURCE_GUID",
      "name": "PayPal",
      "provider": "PayPal",
      "currency": "USD",
      "isActive": true,
      "transactions": [
        {
          "id": "TRANSACTION_GUID",
          "externalId": "PAYPAL_CAPTURE_ID",
          "amount": 100,
          "fee": 3.49,
          "currency": "USD",
          "status": "Completed",
          "description": "Website project",
          "occurredAt": "2026-08-17T12:00:00+00:00"
        }
      ]
    }
  ],
  "financialGoals": [
    {
      "id": "GOAL_GUID",
      "name": "August income goal",
      "targetAmount": 1000,
      "currentAmount": 96.51,
      "progressPercentage": 9.65,
      "currency": "USD",
      "startDate": "2026-08-01",
      "targetDate": "2026-08-31",
      "status": "Active",
      "isAchieved": false
    }
  ]
}
```

### `POST /api/library/goals`

```json
{
  "name": "September income goal",
  "targetAmount": 5000,
  "currency": "USD",
  "startDate": "2026-09-01",
  "targetDate": "2026-09-30"
}
```

Returns `201 Created`. Target amount must be positive, currency must have three letters, and target date cannot precede start date.

## PayPal Sandbox

### `POST /api/integrations/paypal/orders`

```json
{
  "amount": 10,
  "currency": "USD",
  "description": "Freelancer earnings tracker sandbox test"
}
```

Copy the response's top-level order `id`. Open the URL whose `rel` is `approve` and sign in as the personal sandbox buyer.

### `GET /api/integrations/paypal/orders/{orderId}`

Use after buyer approval. Expected pre-capture status is `APPROVED`. Completed captures already embedded in an order response are imported for the authenticated user.

### `POST /api/integrations/paypal/orders/{orderId}/capture`

No body is needed. Call after approval. A successful response is normally `COMPLETED`; its capture ID is:

```text
purchase_units[0].payments.captures[0].id
```

The backend records completed captures and the frontend refreshes the overview.

### `GET /api/integrations/paypal/captures/{captureId}`

Gets a capture. A completed capture is inserted or updated for the authenticated user. Repeating the request does not duplicate that capture within the same income source.

## Paystack

### `POST /api/integrations/paystack/transactions`

Initialize a test transaction in Scalar:

```json
{
  "email": "buyer@example.com",
  "amount": 1000,
  "currency": "NGN",
  "description": "Paystack Scalar test",
  "callbackUrl": "http://localhost:3000"
}
```

`amount` is entered in the main currency unit. The backend converts it to Paystack subunits. Copy `data.authorization_url` from the response into a browser, and save `data.reference` for verification.

Complete checkout with Paystack test card details:

```text
Card: 4084 0840 8408 4081
Expiry: any future date
CVV: 408
```

### `GET /api/integrations/paystack/transactions/{reference}`

After checkout succeeds, paste the saved reference into this route. A successful transaction initialized by the same signed-in EarnTracker user is recorded in dashboard earnings. Repeating verification updates the existing entry instead of creating a duplicate.

Finally call `GET /api/library/overview` and look for the `Paystack` income source. Transactions not initialized through EarnTracker are returned by Paystack but are not imported, because they are not bound to the current user.

## Status codes

| Status | Meaning |
| --- | --- |
| 200 | Success |
| 201 | Created |
| 400 | Validation failed |
| 401 | Missing, invalid, or expired access token |
| 404 | Route or resource not found |
| 409 | Registration email exists |
| 502 | Provider request failed |
| 500 | Unexpected server error; inspect logs |

Application errors use `application/problem+json`. Validation errors include an `errors` object keyed by field.

## PowerShell example

```powershell
$token = "YOUR_EARNTRACKER_ACCESS_TOKEN"
Invoke-RestMethod `
  -Uri "http://localhost:5048/api/library/overview" `
  -Headers @{ Authorization = "Bearer $token" }
```

## Token distinction

- **EarnTracker access token:** authenticates users to this API; paste this into Scalar.
- **EarnTracker refresh token:** rotates an expired access token.
- **PayPal OAuth token:** acquired and cached internally by the backend.

Never put a PayPal client secret or PayPal OAuth token into Scalar's EarnTracker Bearer field.
