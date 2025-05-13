namespace WebhookApp

open System

module Types =

    type PaymentPayload = {
        event : string
        transactionId : string
        amount        : decimal
        currency        : string
        timestamp : DateTime
    }