namespace WebhookApp

open WebhookApp.Types
open Microsoft.AspNetCore.Http

module Webhook =
    let handlePayment (payload: PaymentPayload) =
        // TEMPLATE
        printfn "Received payment %s for %M"
                payload.transactionId
                payload.amount

        // return an HTTP 200 OK with some body, or 400 on error
        Results.Ok {| message = "Processed" |}
