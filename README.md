# F# Webhook Server for Payment Gateway Integration

This project implements a **payment webhook receiver** in **F#** using **ASP.NET Core Minimal API**, designed to simulate and validate payment confirmation from external gateways. It implements features such as **header validation, payload integrity checks, idempotency via SQLite**, and dual-port **HTTP and HTTPS endpoints**.

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
✔️ Compatibility with the Python test script
✔️ Dual-port setup with self-signed HTTPS certs

---

## Project Structure

* **`WebhookApp/`** – the main F# application

  * `Program.fs` – defines routing and configures HTTP/HTTPS
  * `Webhook.fs` – core logic to process and handle payments
  * `Validator.fs` – pure functional validation pipeline
  * `ExternalAPI.fs` – does confirmation/cancellation callbacks
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

   * If a transaction with the same `transaction_id` already exists, it returns a `409 Conflict`.  
   * The DB and table are created and cleared automatically when running the server for testing.

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

# Trust the HTTPS certificate (this is necessary to have HTTPS working)
dotnet dev-certs https --trust

# Restore .NET dependencies
dotnet restore

# Build the project
dotnet build ./WebhookApp
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
* `400 Bad Request` – ONLY if JSON is explicitly malformed (missing braces, etc., which makes impossible to even parse it)
* `202 Accepted` – because the webhook should not return errors to the payment provider, it never returns `400 Bad Request` for validation errors, but rather `202 Accepted` to indicate the payload was accepted for processing. The problems with the payload or transaction are then handled internally.

### `/swagger`

The server also exposes a Swagger UI at `/swagger` for interactive API documentation. You can access it at:

`https://localhost:5002/swagger` (for HTTPS) or `http://localhost:5000/swagger` (for HTTP) to explore the endpoints and test them interactively.

---

## Checklist

- [x] - Se uma transação estiver ok, deve-se retornar 200 e fazer um request em uma url de confirmação

* The confirmation URL is defined in the `confirmar` function in `ExternalAPI.fs`, which is called when a transaction is valid.

- [x] - Se uma transação não estiver ok, não deve retornar 400

* As described before, the server never returns `400 Bad Request` for validation errors. Instead, it returns `202 Accepted` to indicate the payload was accepted for processing, and the issues are handled internally.

- [x] - Se alguma informação estiver errada (ex: valor), deve-se cancelar a transação fazendo um request

* This is handled by the `Validator.fs` module, which checks the payload structure and semantics. If any field is invalid (e.g., `amount` is negative), it triggers a cancellation request via the `cancelar` function in `ExternalAPI.fs`.

- [x] - Se alguma informação faltante (exceto transaction_id), deve-se cancelar a transação fazendo um request

* The `Validator.fs` module when there is an Error, it pushes in the monad the reason for the error, such that the `Webhook.fs` can handle each reason accordingly. If any required field is missing (except `transaction_id`), it will trigger a cancellation request via the `cancelar` function in `ExternalAPI.fs`. If the `transaction_id` is missing, it will not cancel but rather ignore the transaction silently.

- [x] - Se o token estiver errado, é uma transação falsa e deve-se ignorá-la

* The server checks the `X-Webhook-Token` header against a predefined value. If it does not match, the transaction is ignored silently.

### Extras

- [x] - O serviço deve verificar a integridade do payload

* The payload integrity is verified by checking the required fields and their types in the `Validator.fs` module. If any field is missing or has an incorrect type, it will trigger a cancellation request.

* The only situation in which the server returns `400 Bad Request` is when the JSON payload is malformed (e.g., missing braces), which prevents it from being parsed at all. In all other cases, it returns `202 Accepted` to indicate the payload was accepted for processing.

- [x] - O serviço deve implementar algum mecanismo de veracidade da transação

* The server uses a combination of header validation (`X-Webhook-Token`) and payload structure checks to ensure the authenticity of the transaction. If the token is invalid, the transaction is ignored.

- [x] - O serviço deve cancelar a transação em caso de divergência

* If any validation fails (e.g., missing fields, incorrect values), the server will cancel the transaction by making a request to the cancellation URL defined in the `cancelar` function in `ExternalAPI.fs`.

- [x] - O serviço deve confirmar a transação em caso de sucesso

* If the transaction is valid, the server will confirm it by making a request to the confirmation URL defined in the `confirmar` function in `ExternalAPI.fs`.

- [x] - O serviço deve persistir a transação em um BD

* The server uses SQLite to persist transactions in the `Database.fs` module. It checks for existing transactions by `transaction_id` to ensure idempotency. If a transaction with the same `transaction_id` already exists, it ignores the new transaction and returns a `202` like the other valid transactions.

- [x] - Implementar um serviço HTTPS

* The server is configured to run on both HTTP (port 5000) and HTTPS (port 5002). The HTTPS endpoint uses a self-signed certificate generated by the .NET CLI. This is done by running `dotnet dev-certs https --trust` to trust the certificate locally.

* The server can be accessed via `https://localhost:5002` for secure communication. Most browsers will require you to accept the self-signed certificate when accessing the HTTPS endpoint for the first time because it is not signed by a trusted authority, but it is still HTTPS.

* The configuration is done in the `appsettings.json` file, where the HTTPS port is set to `5002` and the HTTP port is set to `5000`.

* For testing, you can access the Swagger UI at `https://localhost:5002/swagger` to see the available endpoints and test them interactively.

---

## AI Usage

AI tools were used to:

* Guide the functional design of validation chaining.
* Draft SQL persistence logic in F#.
* Debug HTTP/HTTPS dual-port deployment using minimal API.
* Generate this README and ensure clarity.

All AI-generated output was reviewed and validated by the developer.