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
/// Configurações de SMTP são obtidas da API.
/// </summary>
public sealed class CotacaoEmailService(
    IWebHostEnvironment env,
    CotacaoApiClient apiClient)
{
    private string _smtpHost = string.Empty;
    private int _smtpPort = 587;
    private bool _smtpSsl = true;
    private string _smtpUser = string.Empty;
    private string _smtpPass = string.Empty;
    private string _smtpFrom = string.Empty;
    private string _smtpFromName = "SIC";

    // ═══════════════════════════════════════════════════════════════════════
    //  PONTO DE ENTRADA PRINCIPAL
    // ═══════════════════════════════════════════════════════════════════════

    public async Task EnviarAsync(
        EnviarEmailCotacaoViewModel form,
        int usuarioLogadoId,
        CancellationToken cancellationToken = default)
    {
        // ── 0. Obter configurações SMTP da API ─────────────────────────────
        var smtpConfig = await apiClient.GetSmtpConfigAsync(cancellationToken)
                         ?? throw new InvalidOperationException("Configurações de SMTP não encontradas na API.");

        _smtpHost = smtpConfig.Host;
        _smtpPort = smtpConfig.Port;
        _smtpSsl = smtpConfig.EnableSsl;
        _smtpUser = smtpConfig.Username;
        _smtpPass = smtpConfig.Password;
        _smtpFrom = smtpConfig.FromEmail;
        _smtpFromName = smtpConfig.FromName;

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

        // ── 5. Gerar HTML do email em C# (sem arquivo em disco) ───────────
        var builder = new CotacaoEmailHtmlBuilder(estabCor);

        var dadosEmail = new DadosEmailInterno
        {
            CdProposta = cot.CdProposta,
            OrdemCompra = cot.OrdemCompra,
            Obs = cot.Obs,
            ContatoNome = cot.ContatoNome,
            ContatoEmail = cot.ContatoEmail,
            DataValidade = cot.DataValidade,
            CondPagtoNome = cot.CondPagtoNome,
            StatusNome = cot.StatusNome,
            DiasPrazoEntrega = cot.DiasPrazoEntrega,
            TransportadoraNome = cot.TransportadoraNome,
            VlrFrete = cot.VlrFrete,
            TotalVendaSemFrete = cot.TotalVendaSemFrete,
            TotalVendaFinal = cot.TotalVendaFinal,
            EstabRazaoSocial = cot.EstabRazaoSocial,
            EstabCNPJ = cot.EstabCNPJ,
            EstabInscrEstadual = cot.EstabInscrEstadual,
            EstabTelefone = cot.EstabTelefone,
            EstabEndereco = cot.EstabEndereco,
            EstabNumero = cot.EstabNumero,
            EstabComplemento = cot.EstabComplemento,
            EstabBairro = cot.EstabBairro,
            EstabCidade = cot.EstabCidade,
            EstabUF = cot.EstabUF,
            EstabCEP = cot.EstabCEP,
            ConsultorNome = cot.ConsultorNome,
            ConsultorEmail = cot.ConsultorEmail,
            ConsultorTelefone = cot.ConsultorTelefone,
            ClienteRazaoSocial = cot.ClienteRazaoSocial,
            ClienteCNPJ = cot.ClienteCNPJ,
            ClienteTelefone = cot.ClienteTelefone,
            ClienteEndereco = cot.ClienteEndereco,
            ClienteNumero = cot.ClienteNumero,
            ClienteComplemento = cot.ClienteComplemento,
            ClienteBairro = cot.ClienteBairro,
            ClienteCidade = cot.ClienteCidade,
            ClienteUF = cot.ClienteUF,
            ClienteCEP = cot.ClienteCEP,
            Itens = cot.Itens.Select(i => new ItemEmailInterno(
                i.CodItemBR, i.DescrItemBR, i.PrecoItem, i.IPI, i.ST,
                i.Quantidade, i.VlrUnitario, i.NmSegmento, i.NCM)).ToList()
        };

        var corpo = builder.Construir(titulo, form.Saudacao, mensagem, dadosEmail, hash);

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
