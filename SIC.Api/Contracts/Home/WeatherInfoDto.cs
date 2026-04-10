namespace SIC.Api.Contracts.Home;

public sealed class WeatherInfoDto
{
    public int EstabelecimentoID { get; set; }
    public string Cidade { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public decimal Temperatura { get; set; }
    public decimal Sensacao { get; set; }
    public int Umidade { get; set; }
    public decimal VelocidadeVento { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime DtUltimaAtualizacao { get; set; }
}
