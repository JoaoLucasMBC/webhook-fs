namespace WebhookApp

open System.Net.Http
open System.Text.Json
open System.Threading.Tasks
open WebhookApp.Types

module ExternalAPI =

    let private client = new HttpClient()

    let confirmTransaction (payload: PaymentPayload) : Task<HttpResponseMessage> =
        let url = "http://localhost:5001/confirmar"
        let json = JsonSerializer.Serialize(payload)
        let content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        // print the content for debugging
        printfn "Sending confirm request with content: %s" json
        client.PostAsync(url, content)

    let cancelTransaction (payload: PaymentPayload) : Task<HttpResponseMessage> =
        let url = "http://localhost:5001/cancelar"
        let json = JsonSerializer.Serialize(payload)
        let content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        // print the content for debugging
        printfn "Sending cancel request with content: %s" json
        client.PostAsync(url, content)