using System.Text.Json;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, SourceGenerationContext.Default);
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

builder.Services.AddNpgsqlDataSource(
    Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? "ERRO de connection string!!!"
);

var app = builder.Build();

var clientes = new Dictionary<int, int>
{
    {1,   1000 * 100},
    {2,    800 * 100},
    {3,  10000 * 100},
    {4, 100000 * 100},
    {5,   5000 * 100}
};

var dbFuncs = new Dictionary<string, string>
        {
            { "c", "creditar" },
            { "d", "debitar" }
        };

app.MapGet("/clientes/{id}/extrato", async (int id, NpgsqlConnection conn) =>
{
    if (!clientes.ContainsKey(id))
        return Results.NotFound();

    await using (conn)
    {
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"(select valor, 'saldo' as tipo, 'saldo' as descricao, now() as realizada_em
                            from saldos
                            where cliente_id = $1)
                            union all
                            (select valor, tipo, descricao, realizada_em
                            from transacoes
                            where cliente_id = $1
                            order by id desc limit 10)";
        cmd.Parameters.AddWithValue(id);

        using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();

        ExtratoSaldoResponse extrato = new(reader.GetInt32(0), reader.GetDateTime(3), clientes[id]);
        IList<ExtratoUltimasTransacoesResponse> transacoes = new List<ExtratoUltimasTransacoesResponse>();

        while (await reader.ReadAsync())
            transacoes.Add(
                new(reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetDateTime(3)));

        return Results.Ok(new ExtratoResponse(extrato, transacoes));
    }
});

app.MapPost("/clientes/{id}/transacoes", async (int id, TransacaoRequest transacao, NpgsqlConnection conn) =>
{
    if (!clientes.ContainsKey(id))
        return Results.NotFound();

    if (!transacao.Valida())
        return Results.UnprocessableEntity();

    await using (conn)
    {
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"select novo_saldo, possui_erro, mensagem from {dbFuncs[transacao.Tipo]}($1, $2, $3)";
        cmd.Parameters.AddWithValue(id);
        cmd.Parameters.AddWithValue(transacao.ValorTratado);
        cmd.Parameters.AddWithValue(transacao.Descricao);
        using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();

        if (reader.GetBoolean(1))
            return Results.UnprocessableEntity();

        return Results.Ok(new TransacaoResponse(reader.GetInt32(0), clientes[id]));
    }
});

app.MapPost("/admin/db-reset", async (NpgsqlConnection conn) =>
{
    await using (conn)
    {
        await conn.OpenAsync();
        await using var cmd = conn.CreateBatch();
        cmd.BatchCommands.Add(new NpgsqlBatchCommand("update saldos set valor = 0"));
        cmd.BatchCommands.Add(new NpgsqlBatchCommand("truncate table transacoes"));
        using var reader = await cmd.ExecuteReaderAsync();
        return Results.Ok("db reset!");
    }
});

app.Run();
