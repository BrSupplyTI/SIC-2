using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIC.Web.Models.Cotacao;
using System.Globalization;

namespace SIC.Web.Services.Cotacao;

public sealed class CotacaoPdfService(IWebHostEnvironment env, IHttpClientFactory httpClientFactory)
{
    private static readonly CultureInfo PtBR = new("pt-BR");

    // Azul BR Supply
    private const string CorHeaderTabela = "#1a3c6e";
    private const string CorLinhaPar     = "#f0f4f8";

    public byte[] Gerar(CotacaoViewModel model, string executivoVendas, bool comFoto, bool comImpostos)
        => GerarAsync(model, executivoVendas, comFoto, comImpostos).GetAwaiter().GetResult();

    public async Task<byte[]> GerarAsync(CotacaoViewModel model, string executivoVendas, bool comFoto, bool comImpostos)
    {
        // Pré-carrega fotos para evitar chamadas HTTP dentro do builder síncrono do QuestPDF
        Dictionary<string, byte[]> fotoCache = [];
        if (comFoto)
        {
            var http = httpClientFactory.CreateClient();
            foreach (var item in model.Itens ?? [])
            {
                if (string.IsNullOrWhiteSpace(item.CodigoProduto)) continue;
                var url = $"https://www.supplymanager.com.br/fotos/{item.CodigoProduto}.jpg";
                if (fotoCache.ContainsKey(url)) continue;
                try
                {
                    var bytes = await http.GetByteArrayAsync(url);
                    if (IsValidImage(bytes))
                        fotoCache[url] = bytes;
                }
                catch { /* foto indisponível, ignora */ }
            }
        }

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(comImpostos ? PageSizes.A4.Landscape() : PageSizes.A4);
                page.MarginHorizontal(1.5f, Unit.Centimetre);
                page.MarginVertical(1.2f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().Row(row => MontarCabecalho(row, model));
                    col.Item().PaddingTop(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                    col.Item().PaddingTop(5).Row(row => MontarDadosCliente(row, model));
                    col.Item().PaddingTop(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                    col.Item().Height(6);
                });

                page.Content().Column(col =>
                {
                    col.Item().Element(c => MontarTabelaItens(c, model, comFoto, comImpostos, fotoCache));
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                    col.Item().PaddingTop(5).Element(c => MontarRodape(c, model, executivoVendas));
                });
            });
        }).GeneratePdf();
    }

    // ──────────────────────────────────────────────
    // CABEÇALHO
    // ──────────────────────────────────────────────

    private void MontarCabecalho(RowDescriptor row, CotacaoViewModel model)
    {
        var logoPath = Path.Combine(env.WebRootPath, "img", "logo-light.png");

        if (File.Exists(logoPath))
            row.ConstantItem(130).Padding(4).Image(logoPath).FitWidth();

        row.RelativeItem().AlignCenter().PaddingVertical(4).Column(col =>
        {
            col.Item().AlignCenter().Text($"PROPOSTA Nº {model.CdProposta}")
                .Bold().FontSize(14).FontColor(CorHeaderTabela);
            col.Item().AlignCenter().Text($"Emitida em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .FontSize(7.5f).FontColor(Colors.Grey.Medium);
            col.Item().AlignCenter().Text($"Validade: {model.DataValidade}")
                .FontSize(7.5f).FontColor(Colors.Grey.Darken1);
        });

        row.ConstantItem(160).PaddingVertical(4).Column(col =>
        {
            col.Item().AlignRight().Text(model.EstabelecimentoNome).Bold().FontSize(8.5f);
            if (!string.IsNullOrWhiteSpace(model.EstabelecimentoCNPJ))
                col.Item().AlignRight().Text($"CNPJ: {model.EstabelecimentoCNPJ}").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
            col.Item().AlignRight().Text(model.TipoCotacao).FontSize(7.5f).Italic().FontColor(Colors.Grey.Medium);
        });
    }

    // ──────────────────────────────────────────────
    // DADOS DO CLIENTE
    // ──────────────────────────────────────────────

    private static void MontarDadosCliente(RowDescriptor row, CotacaoViewModel model)
    {
        row.RelativeItem().Column(col =>
        {
            CampoInfo(col, "Cliente",    model.ClienteCodNome);
            CampoInfo(col, "CNPJ",       model.ClienteCNPJ);
            CampoInfo(col, "Endereço",   model.ClienteEndereco);

            if (!string.IsNullOrWhiteSpace(model.LocalEntregaEndereco))
                CampoInfo(col, "Local Entrega", $"{model.LocalEntregaEndereco} | {model.LocalEntregaCidadeEstado}");
        });

        row.ConstantItem(8);

        row.RelativeItem().Column(col =>
        {
            CampoInfo(col, "Cond. Pagamento",   model.CondPagtoNome);
            CampoInfo(col, "Prazo de Entrega",  $"{model.DiasPrazoEntrega} dias");
            CampoInfo(col, "Transportadora",    string.IsNullOrWhiteSpace(model.TransportadoraNome) ? "CIF" : model.TransportadoraNome);
            CampoInfo(col, "Nat. Operação",     model.NatOperacao);
        });
    }

    private static void CampoInfo(ColumnDescriptor col, string label, string valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return;
        col.Item().Row(r =>
        {
            r.ConstantItem(95).Text(label + ":").SemiBold().FontSize(7.5f).FontColor(Colors.Grey.Darken2);
            r.RelativeItem().Text(valor).FontSize(7.5f);
        });
    }

    // ──────────────────────────────────────────────
    // TABELA DE ITENS
    // ──────────────────────────────────────────────

    private void MontarTabelaItens(IContainer container, CotacaoViewModel model, bool comFoto, bool comImpostos, Dictionary<string, byte[]> fotoCache)
    {
        var itens = model.Itens ?? [];

        if (comImpostos)
            TabelaComImpostos(container, itens, comFoto, model.ClienteID, fotoCache);
        else
            TabelaSemImpostos(container, itens, comFoto, model.ClienteID, fotoCache);
    }

    private void TabelaSemImpostos(IContainer container, IReadOnlyList<CotacaoItemViewModel> itens, bool comFoto, int clienteId, Dictionary<string, byte[]> fotoCache)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(22);    // #
                cols.ConstantColumn(65);    // Código
                cols.RelativeColumn();      // Descrição
                if (comFoto) cols.ConstantColumn(42); // Foto
                cols.ConstantColumn(48);    // Qtd
                cols.ConstantColumn(68);    // Preço Un.
                cols.ConstantColumn(72);    // Total s/ Imp.
            });

            table.Header(h =>
            {
                CelulaHeader(h.Cell(), "#");
                CelulaHeader(h.Cell(), "Código");
                CelulaHeader(h.Cell(), "Descrição");
                if (comFoto) CelulaHeader(h.Cell(), "Foto");
                CelulaHeaderDir(h.Cell(), "Qtd");
                CelulaHeaderDir(h.Cell(), "Preço Un.");
                CelulaHeaderDir(h.Cell(), "Total s/ Imp.");
            });

            var linha = 1;
            foreach (var item in itens)
            {
                Color bg = linha % 2 == 0 ? CorLinhaPar : Colors.White;

                Celula(table, linha.ToString(), bg);
                Celula(table, item.CodigoProduto, bg);
                Celula(table, item.DescricaoProduto, bg);
                if (comFoto) CelulaFoto(table, item, bg, clienteId, fotoCache);
                CelulaDir(table, item.Quantidade.ToString("N0", PtBR), bg);
                CelulaDir(table, item.PrecoUnitario.ToString("C", PtBR), bg);
                CelulaDir(table, item.TotalSemImposto.ToString("C", PtBR), bg);
            }
        });
    }

    private void TabelaComImpostos(IContainer container, IReadOnlyList<CotacaoItemViewModel> itens, bool comFoto, int clienteId, Dictionary<string, byte[]> fotoCache)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(16);    // #
                cols.ConstantColumn(44);    // Código
                cols.RelativeColumn();      // Descrição
                if (comFoto) cols.ConstantColumn(32); // Foto
                cols.ConstantColumn(44);    // NCM
                cols.ConstantColumn(30);    // Qtd
                cols.ConstantColumn(48);    // Vlr Líq. Un
                cols.ConstantColumn(30);    // IPI
                cols.ConstantColumn(44);    // Vlr IPI
                cols.ConstantColumn(36);    // ICMS
                cols.ConstantColumn(44);    // Vlr ICMS
                cols.ConstantColumn(44);    // Vlr ST
                cols.ConstantColumn(54);    // Total c/ Imp.
            });

            table.Header(h =>
            {
                CelulaHeader(h.Cell(), "#");
                CelulaHeader(h.Cell(), "Código");
                CelulaHeader(h.Cell(), "Descrição");
                if (comFoto) CelulaHeader(h.Cell(), "Foto");
                CelulaHeader(h.Cell(), "NCM");
                CelulaHeaderDir(h.Cell(), "Qtd");
                CelulaHeaderDir(h.Cell(), "Vlr Líq. Un");
                CelulaHeaderDir(h.Cell(), "IPI");
                CelulaHeaderDir(h.Cell(), "Vlr IPI");
                CelulaHeaderDir(h.Cell(), "ICMS");
                CelulaHeaderDir(h.Cell(), "Vlr ICMS");
                CelulaHeaderDir(h.Cell(), "Vlr ST");
                CelulaHeaderDir(h.Cell(), "Total c/ Imp.");
            });

            var linha = 1;
            foreach (var item in itens)
            {
                Color bg = linha % 2 == 0 ? CorLinhaPar : Colors.White;

                Celula(table, linha.ToString(), bg);
                Celula(table, item.CodigoProduto, bg);
                Celula(table, item.DescricaoProduto, bg);
                if (comFoto) CelulaFoto(table, item, bg, clienteId, fotoCache);
                Celula(table, item.NCM, bg);
                CelulaDir(table, item.Quantidade.ToString("N0", PtBR), bg);
                CelulaDir(table, item.ValorLiqUnit.ToString("C", PtBR), bg);
                CelulaDir(table, item.PercIPI.ToString("N2", PtBR) + "%", bg);
                CelulaDir(table, item.IPI.ToString("C", PtBR), bg);
                CelulaDir(table, item.ICMS.ToString("N2", PtBR) + "%", bg);
                CelulaDir(table, item.ValorICMS.ToString("C", PtBR), bg);
                CelulaDir(table, item.ST.ToString("C", PtBR), bg);
                CelulaDir(table, item.TotalComImposto.ToString("C", PtBR), bg);

                linha++;
            }
        });
    }

    // ──────────────────────────────────────────────
    // RODAPÉ
    // ──────────────────────────────────────────────

    private static void MontarRodape(IContainer container, CotacaoViewModel model, string executivoVendas)
    {
        container.Row(row =>
        {
            // Observações + assinaturas
            row.RelativeItem().Column(col =>
            {
                if (!string.IsNullOrWhiteSpace(model.Obs))
                    col.Item().Text($"Obs: {model.Obs}").FontSize(7).Italic().FontColor(Colors.Grey.Darken2);
                if (!string.IsNullOrWhiteSpace(model.Observacao) && model.Observacao != model.Obs)
                    col.Item().Text(model.Observacao).FontSize(7).Italic().FontColor(Colors.Grey.Darken2);

                col.Item().PaddingTop(8).Row(r =>
                {
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingBottom(2)
                            .Text(string.IsNullOrWhiteSpace(executivoVendas) ? " " : executivoVendas).FontSize(7.5f);
                        c.Item().Text("Executivo de Vendas").FontSize(7).FontColor(Colors.Grey.Medium);
                    });
                    r.ConstantItem(12);
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingBottom(2)
                            .Text(string.IsNullOrWhiteSpace(model.ConsultorNome) ? " " : model.ConsultorNome).FontSize(7.5f);
                        c.Item().Text("Consultor").FontSize(7).FontColor(Colors.Grey.Medium);
                    });
                });
            });

            row.ConstantItem(12);

            // Totais + paginação
            row.ConstantItem(190).Column(col =>
            {
                LinhaTotal(col, "Total s/ Impostos:", model.TotalVendaSemImposto, negrito: false);
                LinhaTotal(col, "Total c/ Impostos:", model.TotalVendaFrete, negrito: true);

                if (!string.IsNullOrWhiteSpace(model.Frete) && model.Frete != "R$ 0,00")
                    LinhaTotal(col, "Frete:", model.Frete, negrito: false);

                col.Item().PaddingTop(6).AlignRight()
                    .Text(x =>
                    {
                        x.Span("Página ").FontSize(7).FontColor(Colors.Grey.Medium);
                        x.CurrentPageNumber().FontSize(7).FontColor(Colors.Grey.Medium);
                        x.Span(" de ").FontSize(7).FontColor(Colors.Grey.Medium);
                        x.TotalPages().FontSize(7).FontColor(Colors.Grey.Medium);
                    });
            });
        });
    }

    private static void LinhaTotal(ColumnDescriptor col, string label, string valor, bool negrito)
    {
        col.Item().Row(r =>
        {
            r.RelativeItem().Text(label).FontSize(8).SemiBold();
            r.ConstantItem(90).AlignRight().Text(valor).FontSize(8).Bold();
        });
    }

    // ──────────────────────────────────────────────
    // HELPERS DE CÉLULAS
    // ──────────────────────────────────────────────

    private static void CelulaHeader(IContainer c, string texto) =>
        c.Background(CorHeaderTabela).Padding(4)
            .Text(texto).Bold().FontSize(7).FontColor(Colors.White);

    private static void CelulaHeaderDir(IContainer c, string texto) =>
        c.Background(CorHeaderTabela).Padding(4).AlignRight()
            .Text(texto).Bold().FontSize(7).FontColor(Colors.White);

    private static void Celula(TableDescriptor table, string? texto, Color bg) =>
        table.Cell().Background(bg).Padding(3).Text(texto ?? string.Empty).FontSize(7.5f);

    private static void CelulaDir(TableDescriptor table, string? texto, Color bg) =>
        table.Cell().Background(bg).Padding(3).AlignRight().Text(texto ?? string.Empty).FontSize(7.5f);

    private static void CelulaFoto(TableDescriptor table, CotacaoItemViewModel item, Color bg, int clienteId, Dictionary<string, byte[]> fotoCache)
    {
        byte[]? imgBytes = null;

        if (!string.IsNullOrWhiteSpace(item.CodigoProduto))
        {
            var url = $"https://www.supplymanager.com.br/fotos/{item.CodigoProduto}.jpg";
            fotoCache.TryGetValue(url, out imgBytes);
        }

        if (imgBytes is { Length: > 0 })
            table.Cell().Background(bg).Padding(2).Height(36).Image(imgBytes).FitArea();
        else
            table.Cell().Background(bg).Padding(2).Height(36).Text(string.Empty).FontSize(7.5f);
    }

    private static bool IsValidImage(byte[]? bytes)
    {
        if (bytes is not { Length: > 4 }) return false;
        // JPEG: FF D8 FF
        if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return true;
        // PNG: 89 50 4E 47
        if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47) return true;
        return false;
    }

    private string? ResolverImagemProduto(string numCA)
    {
        var semFoto = Path.Combine(env.WebRootPath, "img", "upload", "sem-foto.jpg");
        return File.Exists(semFoto) ? semFoto : null;
    }
}
