namespace WebhookApp

open WebhookApp.Types
open WebhookApp.Validator
open WebhookApp.ExternalAPI
open WebhookApp.Database

open Microsoft.AspNetCore.Http
open System.Threading.Tasks

module Webhook =

    let handlePayment (payload: PaymentPayload) : Task<IResult> =
        task {
            printfn "Received payment %s for %M" payload.transaction_id payload.amount

            match validate payload with
            | Error "Transaction already exists" ->
                return Results.Accepted ("", {| message = "Duplicate transaction" |})

            | Error "Missing transaction_id" ->
                // Cannot cancel transaction without transaction_id
                return Results.Accepted ("", {| message = "Missing transaction_id" |})

            | Error reason ->
                let! res = cancelTransaction payload
                return Results.Accepted ("", {| message = reason |})

            | Ok validPayload ->
                let! res = confirmTransaction validPayload
                insertTransaction validPayload |> ignore
                return Results.Ok {| message = "Transaction confirmed" |}

        }
