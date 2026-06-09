using System.Globalization;
using System.Text;

namespace SIC.Web.Services.Cotacao;

/// <summary>
/// Construtor de HTML para email de cotação.
/// Gera o HTML completo em memória, sem depender de arquivo em disco.
/// </summary>
public sealed class CotacaoEmailHtmlBuilder
{
    private readonly string _estabCor;

    public CotacaoEmailHtmlBuilder(string estabCor)
    {
        _estabCor = estabCor;
    }

    public string Construir(
        string titulo,
        string saudacao,
        string mensagem,
        DadosEmailInterno cot,
        string hash)
    {
        var sb = new StringBuilder();
        var ptBr = new CultureInfo("pt-BR");

        // DOCTYPE e abertura HTML
        sb.AppendLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"utf-8\">");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        sb.AppendLine("    <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">");
        sb.AppendLine($"    <title>{HtmlEncode(titulo)}</title>");
        sb.AppendLine(CssPrincipal());
        sb.AppendLine("</head>");
        sb.AppendLine("<body class=\"my-body\">");
        sb.AppendLine("    <div class=\"my-container\">");

        // Cabeçalho
        ConstruirCabecalho(sb, titulo, cot);

        // Mensagem
        ConstruirMensagem(sb, saudacao, mensagem);

        // Emitente
        ConstruirEmitente(sb, cot);

        // Destinatário
        ConstruirDestinatario(sb, cot);

        // Dados da Cotação
        ConstruirDadosCotacao(sb, cot);

        // Itens da Cotação
        ConstruirItensCotacao(sb, cot, ptBr);

        // Totais
        ConstruirTotais(sb, cot, ptBr);

        // Botão de Ação
        ConstruirBotaoAcao(sb, hash);

        sb.AppendLine("    </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private void ConstruirCabecalho(StringBuilder sb, string titulo, DadosEmailInterno cot)
    {
        sb.AppendLine("        <table class=\"my-panel\">");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-5\"></td>");
        sb.AppendLine("                <td class=\"my-col-5\">");
        sb.AppendLine("                    <h3 class=\"my-h3 my-text-right\">");
        sb.AppendLine($"                        <strong>{HtmlEncode(titulo)}</strong>");
        sb.AppendLine("                    </h3>");
        sb.AppendLine($"                    <h4 class=\"my-h4 my-text-right\">{HtmlEncode(cot.StatusNome)}</h4>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("        </table>");
        sb.AppendLine("        <br>");
    }

    private void ConstruirMensagem(StringBuilder sb, string saudacao, string mensagem)
    {
        sb.AppendLine("        <table class=\"my-panel\">");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <h5 class=\"my-text-left\">");
        sb.AppendLine($"                        <span class=\"my-marcador\">></span>");
        sb.AppendLine("                        <strong class=\"my-cab\">Mensagem</strong>");
        sb.AppendLine("                    </h5>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <table class=\"my-dados\">");
        sb.AppendLine("                        <tr>");
        sb.AppendLine("                            <td>");
        sb.AppendLine($"                                <strong>{HtmlEncode(saudacao)}</strong>");
        sb.AppendLine("                                <br><br>");
        sb.AppendLine($"                                {HtmlEncode(mensagem)}");
        sb.AppendLine("                                <br><br>");
        sb.AppendLine("                            </td>");
        sb.AppendLine("                        </tr>");
        sb.AppendLine("                    </table>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("        </table>");
        sb.AppendLine("        <br>");
    }

    private void ConstruirEmitente(StringBuilder sb, DadosEmailInterno cot)
    {
        sb.AppendLine("        <table class=\"my-panel\">");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <h5 class=\"my-text-left\">");
        sb.AppendLine("                        <span class=\"my-marcador\">></span>");
        sb.AppendLine("                        <strong class=\"my-cab\">Emitente</strong>");
        sb.AppendLine("                    </h5>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <table class=\"my-dados\">");
        sb.AppendLine("                        <tr>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Razão Social</span><br>{HtmlEncode(cot.EstabRazaoSocial)}</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">CNPJ</span><br>{FormatCnpj(cot.EstabCNPJ)}</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Inscr. Estadual</span><br>{HtmlEncode(cot.EstabInscrEstadual)}</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Telefone</span><br>{HtmlEncode(cot.EstabTelefone)}</td>");
        sb.AppendLine("                        </tr>");
        sb.AppendLine("                    </table>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <table class=\"my-dados\">");
        sb.AppendLine("                        <tr>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Endereço</span><br>{HtmlEncode(cot.EstabEndereco)}</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Número</span><br>{HtmlEncode(cot.EstabNumero)}&nbsp;</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Complemento</span><br>{HtmlEncode(cot.EstabComplemento)}&nbsp;</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Bairro</span><br>{HtmlEncode(cot.EstabBairro)}</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Cidade</span><br>{HtmlEncode(cot.EstabCidade)}</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">UF</span><br>{HtmlEncode(cot.EstabUF)}</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">CEP</span><br>{HtmlEncode(cot.EstabCEP)}</td>");
        sb.AppendLine("                        </tr>");
        sb.AppendLine("                    </table>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <table class=\"my-dados\">");
        sb.AppendLine("                        <tr>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Consultor</span><br>{HtmlEncode(cot.ConsultorNome)}</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">E-mail</span><br>{HtmlEncode(cot.ConsultorEmail)}</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Telefone</span><br>{HtmlEncode(cot.ConsultorTelefone)}&nbsp;</td>");
        sb.AppendLine("                        </tr>");
        sb.AppendLine("                    </table>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("        </table>");
        sb.AppendLine("        <br>");
    }

    private void ConstruirDestinatario(StringBuilder sb, DadosEmailInterno cot)
    {
        sb.AppendLine("        <table class=\"my-panel\">");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <h5 class=\"my-text-left\">");
        sb.AppendLine("                        <span class=\"my-marcador\">></span>");
        sb.AppendLine("                        <strong class=\"my-cab\">Destinatário</strong>");
        sb.AppendLine("                    </h5>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <table class=\"my-dados\">");
        sb.AppendLine("                        <tr>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Razão Social / Nome</span><br>{HtmlEncode(cot.ClienteRazaoSocial)}</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">CNPJ</span><br>{FormatCnpj(cot.ClienteCNPJ)}</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Telefone</span><br>{HtmlEncode(cot.ClienteTelefone)}</td>");
        sb.AppendLine("                        </tr>");
        sb.AppendLine("                    </table>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <table class=\"my-dados\">");
        sb.AppendLine("                        <tr>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Endereço</span><br>{HtmlEncode(cot.ClienteEndereco)}</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Número</span><br>{HtmlEncode(cot.ClienteNumero)}&nbsp;</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Complemento</span><br>{HtmlEncode(cot.ClienteComplemento)}&nbsp;</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Bairro</span><br>{HtmlEncode(cot.ClienteBairro)}</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Cidade</span><br>{HtmlEncode(cot.ClienteCidade)}</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">UF</span><br>{HtmlEncode(cot.ClienteUF)}</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">CEP</span><br>{HtmlEncode(cot.ClienteCEP)}</td>");
        sb.AppendLine("                        </tr>");
        sb.AppendLine("                    </table>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <table class=\"my-dados\">");
        sb.AppendLine("                        <tr>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Contato</span><br>{HtmlEncode(cot.ContatoNome)}</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">E-mail</span><br>{HtmlEncode(cot.ContatoEmail)}</td>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Ordem de Compra</span><br>{HtmlEncode(cot.OrdemCompra)}</td>");
        sb.AppendLine("                        </tr>");
        sb.AppendLine("                    </table>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("        </table>");
        sb.AppendLine("        <br>");
    }

    private void ConstruirDadosCotacao(StringBuilder sb, DadosEmailInterno cot)
    {
        sb.AppendLine("        <table class=\"my-panel\">");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <h5 class=\"my-text-left\">");
        sb.AppendLine("                        <span class=\"my-marcador\">></span>");
        sb.AppendLine("                        <strong class=\"my-cab\">Dados da Cotação</strong>");
        sb.AppendLine("                    </h5>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <table class=\"my-dados\">");
        sb.AppendLine("                        <tr>");
        sb.AppendLine($"                            <td class=\"my-text-center\"><span class=\"my-titulo-campo\">Data da Cotação</span><br>{DateTime.Now:dd/MM/yyyy}</td>");
        sb.AppendLine($"                            <td class=\"my-text-center\"><span class=\"my-titulo-campo\">Data de Validade</span><br>{HtmlEncode(cot.DataValidade)}</td>");
        sb.AppendLine($"                            <td class=\"my-text-left\"><span class=\"my-titulo-campo\">Condição de Pagamento</span><br>{HtmlEncode(cot.CondPagtoNome)}</td>");
        sb.AppendLine("                        </tr>");
        sb.AppendLine("                    </table>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <table class=\"my-dados\">");
        sb.AppendLine("                        <tr>");
        sb.AppendLine($"                            <td><span class=\"my-titulo-campo\">Observação</span><br>{HtmlEncode(cot.Obs)}</td>");
        sb.AppendLine("                        </tr>");
        sb.AppendLine("                    </table>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("        </table>");
        sb.AppendLine("        <br>");
    }

    private void ConstruirItensCotacao(StringBuilder sb, DadosEmailInterno cot, CultureInfo ptBr)
    {
        sb.AppendLine("        <table class=\"my-panel\">");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <h5 class=\"my-text-left\">");
        sb.AppendLine("                        <span class=\"my-marcador\">></span>");
        sb.AppendLine("                        <strong class=\"my-cab\">Itens da Cotação</strong>");
        sb.AppendLine("                    </h5>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <table class=\"my-dados\">");
        sb.AppendLine("                        <thead>");
        sb.AppendLine("                            <tr>");
        sb.AppendLine("                                <th class=\"my-text-center\">Foto</th>");
        sb.AppendLine("                                <th class=\"my-text-center\">Código</th>");
        sb.AppendLine("                                <th class=\"my-text-left\">Nome do Item</th>");
        sb.AppendLine("                                <th class=\"my-text-center\">NCM</th>");
        sb.AppendLine("                                <th class=\"my-text-center\">Preço</th>");
        sb.AppendLine("                                <th class=\"my-text-center\">IPI</th>");
        sb.AppendLine("                                <th class=\"my-text-center\">ST</th>");
        sb.AppendLine("                                <th class=\"my-text-center\">Quantidade</th>");
        sb.AppendLine("                                <th class=\"my-text-center\">Total</th>");
        sb.AppendLine("                            </tr>");
        sb.AppendLine("                        </thead>");
        sb.AppendLine("                        <tbody>");

        foreach (var item in cot.Itens ?? [])
        {
            sb.AppendLine("                            <tr>");
            sb.AppendLine("                                <td class=\"my-text-center\"></td>");
            sb.AppendLine($"                                <td class=\"my-text-center\">{HtmlEncode(item.CodItemBR)}</td>");
            sb.AppendLine($"                                <td class=\"my-text-left\">{HtmlEncode(item.DescrItemBR)}<br><span class=\"my-cinza\">{HtmlEncode(item.NmSegmento)}</span></td>");
            sb.AppendLine($"                                <td class=\"my-text-center\">{HtmlEncode(item.NCM)}</td>");
            sb.AppendLine($"                                <td class=\"my-text-center\">{item.PrecoItem.ToString("C", ptBr)}</td>");
            sb.AppendLine($"                                <td class=\"my-text-center\">{item.IPI.ToString("C", ptBr)}</td>");
            sb.AppendLine($"                                <td class=\"my-text-center\">{item.ST.ToString("C", ptBr)}</td>");
            sb.AppendLine($"                                <td class=\"my-text-center\"><strong>{item.Quantidade}</strong></td>");
            sb.AppendLine($"                                <td class=\"my-text-center\"><strong>{item.VlrUnitario.ToString("C", ptBr)}</strong></td>");
            sb.AppendLine("                            </tr>");
        }

        sb.AppendLine("                        </tbody>");
        sb.AppendLine("                    </table>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("        </table>");
        sb.AppendLine("        <br>");
    }

    private void ConstruirTotais(StringBuilder sb, DadosEmailInterno cot, CultureInfo ptBr)
    {
        var prazoEntrega = cot.DiasPrazoEntrega == 0
            ? "<span class='my-cinza'>A definir</span>"
            : $"{cot.DiasPrazoEntrega} Dias Úteis";

        var transportadora = string.IsNullOrWhiteSpace(cot.TransportadoraNome)
            ? "<span class='my-cinza'>Definida no momento do faturamento.</span>"
            : HtmlEncode(cot.TransportadoraNome);

        var tipoFrete = cot.VlrFrete > 0 ? "FOB" : "CIF";

        sb.AppendLine("        <table class=\"my-panel\">");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <h5 class=\"my-text-left\">");
        sb.AppendLine("                        <span class=\"my-marcador\">></span>");
        sb.AppendLine("                        <strong class=\"my-cab\">Totais da Cotação</strong>");
        sb.AppendLine("                    </h5>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <table class=\"my-dados\">");
        sb.AppendLine("                        <tr>");
        sb.AppendLine($"                            <td class=\"my-text-center\"><span class=\"my-titulo-campo\">Prazo de Entrega</span><br>{prazoEntrega}</td>");
        sb.AppendLine($"                            <td class=\"my-text-left\"><span class=\"my-titulo-campo\">Transportadora</span><br>{transportadora}</td>");
        sb.AppendLine($"                            <td class=\"my-text-center\"><span class=\"my-titulo-campo\">Frete</span><br>{tipoFrete}</td>");
        sb.AppendLine($"                            <td class=\"my-text-right\"><span class=\"my-titulo-campo\">Valor do Frete</span><br>{cot.VlrFrete.ToString("C", ptBr)}</td>");
        sb.AppendLine($"                            <td class=\"my-text-right\"><span class=\"my-titulo-campo\">Valor dos Produtos</span><br>{cot.TotalVendaSemFrete.ToString("C", ptBr)}</td>");
        sb.AppendLine("                        </tr>");
        sb.AppendLine("                    </table>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <table class=\"my-dados\">");
        sb.AppendLine("                        <tr>");
        sb.AppendLine($"                            <td class=\"my-text-right\"><span class=\"my-titulo-campo\">Valor Total da Cotação</span><br><span class=\"my-valores\"><strong>{cot.TotalVendaFinal.ToString("C", ptBr)}</strong></span></td>");
        sb.AppendLine("                        </tr>");
        sb.AppendLine("                    </table>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("        </table>");
        sb.AppendLine("        <br>");
    }

    private void ConstruirBotaoAcao(StringBuilder sb, string hash)
    {
        sb.AppendLine("        <table class=\"my-panel\">");
        sb.AppendLine("            <tr class=\"my-row\">");
        sb.AppendLine("                <td class=\"my-col-10\">");
        sb.AppendLine("                    <div class=\"my-text-center\">");
        sb.AppendLine($"                        <a href=\"http://wbsvc.brsupply.com.br/cotacao/?hash={Uri.EscapeDataString(hash)}\">");
        sb.AppendLine("                            <img src=\"http://wbsvc.brsupply.com.br/cotacao/img/btnAutorizarFaturamento.png\" alt=\"Autorizar Faturamento\">");
        sb.AppendLine("                        </a>");
        sb.AppendLine("                    </div>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("        </table>");
    }

    private string CssPrincipal()
    {
        return $@"
    <style type=""text/css"">
        .my-body {{
            background-color: #f2f2f2;
            font-family: ""Helvetica Neue"", Helvetica, Arial, sans-serif;
            font-size: 14px;
            padding: 10px;
        }}
        .my-container {{ margin: auto; }}
        .my-panel {{ background: white; width: 100%; padding: 10px; }}
        .my-row {{ width: 100%; }}
        .my-col-1 {{ width: 10%; }}
        .my-col-2 {{ width: 20%; }}
        .my-col-3 {{ width: 30%; }}
        .my-col-4 {{ width: 40%; }}
        .my-col-5 {{ width: 50%; }}
        .my-col-6 {{ width: 60%; }}
        .my-col-7 {{ width: 70%; }}
        .my-col-8 {{ width: 80%; }}
        .my-col-9 {{ width: 90%; }}
        .my-col-10 {{ width: 100%; }}
        .my-text-right {{ text-align: right; }}
        .my-text-left {{ text-align: left; }}
        .my-text-center {{ text-align: center; }}
        .my-marcador {{ font-weight: bold; font-size: 18px; color: {_estabCor}; }}
        .my-dados {{ background: white; width: 100%; }}
        .my-dados td {{ border: 1px solid #dddddd; padding: 5px; }}
        .my-titulo-campo {{ font-size: 10px; color: {_estabCor}; }}
        .my-h3 {{ font-size: 25px; margin-bottom: 2.5px !important; }}
        .my-h4 {{ font-size: 20px; font-weight: normal; margin-top: 2.5px !important; }}
        .my-cab {{ font-size: 15px; }}
        .my-cinza {{ color: silver; font-size: 12px; }}
        .my-valores {{ font-size: 20px; }}
    </style>";
    }

    private static string HtmlEncode(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "&nbsp;";
        return System.Web.HttpUtility.HtmlEncode(text);
    }

    private static string FormatCnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj) || cnpj.Length < 14)
            return "&nbsp;";
        return $"{cnpj[..2]}.{cnpj[2..5]}.{cnpj[5..8]}/{cnpj[8..12]}-{cnpj[12..]}";
    }
}

/// <summary>
/// Dados do email passados do CotacaoEmailService
/// </summary>
public sealed class DadosEmailInterno
{
    public string CdProposta { get; set; } = string.Empty;
    public string OrdemCompra { get; set; } = string.Empty;
    public string Obs { get; set; } = string.Empty;
    public string ContatoNome { get; set; } = string.Empty;
    public string ContatoEmail { get; set; } = string.Empty;
    public string DataValidade { get; set; } = string.Empty;
    public string CondPagtoNome { get; set; } = string.Empty;
    public string StatusNome { get; set; } = string.Empty;
    public int DiasPrazoEntrega { get; set; }
    public string TransportadoraNome { get; set; } = string.Empty;
    public decimal VlrFrete { get; set; }
    public decimal TotalVendaSemFrete { get; set; }
    public decimal TotalVendaFinal { get; set; }

    public string EstabRazaoSocial { get; set; } = string.Empty;
    public string EstabCNPJ { get; set; } = string.Empty;
    public string EstabInscrEstadual { get; set; } = string.Empty;
    public string EstabTelefone { get; set; } = string.Empty;
    public string EstabEndereco { get; set; } = string.Empty;
    public string EstabNumero { get; set; } = string.Empty;
    public string EstabComplemento { get; set; } = string.Empty;
    public string EstabBairro { get; set; } = string.Empty;
    public string EstabCidade { get; set; } = string.Empty;
    public string EstabUF { get; set; } = string.Empty;
    public string EstabCEP { get; set; } = string.Empty;

    public string ConsultorNome { get; set; } = string.Empty;
    public string ConsultorEmail { get; set; } = string.Empty;
    public string ConsultorTelefone { get; set; } = string.Empty;

    public string ClienteRazaoSocial { get; set; } = string.Empty;
    public string ClienteCNPJ { get; set; } = string.Empty;
    public string ClienteTelefone { get; set; } = string.Empty;
    public string ClienteEndereco { get; set; } = string.Empty;
    public string ClienteNumero { get; set; } = string.Empty;
    public string ClienteComplemento { get; set; } = string.Empty;
    public string ClienteBairro { get; set; } = string.Empty;
    public string ClienteCidade { get; set; } = string.Empty;
    public string ClienteUF { get; set; } = string.Empty;
    public string ClienteCEP { get; set; } = string.Empty;

    public IReadOnlyList<ItemEmailInterno> Itens { get; set; } = [];
}

/// <summary>
/// Item do email
/// </summary>
public sealed record ItemEmailInterno(
    string CodItemBR,
    string DescrItemBR,
    decimal PrecoItem,
    decimal IPI,
    decimal ST,
    decimal Quantidade,
    decimal VlrUnitario,
    string NmSegmento,
    string NCM);
