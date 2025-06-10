namespace WebhookApp

open System
open WebhookApp.Types
open WebhookApp.Database

module Validator =

    let private checkEvent payload =
        if payload.event = "payment_success" then Ok payload
        else Error "Invalid event type"

    let private checkTransactionId payload =
        if String.IsNullOrWhiteSpace(payload.transaction_id) then Error "Missing transaction_id"
        else Ok payload

    let private checkCurrency payload =
        if String.IsNullOrWhiteSpace(payload.currency) then Error "Missing currency"
        else Ok payload

    let private checkAmount payload =
        if payload.amount > 0M then Ok payload
        else Error "Amount must be greater than 0"

    let private checkTimestamp payload =
        if payload.timestamp > DateTime.MinValue then Ok payload
        else Error "Invalid timestamp"

    let private checkIdempotency payload =
        if transactionExists payload.transaction_id then
            Error "Transaction already exists"
        else
            Ok payload

    let validate payload =
        payload
        |> checkEvent
        |> Result.bind checkTransactionId
        |> Result.bind checkCurrency
        |> Result.bind checkAmount
        |> Result.bind checkTimestamp
        |> Result.bind checkIdempotency
