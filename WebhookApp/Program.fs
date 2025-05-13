open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.OpenApi.Models
open Microsoft.AspNetCore.Http

open WebhookApp.Webhook
open WebhookApp.Types

[<EntryPoint>]
let main args =
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

    app.MapPost(
        "/webhook", 
        Func<PaymentPayload, IResult>(handlePayment)
    ) |> ignore

    app.Run()
    0
