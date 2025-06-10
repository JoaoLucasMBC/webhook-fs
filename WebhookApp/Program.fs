open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.OpenApi.Models
open Microsoft.AspNetCore.Http
open System.Threading.Tasks


open WebhookApp.Webhook
open WebhookApp.Types
open WebhookApp.Database


[<EntryPoint>]
let main args =

    let secretKey = "eu-amo-prog-func"

    let builder = WebApplication.CreateBuilder(args)

    // ── Register Swagger/OpenAPI services before Build() ──
    builder.Services.AddEndpointsApiExplorer() |> ignore
    builder.Services.AddSwaggerGen(fun c ->
        c.SwaggerDoc("v1", OpenApiInfo(Title = "Webhook API", Version = "v1"))
    ) |> ignore

    let app = builder.Build()

    // ── Enable middleware ──
    if app.Environment.IsDevelopment() then
        app.UseSwagger()   |> ignore
        app.UseSwaggerUI() |> ignore

    // ── Your endpoints ──
    app.MapGet("/", Func<string>(fun () -> "Hello World!")) |> ignore

    app.MapPost("/webhook", Func<HttpRequest, Task<IResult>>(fun req -> task {
        let token =
            match req.Headers.TryGetValue("X-Webhook-Token") with
            | true, values -> values.ToString()
            | _ -> ""

        if token <> secretKey then
            return Results.Accepted ("", {| message = "Transaction ignored because of invalid token" |})
        else
            try
                let! payload = req.ReadFromJsonAsync<PaymentPayload>()
                return! handlePayment payload
            with ex ->
                return Results.BadRequest {| message = "Invalid payload format" |}
    })) |> ignore


    initializeDatabase ()

    app.Run("http://localhost:5000")
    0