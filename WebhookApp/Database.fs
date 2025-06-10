namespace WebhookApp

open System
open Microsoft.Data.Sqlite
open WebhookApp.Types

module Database =

    let private connectionString = "Data Source=webhook.db"

    let initializeDatabase () =
        use conn = new SqliteConnection(connectionString)
        conn.Open()

        // Create the table if it doesn't exist
        let createCmd = conn.CreateCommand()
        createCmd.CommandText <- """
            CREATE TABLE IF NOT EXISTS transactions (
                transaction_id TEXT PRIMARY KEY,
                event TEXT NOT NULL,
                amount REAL NOT NULL,
                currency TEXT NOT NULL,
                timestamp TEXT NOT NULL
            )
        """
        createCmd.ExecuteNonQuery() |> ignore

        // Clear all rows from the table
        let clearCmd = conn.CreateCommand()
        clearCmd.CommandText <- "DELETE FROM transactions"
        clearCmd.ExecuteNonQuery() |> ignore


    let insertTransaction (payload: PaymentPayload) =
        use conn = new SqliteConnection(connectionString)
        conn.Open()
        let cmd = conn.CreateCommand()
        cmd.CommandText <- """
            INSERT INTO transactions (transaction_id, event, amount, currency, timestamp)
            VALUES ($id, $event, $amount, $currency, $timestamp)
        """
        cmd.Parameters.AddWithValue("$id", payload.transaction_id) |> ignore
        cmd.Parameters.AddWithValue("$event", payload.event) |> ignore
        cmd.Parameters.AddWithValue("$amount", payload.amount) |> ignore
        cmd.Parameters.AddWithValue("$currency", payload.currency) |> ignore
        cmd.Parameters.AddWithValue("$timestamp", payload.timestamp.ToString("o")) |> ignore
        try
            cmd.ExecuteNonQuery() |> ignore
            true
        with
        | :? SqliteException -> false  // Transaction already exists

    let transactionExists (transactionId: string) =
        use conn = new SqliteConnection(connectionString)
        conn.Open()
        let cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT COUNT(*) FROM transactions WHERE transaction_id = $id"
        cmd.Parameters.AddWithValue("$id", transactionId) |> ignore
        let count = cmd.ExecuteScalar() :?> int64
        count > 0L
