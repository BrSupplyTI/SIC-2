using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Data.SqlClient;
using MimeKit;
using SIC.Web.Models.Cotacao;
using System.Text;

namespace SIC.Web.Services.Cotacao;

/// <summary>
/// Serviço de envio de e-mail da Cotação e gravação do log.
/// Replica a lógica do PHP controllers/cotacoes/EnviarEmail.php.
/// </summary>
public sealed class CotacaoEmailService(
    IConfiguration configuration,
    IWebHostEnvironment env)
{
    private readonly string _smtpHost      = configuration["Smtp:Host"]      ?? string.Empty;
    private readonly int    _smtpPort      = configuration.GetValue<int?>("Smtp:Port") ?? 587;
    private readonly bool   _smtpSsl       = configuration.GetValue<bool?>("Smtp:EnableSsl") ?? true;
    private readonly string _smtpUser      = configuration["Smtp:Username"]  ?? string.Empty;
    private readonly string _smtpPass      = configuration["Smtp:Password"]  ?? string.Empty;
    private readonly string _smtpFrom      = configuration["Smtp:FromEmail"] ?? string.Empty;
    private readonly string _smtpFromName  = configuration["Smtp:FromName"]  ?? "SIC";
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    // ═══════════════════════════════════════════════════════════════════════
    //  PONTO DE ENTRADA PRINCIPAL
    // ═══════════════════════════════════════════════════════════════════════

    public async Task EnviarAsync(
        EnviarEmailCotacaoViewModel form,
        int usuarioLogadoId,
        CancellationToken cancellationToken = default)
    {
        // ── 1. Normalizar campos (igual ao PHP) ───────────────────────────
        var emailDestinatario = form.EmailDestinatario.Replace(" ", "");
        var comCopia          = string.IsNullOrWhiteSpace(form.ComCopia) ? null
                                    : form.ComCopia.Replace(" ", "");
        var mensagem          = (form.Mensagem ?? "").Replace("'", " ");

        // ── 2. Gerar hash único (equivalente a "BRS-{id}-{md5(uniqid)}") ──
        var hash = $"BRS-{form.CotacaoID}-{Guid.NewGuid():N}";

        // ── 3. Título e cor do estabelecimento ────────────────────────────
        const string estabTitulo = "Br Supply";
        const string estabCor    = "#F68620";
        const string estabLogo   = "https://supplymanager.com.br/logos/brsupply_350x70.png";

        var titulo = $"Cotação {estabTitulo}: {form.CotacaoID}";

        // ── 4. Carregar dados completos da proposta para o template ───────
        var cot = await GetDadosEmailAsync(form.PropostaId, cancellationToken)
                  ?? throw new InvalidOperationException($"Proposta #{form.PropostaId} não encontrada.");

        // ── 5. Carregar template HTML ─────────────────────────────────────
        var templatePath = Path.Combine(
            env.ContentRootPath,
            "Views", "Cotacao", "EmailInformarClienteCotacaoLiberadaModelo2.html");

        var corpo = await File.ReadAllTextAsync(templatePath, cancellationToken);

        // ── 6. Substituir placeholders ────────────────────────────────────
        corpo = AplicarPlaceholders(corpo, titulo, estabCor, estabLogo,
                                    form, cot, mensagem, hash);

        // ── 7. Montar MimeMessage com CC / BCC / Reply-To ─────────────────
        var bcc     = new List<string>();
        string? replyTo = null;

        if (form.ConsultorRecebeCCO && !string.IsNullOrWhiteSpace(form.ConsultorEmail))
        {
            bcc.Add(form.ConsultorEmail);
            replyTo = form.ConsultorEmail;
        }

        if (form.ExecutivoRecebeCCO && !string.IsNullOrWhiteSpace(form.ExecutivoEmail))
        {
            bcc.Add(form.ExecutivoEmail);
            replyTo ??= form.ExecutivoEmail;
        }

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_smtpFromName, _smtpFrom));
        mimeMessage.To.Add(MailboxAddress.Parse(emailDestinatario));
        mimeMessage.Subject = titulo;

        if (!string.IsNullOrWhiteSpace(comCopia))
            mimeMessage.Cc.Add(MailboxAddress.Parse(comCopia));

        foreach (var bccAddr in bcc)
            mimeMessage.Bcc.Add(MailboxAddress.Parse(bccAddr));

        if (!string.IsNullOrWhiteSpace(replyTo))
            mimeMessage.ReplyTo.Add(MailboxAddress.Parse(replyTo));

        mimeMessage.Body = new TextPart("html") { Text = corpo };

        // ── 8. Enviar via MailKit (STARTTLS — igual ao PHPMailer) ─────────
        System.Diagnostics.Debug.WriteLine($"[SMTP] Conectando: {_smtpHost}:{_smtpPort} user={_smtpUser} to={emailDestinatario}");
        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await smtp.AuthenticateAsync(_smtpUser, _smtpPass, cancellationToken);
            await smtp.SendAsync(mimeMessage, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
            System.Diagnostics.Debug.WriteLine("[SMTP] Enviado com sucesso!");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SMTP ERRO] {ex.GetType().Name}: {ex.Message}");
            throw;
        }

        // ── 9. Gravar log no banco (saveLogEnvio do PHP) ──────────────────
        await SalvarLogEnvioAsync(new LogEnvioParams(
            PropostaId:            form.PropostaId,
            Nome:                  form.ContatoNome,
            Email:                 emailDestinatario,
            Saudacao:              form.Saudacao,
            Mensagem:              mensagem,
            ComCopia:              comCopia,
            Hash:                  hash,
            UsuarioID:             usuarioLogadoId,
            PodeDispEstoque:       form.PodeDispEstoque       ? 1 : 0,
            PodeAltTransportadora: form.PodeAltTransportadora ? 1 : 0,
            PodeAltCondPagamento:  form.PodeAltCondPagamento  ? 1 : 0,
            PodeNegociar:          form.PodeNegociar          ? 1 : 0
        ), cancellationToken);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  SUBSTITUIÇÃO DE PLACEHOLDERS
    // ═══════════════════════════════════════════════════════════════════════

    private static string AplicarPlaceholders(
        string corpo,
        string titulo,
        string estabCor,
        string estabLogo,
        EnviarEmailCotacaoViewModel form,
        DadosEmail cot,
        string mensagem,
        string hash)
    {
        // Cabeçalho
        corpo = corpo
            .Replace("{{EMAIL_TITULO}}",   titulo)
            .Replace("{{EMAIL_EST_LOGO}}", estabLogo)
            .Replace("{{EMAIL_EST_ICON}}", estabLogo)
            .Replace("{{EMAIL_EST_COR}}",  estabCor);

        // Cotação
        corpo = corpo
            .Replace("{{EMAIL_COT_ID}}",     cot.CdProposta)
            .Replace("{{EMAIL_COT_ANO}}",    DateTime.Now.Year.ToString())
            .Replace("{{EMAIL_COT_STATUS}}", cot.StatusNome)
            .Replace("{{EMAIL_COT_DATA}}",   DateTime.Now.ToString("dd/MM/yyyy"))
            .Replace("{{EMAIL_COT_DATAVAL}}", cot.DataValidade)
            .Replace("{{EMAIL_COT_CONDPAG}}", cot.CondPagtoNome)
            .Replace("{{EMAIL_COT_OC}}",     string.IsNullOrEmpty(cot.OrdemCompra)  ? "&nbsp;" : cot.OrdemCompra)
            .Replace("{{EMAIL_COT_OBS}}",    string.IsNullOrEmpty(cot.Obs)          ? "&nbsp;" : cot.Obs)
            .Replace("{{EMAIL_CONT_NOME}}",  string.IsNullOrEmpty(cot.ContatoNome)  ? "&nbsp;" : cot.ContatoNome)
            .Replace("{{EMAIL_CONT_EMAIL}}", string.IsNullOrEmpty(cot.ContatoEmail) ? "&nbsp;" : cot.ContatoEmail);

        // Saudação e mensagem
        corpo = corpo
            .Replace("{{EMAIL_SAUDACAO}}", form.Saudacao)
            .Replace("{{EMAIL_MENSAGEM}}", mensagem);

        // Estabelecimento
        corpo = corpo
            .Replace("{{EMAIL_EST_RAZAO}}",    cot.EstabRazaoSocial)
            .Replace("{{EMAIL_EST_CNPJ}}",     FormatCnpj(cot.EstabCNPJ))
            .Replace("{{EMAIL_EST_INSCREST}}", cot.EstabInscrEstadual)
            .Replace("{{EMAIL_EST_TELEFONE}}", cot.EstabTelefone)
            .Replace("{{EMAIL_EST_ENDERECO}}", cot.EstabEndereco)
            .Replace("{{EMAIL_EST_NUMERO}}",   cot.EstabNumero)
            .Replace("{{EMAIL_EST_COMP}}",     cot.EstabComplemento)
            .Replace("{{EMAIL_EST_BAIRRO}}",   cot.EstabBairro)
            .Replace("{{EMAIL_EST_CIDADE}}",   cot.EstabCidade)
            .Replace("{{EMAIL_EST_UF}}",       cot.EstabUF)
            .Replace("{{EMAIL_EST_CEP}}",      cot.EstabCEP);

        // Consultor
        corpo = corpo
            .Replace("{{EMAIL_CONSULTOR_NOME}}",     cot.ConsultorNome)
            .Replace("{{EMAIL_CONSULTOR_EMAIL}}",    cot.ConsultorEmail)
            .Replace("{{EMAIL_CONSULTOR_TELEFONE}}", cot.ConsultorTelefone);

        // Cliente
        corpo = corpo
            .Replace("{{EMAIL_CLI_RAZAO}}",    cot.ClienteRazaoSocial)
            .Replace("{{EMAIL_CLI_CNPJ}}",     FormatCnpj(cot.ClienteCNPJ))
            .Replace("{{EMAIL_CLI_TELEFONE}}", cot.ClienteTelefone)
            .Replace("{{EMAIL_CLI_ENDERECO}}", cot.ClienteEndereco)
            .Replace("{{EMAIL_CLI_NUMERO}}",   cot.ClienteNumero)
            .Replace("{{EMAIL_CLI_COMP}}",     cot.ClienteComplemento)
            .Replace("{{EMAIL_CLI_BAIRRO}}",   cot.ClienteBairro)
            .Replace("{{EMAIL_CLI_CIDADE}}",   cot.ClienteCidade)
            .Replace("{{EMAIL_CLI_UF}}",       cot.ClienteUF)
            .Replace("{{EMAIL_CLI_CEP}}",      cot.ClienteCEP);

        // Itens
        corpo = corpo.Replace("{{EMAIL_ITENS}}", MontarItens(cot.Itens));

        // Prazo, frete e totais
        var prazoEntrega = cot.DiasPrazoEntrega == 0
            ? "<span class='my-cinza'>A definir</span>"
            : $"{cot.DiasPrazoEntrega} Dias Úteis";

        var transportadora = string.IsNullOrWhiteSpace(cot.TransportadoraNome)
            ? "<span class='my-cinza'>Definida no momento do faturamento.</span>"
            : cot.TransportadoraNome;

        var tipoFrete = cot.VlrFrete > 0 ? "FOB" : "CIF";

        corpo = corpo
            .Replace("{{EMAIL_COT_PRAZOENT}}",      prazoEntrega)
            .Replace("{{EMAIL_COT_TRANSP}}",        transportadora)
            .Replace("{{EMAIL_COT_TIPOFRETE}}",     tipoFrete)
            .Replace("{{EMAIL_COT_VLRFRETE}}",      cot.VlrFrete.ToString("C", new System.Globalization.CultureInfo("pt-BR")))
            .Replace("{{EMAIL_COT_VLRTOTAL}}",      cot.TotalVendaSemFrete.ToString("C", new System.Globalization.CultureInfo("pt-BR")))
            .Replace("{{EMAIL_COT_VLRTOTALFRETE}}", cot.TotalVendaFinal.ToString("C", new System.Globalization.CultureInfo("pt-BR")))
            .Replace("{{EMAIL_HASH}}",              hash);

        return corpo;
    }

    private static string MontarItens(IReadOnlyList<ItemEmail> itens)
    {
        var sb = new StringBuilder();
        foreach (var item in itens)
        {
            var foto = $"http://www.supplymanager.com.br/fotos/low/{item.CodItemBR}.jpg";
            sb.AppendLine($"""
                <tr>
                  <td class='my-text-center'><img src='{foto}' alt='{item.DescrItemBR}' style='width:50px;'></td>
                  <td class='my-text-center'>{item.CodItemBR}</td>
                  <td class='my-text-left'>
                    {item.DescrItemBR}<br>
                    <span class='my-cinza'>{item.NmSegmento}</span>
                  </td>
                  <td class='my-text-center'>{item.NCM}</td>
                  <td class='my-text-right'>{item.PrecoItem.ToString("C", new System.Globalization.CultureInfo("pt-BR"))}</td>
                  <td class='my-text-right'>{item.IPI.ToString("C", new System.Globalization.CultureInfo("pt-BR"))}</td>
                  <td class='my-text-right'>{item.ST.ToString("C", new System.Globalization.CultureInfo("pt-BR"))}</td>
                  <td class='my-text-center'><strong>{item.Quantidade}</strong></td>
                  <td class='my-text-right'><strong>{item.VlrUnitario.ToString("C", new System.Globalization.CultureInfo("pt-BR"))}</strong></td>
                </tr>
                """);
        }
        return sb.ToString();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  CONSULTA DE DADOS PARA O TEMPLATE (tudo que o PHP buscava via $cot)
    // ═══════════════════════════════════════════════════════════════════════

    private async Task<DadosEmail?> GetDadosEmailAsync(
        int propostaId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                -- Cotação
                Proposta.CdProposta                                         AS CdProposta,
                ISNULL(Proposta.OrdemCompra, '')                            AS OrdemCompra,
                ISNULL(Proposta.Obs, '')                                    AS Obs,
                ISNULL(Proposta.ContatoNome, '')                            AS ContatoNome,
                ISNULL(Proposta.ContatoEmail, '')                           AS ContatoEmail,
                CONVERT(VARCHAR(10), Proposta.DataValidade, 103)            AS DataValidade,
                ISNULL(CP.NmCondPagto, '')                                  AS CondPagtoNome,
                PropostaStatus.NmStatus                                     AS StatusNome,
                ISNULL(Proposta.DiasPrazoEntrega, 0)                        AS DiasPrazoEntrega,
                ISNULL(Transp.NmTransportadora, '')                         AS TransportadoraNome,
                ISNULL(Proposta.Frete, 0)                                   AS VlrFrete,
                (SELECT ISNULL(SUM(PI.VlrPrecoVenda),0)
                 FROM BrWeb..Proposta_Itens PI WITH (NOLOCK)
                 WHERE PI.PropostaID = Proposta.PropostaId)                 AS TotalVendaSemFrete,
                (SELECT ISNULL(SUM(PI.VlrPrecoVenda),0) + ISNULL(Proposta.Frete,0)
                 FROM BrWeb..Proposta_Itens PI WITH (NOLOCK)
                 WHERE PI.PropostaID = Proposta.PropostaId)                 AS TotalVendaFinal,

                -- Estabelecimento
                ISNULL(Est.EstabelRazaoSocial, '')                          AS EstabRazaoSocial,
                ISNULL(Est.EstabelCNPJ, '')                                 AS EstabCNPJ,
                ISNULL(Est.InscrEstadual, '')                               AS EstabInscrEstadual,
                ISNULL(Est.EstabelTelefone, '')                             AS EstabTelefone,
                ISNULL(PARSENAME(REPLACE(Est.EstabelEndereco,',','.'),3),'') AS EstabEndereco,
                ISNULL(PARSENAME(REPLACE(Est.EstabelEndereco,',','.'),2),'') AS EstabNumero,
                ISNULL(PARSENAME(REPLACE(Est.EstabelEndereco,',','.'),1),'') AS EstabComplemento,
                ISNULL(Est.EstabelBairro, '')                               AS EstabBairro,
                ISNULL(CidEst.NmCidade, '')                                 AS EstabCidade,
                ISNULL(UFEst.CdUF, '')                                      AS EstabUF,
                ISNULL(Est.EstabelCEP, '')                                         AS EstabCEP,

                -- Consultor
                ISNULL(Consultor.NmUsuario, '')                             AS ConsultorNome,
                ISNULL(Consultor.Email, '')                                 AS ConsultorEmail,
                ISNULL(Consultor.Telefone, '')                              AS ConsultorTelefone,

                -- Cliente
                ISNULL(Cli.NmCliente, '')                                   AS ClienteRazaoSocial,
                ISNULL(Cli.CNPJCliente, '')                                 AS ClienteCNPJ,
                ISNULL(Cli.TelefoneCliente, '')                                    AS ClienteTelefone,

                -- Endereço de entrega (local diferente tem prioridade, igual ao PHP)
                ISNULL(CLE.FlagEnderecoDiferente, 0)                        AS FlagEnderecoDiferente,

                -- Local de entrega
                ISNULL(CLE.DsLogradouro, '')                                AS LocLogradouro,
                ISNULL(CLE.DsNumero, '')                                    AS LocNumero,
                ISNULL(CLE.DsComplemento, '')                               AS LocComplemento,
                ISNULL(CLE.DsBairro, '')                                    AS LocBairro,
                ISNULL(CidLoc.NmCidade, '')                                 AS LocCidade,
                ISNULL(UFLoc.CdUF, '')                                      AS LocUF,
                ISNULL(CLE.DsCEP, '')                                       AS LocCEP,

                -- Endereço principal do cliente
                ISNULL(CE.Logradouro, '')                                   AS EndLogradouro,
                ISNULL(CE.Numero, '')                                       AS EndNumero,
                ISNULL(CE.Complemento, '')                                  AS EndComplemento,
                ISNULL(CE.Bairro, '')                                       AS EndBairro,
                ISNULL(CE.Cidade, '')                                       AS EndCidade,
                ISNULL(UFEnd.CdUF, '')                                      AS EndUF,
                ISNULL(CE.CEP, '')                                          AS EndCEP

            FROM BrWeb.dbo.Proposta Proposta (NOLOCK)
            LEFT JOIN BrWeb.dbo.Proposta_Status PropostaStatus (NOLOCK)
                ON PropostaStatus.StatusID = Proposta.StatusID
            LEFT JOIN BrSupply.dbo.BR_CondPagto CP (NOLOCK)
                ON CP.CondPagtoID = Proposta.CondPagto
            LEFT JOIN BrSupply.dbo.BR_Transportadora Transp (NOLOCK)
                ON Transp.TransportadoraID = Proposta.TransportadoraID
            LEFT JOIN BrSupply.dbo.BR_Estabelecimento Est (NOLOCK)
                ON Est.EstabelecimentoID = Proposta.EstabelecimentoID
            LEFT JOIN BrSupply.dbo.BR_Cidade CidEst (NOLOCK)
                ON CidEst.CidadeID = Est.EstabelCidadeID
            LEFT JOIN BrSupply.dbo.BR_UF UFEst (NOLOCK)
                ON UFEst.UFID = Est.UFID
            LEFT JOIN BrSupply.dbo.BR_Usuario Consultor (NOLOCK)
                ON Consultor.UsuarioID = Proposta.UsuarioID
            LEFT JOIN BrSupply.dbo.BR_Cliente Cli (NOLOCK)
                ON Cli.ClienteID = Proposta.ClienteId
            LEFT JOIN BrSupply.dbo.BR_ClienteLocalEntrega CLE (NOLOCK)
                ON CLE.ClienteLocalEntregaID = Proposta.ClienteLocalEntregaID
            LEFT JOIN BrSupply.dbo.BR_Cidade CidLoc (NOLOCK)
                ON CidLoc.CidadeID = CLE.CdCidadeID
            LEFT JOIN BrSupply.dbo.BR_UF UFLoc (NOLOCK)
                ON UFLoc.UFID = CLE.CdUFID
            LEFT JOIN BrSupply.dbo.BR_ClienteEndereco CE (NOLOCK)
                ON CE.ClienteEnderecoID = Proposta.ClienteEnderecoID
            LEFT JOIN BrSupply.dbo.BR_UF UFEnd (NOLOCK)
                ON UFEnd.UFID = CE.UFID
            WHERE Proposta.PropostaId = @PropostaID
            """;

        const string itensSql = """
            SELECT
                PI.CodItemBR,
                PI.DescrItemBR,
                ISNULL(PI.PrecoItem, 0)      AS PrecoItem,
                ISNULL(PI.IPI, 0)            AS IPI,
                ISNULL(PI.ST, 0)             AS ST,
                ISNULL(PI.Quantidade, 0)     AS Quantidade,
                ISNULL(PI.VlrPrecoVenda / NULLIF(PI.Quantidade, 0), 0) AS VlrUnitario,
                ISNULL(Seg.NmSegmento, '')   AS NmSegmento,
                ISNULL(CF.CdClassificacaoFiscal, '') AS NCM
            FROM BrWeb.dbo.Proposta_Itens PI (NOLOCK)
            LEFT JOIN BrSupply.dbo.BR_Item Item (NOLOCK)
                ON Item.CdItem = PI.CodItemBR
            LEFT JOIN BrSupply.dbo.BR_Segmento Seg (NOLOCK)
                ON Seg.SegmentoID = Item.SegmentoID
            LEFT JOIN BrSupply.dbo.BR_ClassificacaoFiscal CF (NOLOCK)
                ON CF.ClassificacaoFiscalID = Item.ClassificacaoFiscalID
            WHERE PI.PropostaID = @PropostaID
            ORDER BY PI.PropostaItemID
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Cabeçalho
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        static string S(SqlDataReader r, string col)
        {
            var ord = r.GetOrdinal(col);
            return r.IsDBNull(ord) ? string.Empty : r.GetString(ord);
        }
        static int I(SqlDataReader r, string col)
        {
            var ord = r.GetOrdinal(col);
            return r.IsDBNull(ord) ? 0 : Convert.ToInt32(r.GetValue(ord));
        }
        static decimal D(SqlDataReader r, string col)
        {
            var ord = r.GetOrdinal(col);
            return r.IsDBNull(ord) ? 0m : Convert.ToDecimal(r.GetValue(ord));
        }

        var flagEndDiferente = I(reader, "FlagEnderecoDiferente");

        var dados = new DadosEmail
        {
            CdProposta         = S(reader, "CdProposta"),
            OrdemCompra        = S(reader, "OrdemCompra"),
            Obs                = S(reader, "Obs"),
            ContatoNome        = S(reader, "ContatoNome"),
            ContatoEmail       = S(reader, "ContatoEmail"),
            DataValidade       = S(reader, "DataValidade"),
            CondPagtoNome      = S(reader, "CondPagtoNome"),
            StatusNome         = S(reader, "StatusNome"),
            DiasPrazoEntrega   = I(reader, "DiasPrazoEntrega"),
            TransportadoraNome = S(reader, "TransportadoraNome"),
            VlrFrete           = D(reader, "VlrFrete"),
            TotalVendaSemFrete = D(reader, "TotalVendaSemFrete"),
            TotalVendaFinal    = D(reader, "TotalVendaFinal"),

            EstabRazaoSocial   = S(reader, "EstabRazaoSocial"),
            EstabCNPJ          = S(reader, "EstabCNPJ"),
            EstabInscrEstadual = S(reader, "EstabInscrEstadual"),
            EstabTelefone      = S(reader, "EstabTelefone"),
            EstabEndereco      = S(reader, "EstabEndereco"),
            EstabNumero        = S(reader, "EstabNumero"),
            EstabComplemento   = S(reader, "EstabComplemento"),
            EstabBairro        = S(reader, "EstabBairro"),
            EstabCidade        = S(reader, "EstabCidade"),
            EstabUF            = S(reader, "EstabUF"),
            EstabCEP           = S(reader, "EstabCEP"),

            ConsultorNome      = S(reader, "ConsultorNome"),
            ConsultorEmail     = S(reader, "ConsultorEmail"),
            ConsultorTelefone  = S(reader, "ConsultorTelefone"),

            ClienteRazaoSocial = S(reader, "ClienteRazaoSocial"),
            ClienteCNPJ        = S(reader, "ClienteCNPJ"),
            ClienteTelefone    = S(reader, "ClienteTelefone"),

            // Endereço: local diferente tem prioridade (igual ao PHP)
            ClienteEndereco    = flagEndDiferente > 0 ? S(reader, "LocLogradouro")  : S(reader, "EndLogradouro"),
            ClienteNumero      = flagEndDiferente > 0 ? S(reader, "LocNumero")      : S(reader, "EndNumero"),
            ClienteComplemento = flagEndDiferente > 0 ? S(reader, "LocComplemento") : S(reader, "EndComplemento"),
            ClienteBairro      = flagEndDiferente > 0 ? S(reader, "LocBairro")      : S(reader, "EndBairro"),
            ClienteCidade      = flagEndDiferente > 0 ? S(reader, "LocCidade")      : S(reader, "EndCidade"),
            ClienteUF          = flagEndDiferente > 0 ? S(reader, "LocUF")          : S(reader, "EndUF"),
            ClienteCEP         = flagEndDiferente > 0 ? S(reader, "LocCEP")         : S(reader, "EndCEP"),
        };

        await reader.CloseAsync();

        // Itens
        await using var cmdItens = new SqlCommand(itensSql, connection);
        cmdItens.Parameters.AddWithValue("@PropostaID", propostaId);

        var itens = new List<ItemEmail>();
        await using var readerItens = await cmdItens.ExecuteReaderAsync(cancellationToken);
        while (await readerItens.ReadAsync(cancellationToken))
        {
            itens.Add(new ItemEmail(
                CodItemBR:   S(readerItens, "CodItemBR"),
                DescrItemBR: S(readerItens, "DescrItemBR"),
                PrecoItem:   D(readerItens, "PrecoItem"),
                IPI:         D(readerItens, "IPI"),
                ST:          D(readerItens, "ST"),
                Quantidade:  D(readerItens, "Quantidade"),
                VlrUnitario: D(readerItens, "VlrUnitario"),
                NmSegmento:  S(readerItens, "NmSegmento"),
                NCM:         S(readerItens, "NCM")
            ));
        }

        dados.Itens = itens;
        return dados;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  LOG DE ENVIO (saveLogEnvio do PHP → BRWeb..Proposta_CotacaoEnvio)
    // ═══════════════════════════════════════════════════════════════════════

    private async Task SalvarLogEnvioAsync(
        LogEnvioParams p,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO BRWeb..Proposta_CotacaoEnvio
                (PropostaID, Nome, Email, Saudacao, Mensagem, ComCopia, Hash,
                 UsuarioID,
                 FlagVisualizaEstoque, FlagPodeTrocarTransportadora,
                 FlagPodeTrocarCondPagto, FlagPodeNegociar,
                 DataHora, FlagAtivo)
            VALUES
                (@PropostaID, @Nome, @Email, @Saudacao, @Mensagem, @ComCopia, @Hash,
                 @UsuarioID,
                 @FlagVisualizaEstoque, @FlagPodeTrocarTransportadora,
                 @FlagPodeTrocarCondPagto, @FlagPodeNegociar,
                 GETDATE(), 1)
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@PropostaID",               p.PropostaId);
        cmd.Parameters.AddWithValue("@Nome",                     p.Nome);
        cmd.Parameters.AddWithValue("@Email",                    p.Email);
        cmd.Parameters.AddWithValue("@Saudacao",                 p.Saudacao);
        cmd.Parameters.AddWithValue("@Mensagem",                 (object?)p.Mensagem     ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ComCopia",                 (object?)p.ComCopia     ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Hash",                     p.Hash);
        cmd.Parameters.AddWithValue("@UsuarioID",                p.UsuarioID);
        cmd.Parameters.AddWithValue("@FlagVisualizaEstoque",     p.PodeDispEstoque);
        cmd.Parameters.AddWithValue("@FlagPodeTrocarTransportadora", p.PodeAltTransportadora);
        cmd.Parameters.AddWithValue("@FlagPodeTrocarCondPagto",  p.PodeAltCondPagamento);
        cmd.Parameters.AddWithValue("@FlagPodeNegociar",         p.PodeNegociar);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private static string FormatCnpj(string cnpj)
    {
        cnpj = new string(cnpj.Where(char.IsDigit).ToArray());
        return cnpj.Length == 14
            ? $"{cnpj[..2]}.{cnpj[2..5]}.{cnpj[5..8]}/{cnpj[8..12]}-{cnpj[12..14]}"
            : cnpj;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  TIPOS INTERNOS
    // ═══════════════════════════════════════════════════════════════════════

    private sealed class DadosEmail
    {
        public string CdProposta         { get; set; } = string.Empty;
        public string OrdemCompra        { get; set; } = string.Empty;
        public string Obs                { get; set; } = string.Empty;
        public string ContatoNome        { get; set; } = string.Empty;
        public string ContatoEmail       { get; set; } = string.Empty;
        public string DataValidade       { get; set; } = string.Empty;
        public string CondPagtoNome      { get; set; } = string.Empty;
        public string StatusNome         { get; set; } = string.Empty;
        public int    DiasPrazoEntrega   { get; set; }
        public string TransportadoraNome { get; set; } = string.Empty;
        public decimal VlrFrete          { get; set; }
        public decimal TotalVendaSemFrete { get; set; }
        public decimal TotalVendaFinal   { get; set; }

        public string EstabRazaoSocial   { get; set; } = string.Empty;
        public string EstabCNPJ          { get; set; } = string.Empty;
        public string EstabInscrEstadual { get; set; } = string.Empty;
        public string EstabTelefone      { get; set; } = string.Empty;
        public string EstabEndereco      { get; set; } = string.Empty;
        public string EstabNumero        { get; set; } = string.Empty;
        public string EstabComplemento   { get; set; } = string.Empty;
        public string EstabBairro        { get; set; } = string.Empty;
        public string EstabCidade        { get; set; } = string.Empty;
        public string EstabUF            { get; set; } = string.Empty;
        public string EstabCEP           { get; set; } = string.Empty;

        public string ConsultorNome      { get; set; } = string.Empty;
        public string ConsultorEmail     { get; set; } = string.Empty;
        public string ConsultorTelefone  { get; set; } = string.Empty;

        public string ClienteRazaoSocial { get; set; } = string.Empty;
        public string ClienteCNPJ        { get; set; } = string.Empty;
        public string ClienteTelefone    { get; set; } = string.Empty;
        public string ClienteEndereco    { get; set; } = string.Empty;
        public string ClienteNumero      { get; set; } = string.Empty;
        public string ClienteComplemento { get; set; } = string.Empty;
        public string ClienteBairro      { get; set; } = string.Empty;
        public string ClienteCidade      { get; set; } = string.Empty;
        public string ClienteUF          { get; set; } = string.Empty;
        public string ClienteCEP         { get; set; } = string.Empty;

        public IReadOnlyList<ItemEmail> Itens { get; set; } = [];
    }

    private sealed record ItemEmail(
        string CodItemBR,
        string DescrItemBR,
        decimal PrecoItem,
        decimal IPI,
        decimal ST,
        decimal Quantidade,
        decimal VlrUnitario,
        string NmSegmento,
        string NCM);

    private sealed record LogEnvioParams(
        int     PropostaId,
        string  Nome,
        string  Email,
        string  Saudacao,
        string? Mensagem,
        string? ComCopia,
        string  Hash,
        int     UsuarioID,
        int     PodeDispEstoque,
        int     PodeAltTransportadora,
        int     PodeAltCondPagamento,
        int     PodeNegociar);
}
