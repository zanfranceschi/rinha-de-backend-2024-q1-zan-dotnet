using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

public record ExtratoSaldoResponse(int Total, DateTime DataExtrato, int Limite);
public record ExtratoUltimasTransacoesResponse(int Valor, string Tipo, string Descricao, DateTime RealizadaEm);
public record ExtratoResponse(ExtratoSaldoResponse Saldo, IList<ExtratoUltimasTransacoesResponse> UltimasTransacoes);
public class TransacaoRequest
{
    private readonly static string[] TIPOS = ["c", "d"];
    public object? Valor { get; set; }
    public int ValorTratado;
    public string? Tipo { get; set; }
    public string? Descricao { get; set; }
    public bool Valida()
    {
        return TIPOS.Contains(Tipo)
            && !string.IsNullOrEmpty(Descricao)
            && Descricao.Length <= 10
            && Valor is not null
            && int.TryParse(Valor.ToString(), out ValorTratado);
    }
}
public record TransacaoResponse(int Saldo, int Limite);

[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ExtratoResponse))]
[JsonSerializable(typeof(ExtratoSaldoResponse))]
[JsonSerializable(typeof(ExtratoUltimasTransacoesResponse))]
[JsonSerializable(typeof(IList<ExtratoUltimasTransacoesResponse>))]
[JsonSerializable(typeof(TransacaoRequest))]
[JsonSerializable(typeof(TransacaoResponse))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(int))]
public partial class SourceGenerationContext 
    : JsonSerializerContext { }
