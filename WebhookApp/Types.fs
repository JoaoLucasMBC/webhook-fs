namespace WebhookApp

open System

module Types =

    type PaymentPayload = {
        event : string
        transaction_id : string
        amount : decimal
        currency : string
        timestamp : DateTime
    }