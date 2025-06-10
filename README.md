# F# Webhook Server for Payment Gateway Integration

This project implements a **payment webhook receiver** in **F#** using **ASP.NET Core Minimal API**, designed to simulate and validate payment confirmation flows from external gateways (e.g., PayPal, MercadoPago). It supports **header validation, payload integrity checks, idempotency via SQLite**, and dual-port **HTTP and HTTPS endpoints**.

---

## Overview

Webhooks are used by payment platforms to notify systems of completed transactions. This server simulates a full webhook integration by exposing a `/webhook` endpoint that:

* Accepts `POST` requests containing JSON payment data.
* Verifies request authenticity via a header token.
* Validates payload structure and semantics.
* Confirms or cancels transactions via callback URLs.
* Prevents duplicate transaction processing.
* Supports both **HTTP (port 5000)** and **HTTPS (port 5002)** endpoints.

---

## Features

✔️ Functional-style validation with `Result` chaining
✔️ SQLite-based persistence to ensure idempotency
✔️ Header-based authentication (`X-Webhook-Token`)
✔️ Semantic error handling with appropriate HTTP status codes
✔️ Compatibility with the professor’s Python test script
✔️ Clean dual-port setup with self-signed HTTPS certs

---

## Project Structure

* **`WebhookApp/`** – the main F# application

  * `Program.fs` – defines routing and configures HTTP/HTTPS
  * `Webhook.fs` – core logic to process and handle payments
  * `Validator.fs` – pure functional validation pipeline
  * `ExternalAPI.fs` – simulates confirmation/cancellation callbacks
  * `Database.fs` – uses SQLite to store processed transactions
  * `Types.fs` – defines the `PaymentPayload` type

* **`test/test_webhook.py`** – official test suite used to validate correctness against required behavior.

---

## Webhook Flow

1. A payment provider sends a POST request to `/webhook`.
2. The server checks the `X-Webhook-Token` header.
3. The JSON payload is validated:

   * Required fields: `event`, `transaction_id`, `amount`, `currency`, `timestamp`
   * `amount` must be positive
   * `timestamp` must be a valid ISO datetime
4. The server:

   * Confirms the transaction via `/confirmar` if valid.
   * Cancels via `/cancelar` if invalid and the `transaction_id` is present.
   * Ignores silently if the token is invalid.
5. Processed transactions are stored in a local SQLite DB to avoid duplication.

---

## Running the Project

### Prerequisites

* [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* Python 3.8+
* `pip install fastapi uvicorn requests`

---

### Setup

```bash
# Clone the repo
git clone git@github.com:JoaoLucasMBC/webhook-fs.git
cd webhook-fs

# Trust the HTTPS certificate (only once)
dotnet dev-certs https --trust

# Restore .NET dependencies
dotnet restore
```

---

### Run the Server

```bash
dotnet run --project WebhookApp
```

This will start:

* `http://localhost:5000` for HTTP (used by the test script)
* `https://localhost:5002` for HTTPS

---

## Testing

Run the provided tests:

```bash
cd test
python test_webhook.py
```

✅ The server is expected to pass all 6 webhook handling tests.

---

## Endpoints

### POST `/webhook`

Consumes a JSON payload like:

```json
{
  "event": "payment_success",
  "transaction_id": "abc123",
  "amount": 49.90,
  "currency": "BRL",
  "timestamp": "2025-05-11T16:00:00Z"
}
```

**Required header:**

```
X-Webhook-Token: eu-amo-prog-func
```

Returns:

* `200 OK` – on successful confirmation
* `409 Conflict` – on duplicate transaction
* `422 Unprocessable Entity` – on invalid payload
* `400 Bad Request` – if JSON is malformed
* `202 Accepted` – if token is invalid

---

## AI Usage

AI tools were used to:

* Guide the functional design of validation chaining.
* Draft SQL persistence logic in F# idioms.
* Debug HTTP/HTTPS dual-port deployment using minimal API.
* Generate this README and ensure clarity and professionalism.

All AI-generated output was reviewed and validated by the developer.