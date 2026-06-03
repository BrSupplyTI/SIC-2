using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SIC.Web.Models.Cotacao;
using System.Text;

namespace SIC.Web.Services.Cotacao;

/// <summary>
/// Serviço de envio de e-mail da Cotação e gravação do log.
/// Replica a lógica do PHP controllers/cotacoes/EnviarEmail.php.
/// Os dados são obtidos via CotacaoApiClient (sem SQL direto no Web).
/// </summary>
public sealed class CotacaoEmailService(
    IConfiguration configuration,
    IWebHostEnvironment env,
    CotacaoApiClient apiClient)
{
    private readonly string _smtpHost      = configuration["Smtp:Host"]      ?? string.Empty;
    private readonly int    _smtpPort      = configuration.GetValue<int?>("Smtp:Port") ?? 587;
    private readonly bool   _smtpSsl       = configuration.GetValue<bool?>("Smtp:EnableSsl") ?? true;
    private readonly string _smtpUser      = configuration["Smtp:Username"]  ?? string.Empty;
    private readonly string _smtpPass      = configuration["Smtp:Password"]  ?? string.Empty;
    private readonly string _smtpFrom      = configuration["Smtp:FromEmail"] ?? string.Empty;
    private readonly string _smtpFromName  = configuration["Smtp:FromName"]  ?? "SIC";

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
        var tpl = await apiClient.GetEmailTemplateAsync(form.PropostaId, cancellationToken)
                  ?? throw new InvalidOperationException($"Proposta #{form.PropostaId} não encontrada.");

        var cot = MapToDadosEmail(tpl);

        // ── 5. Carregar template HTML ─────────────────────────────────────
        var templatePath = Path.Combine(
            env.ContentRootPath,
            "Views", "Cotacao", "EmailInformarClienteCotacaoLiberadaModelo2.html");

        System.Diagnostics.Debug.WriteLine($"[EMAIL] ContentRootPath: {env.ContentRootPath}");
        System.Diagnostics.Debug.WriteLine($"[EMAIL] WebRootPath: {env.WebRootPath}");
        System.Diagnostics.Debug.WriteLine($"[EMAIL] Template Path: {templatePath}");
        System.Diagnostics.Debug.WriteLine($"[EMAIL] File Exists: {File.Exists(templatePath)}");

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

        // ── 9. Gravar log no banco via API ────────────────────────────────
        await apiClient.SalvarLogEnvioAsync(new SalvarLogEnvioRequest
        {
            PropostaId            = form.PropostaId,
            Nome                  = form.ContatoNome,
            Email                 = emailDestinatario,
            Saudacao              = form.Saudacao,
            Mensagem              = mensagem,
            ComCopia              = comCopia,
            Hash                  = hash,
            UsuarioId             = usuarioLogadoId,
            PodeDispEstoque       = form.PodeDispEstoque       ? 1 : 0,
            PodeAltTransportadora = form.PodeAltTransportadora ? 1 : 0,
            PodeAltCondPagamento  = form.PodeAltCondPagamento  ? 1 : 0,
            PodeNegociar          = form.PodeNegociar          ? 1 : 0,
        }, cancellationToken);
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
    //  MAPPER: CotacaoEmailTemplateViewModel → DadosEmail
    // ═══════════════════════════════════════════════════════════════════════

    private static DadosEmail MapToDadosEmail(CotacaoEmailTemplateViewModel tpl) => new()
    {
        CdProposta         = tpl.CdProposta,
        OrdemCompra        = tpl.OrdemCompra,
        Obs                = tpl.Obs,
        ContatoNome        = tpl.ContatoNome,
        ContatoEmail       = tpl.ContatoEmail,
        DataValidade       = tpl.DataValidade,
        CondPagtoNome      = tpl.CondPagtoNome,
        StatusNome         = tpl.StatusNome,
        DiasPrazoEntrega   = tpl.DiasPrazoEntrega,
        TransportadoraNome = tpl.TransportadoraNome,
        VlrFrete           = tpl.VlrFrete,
        TotalVendaSemFrete = tpl.TotalVendaSemFrete,
        TotalVendaFinal    = tpl.TotalVendaFinal,
        EstabRazaoSocial   = tpl.EstabRazaoSocial,
        EstabCNPJ          = tpl.EstabCNPJ,
        EstabInscrEstadual = tpl.EstabInscrEstadual,
        EstabTelefone      = tpl.EstabTelefone,
        EstabEndereco      = tpl.EstabEndereco,
        EstabNumero        = tpl.EstabNumero,
        EstabComplemento   = tpl.EstabComplemento,
        EstabBairro        = tpl.EstabBairro,
        EstabCidade        = tpl.EstabCidade,
        EstabUF            = tpl.EstabUF,
        EstabCEP           = tpl.EstabCEP,
        ConsultorNome      = tpl.ConsultorNome,
        ConsultorEmail     = tpl.ConsultorEmail,
        ConsultorTelefone  = tpl.ConsultorTelefone,
        ClienteRazaoSocial = tpl.ClienteRazaoSocial,
        ClienteCNPJ        = tpl.ClienteCNPJ,
        ClienteTelefone    = tpl.ClienteTelefone,
        ClienteEndereco    = tpl.ClienteEndereco,
        ClienteNumero      = tpl.ClienteNumero,
        ClienteComplemento = tpl.ClienteComplemento,
        ClienteBairro      = tpl.ClienteBairro,
        ClienteCidade      = tpl.ClienteCidade,
        ClienteUF          = tpl.ClienteUF,
        ClienteCEP         = tpl.ClienteCEP,
        Itens              = tpl.Itens.Select(i => new ItemEmail(
            i.CodItemBR, i.DescrItemBR, i.PrecoItem, i.IPI, i.ST,
            i.Quantidade, i.VlrUnitario, i.NmSegmento, i.NCM)).ToList(),
    };

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
}
