using ClosedXML.Excel;
using SIC.Web.Models.Cotacao;

namespace SIC.Web.Services.Cotacao;

public sealed class CotacaoExcelService(IWebHostEnvironment env)
{
    private static readonly XLColor CinzaHeader = XLColor.FromHtml("#4f4f4f");

    private const string FmtReais   = "R$ #,##0.00";
    private const string FmtDecimal = "#,##0.00";
    private const string FmtPercent = "#,##0.00\"%\"";
    private const string FmtInteiro = "#,##0";

    public async Task<(byte[] FileBytes, string FileName)> GerarExcelAsync(
        CotacaoViewModel model,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => GerarExcel(model), cancellationToken);
    }

    private (byte[] FileBytes, string FileName) GerarExcel(CotacaoViewModel model)
    {
        var freteTexto = string.IsNullOrWhiteSpace(model.Frete) || model.Frete == "R$ 0,00"
            ? "CIF" : model.Frete;

        using var workbook = new XLWorkbook();
        workbook.Style.Font.FontName = "Calibri";
        workbook.Style.Font.FontSize = 10;

        var ws = workbook.Worksheets.Add("Cotacao_" + model.PropostaID);
        ws.ShowGridLines = true;
        ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        ws.PageSetup.PaperSize       = XLPaperSize.A4Paper;
        ws.PageSetup.FitToPages(1, 0);
        ws.PageSetup.Margins.Left   = 0.5;
        ws.PageSetup.Margins.Right  = 0.5;
        ws.PageSetup.Margins.Top    = 0.5;
        ws.PageSetup.Margins.Bottom = 0.5;

        // ── LOGO (linhas 1-4) ───────────────────────────────────────────
        ws.Row(1).Height = 15;
        ws.Row(2).Height = 20;
        ws.Row(3).Height = 20;
        ws.Row(4).Height = 15;

        var logoPath = ObterCaminhoLogo(model.EstabelecimentoID);
        if (File.Exists(logoPath))
        {
            try { ws.AddPicture(logoPath).MoveTo(ws.Cell(1, 1), 4, 4).WithSize(200, 65); }
            catch { /* sem logo */ }
        }

        // ── CABECALHO INFORMATIVO (linhas 6-11) ─────────────────────────
        ws.Row(5).Height = 0;

        InfoCell(ws, 6,  "BR Supply Cotacao Nro " + model.CdProposta, bold: true);
        InfoCell(ws, 7,  "Cotacao: "          + model.Nome);
        InfoCell(ws, 8,  "Cliente: "          + model.ClienteNome);
        InfoCell(ws, 9,  "Frete: "            + freteTexto);
        InfoCell(ws, 10, "Valor Total S/Imposto: " + model.TotalVendaSemImposto);
        InfoCell(ws, 11, "Valor Total C/Imposto: " + model.TotalVendaFrete);

        ws.Row(12).Height = 0;

        // ── CABECALHO DA TABELA (linha 13) ──────────────────────────────
        int headerRow = 13;
        ws.Row(headerRow).Height = 20;

        string[] headers =
        [
            "CodItem", "DescrItem", "Quantidade", "MargemCalculada",
            "ValorPis", "VlrPrecoVenda", "ValorLiqUnit",
            "ICMS", "ST", "IPI", "PercIPI",
            "ValorCofins", "ValorFundoCombPobreza", "MVA", "ValorFCPST",
            "ValorICMSPartilhaOrigem", "ValorICMSPartilhaDestino",
            "NCM", "NmSegmento", "NmFamilia", "NmSubFamilia", "CodBarras"
        ];

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style
                .Fill.SetBackgroundColor(CinzaHeader)
                .Font.SetFontColor(XLColor.White)
                .Font.SetBold(true) 
                .Font.SetFontSize(10)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                .Alignment.SetWrapText(false);
        }

        ws.SheetView.FreezeRows(headerRow);

        // ── LINHAS DE DADOS ─────────────────────────────────────────────
        int dataRow = headerRow + 1;

        foreach (var item in model.Itens ?? [])
        {
            ws.Row(dataRow).Height = 15;

            // Col 1 - CodItem (texto)
            var cCod = ws.Cell(dataRow, 1);
            cCod.Style.NumberFormat.Format = "@";
            cCod.Value = item.CodigoProduto;
            cCod.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

            // Col 2 - DescrItem
            ws.Cell(dataRow, 2).Value = item.DescricaoProduto;

            // Col 3 - Quantidade
            Num(ws, dataRow, 3, item.Quantidade, FmtInteiro);

            // Col 4 - MargemCalculada (%)
            Num(ws, dataRow, 4, item.Margem, FmtPercent);

            // Col 5 - ValorPis (R$)
            Num(ws, dataRow, 5, item.ValorPis, FmtReais);

            // Col 6 - VlrPrecoVenda (R$)
            Num(ws, dataRow, 6, item.VlrPrecoVenda, FmtReais);

            // Col 7 - ValorLiqUnit (R$)
            Num(ws, dataRow, 7, item.ValorLiqUnit, FmtReais);

            // Col 8 - ICMS (%)
            Num(ws, dataRow, 8, item.ICMS, FmtPercent);

            // Col 9 - ST (%)
            Num(ws, dataRow, 9, item.ST, FmtPercent);

            // Col 10 - IPI (%)
            Num(ws, dataRow, 10, item.IPI, FmtPercent);

            // Col 11 - PercIPI (%)
            Num(ws, dataRow, 11, item.PercIPI, FmtPercent);

            // Col 12 - ValorCofins (R$)
            Num(ws, dataRow, 12, item.COFINS, FmtReais);

            // Col 13 - ValorFundoCombPobreza (R$)
            Num(ws, dataRow, 13, item.ValorFundoCombPobreza, FmtReais);

            // Col 14 - MVA (%)
            Num(ws, dataRow, 14, item.MVA, FmtPercent);

            // Col 15 - ValorFCPST (R$)
            Num(ws, dataRow, 15, item.ValorFCPST, FmtReais);

            // Col 16 - ValorICMSPartilhaOrigem (R$)
            Num(ws, dataRow, 16, item.ValorICMSPartilhaOrigem, FmtReais);

            // Col 17 - ValorICMSPartilhaDestino (R$)
            Num(ws, dataRow, 17, item.ValorICMSPartilhaDestino, FmtReais);

            // Col 18 - NCM (texto)
            var cNcm = ws.Cell(dataRow, 18);
            cNcm.Style.NumberFormat.Format = "@";
            cNcm.Value = item.NCM;

            // Col 19-21 - textos
            ws.Cell(dataRow, 19).Value = item.NmSegmento;
            ws.Cell(dataRow, 20).Value = item.NmFamilia;
            ws.Cell(dataRow, 21).Value = item.NmSubFamilia;

            // Col 22 - CodBarras (texto)
            var cbCell = ws.Cell(dataRow, 22);
            cbCell.Style.NumberFormat.Format = "@";
            cbCell.Value = item.CodBarras ?? string.Empty;

            dataRow++;
        }

        // ── AJUSTES FINAIS DE COLUNAS ───────────────────────────────────
        ws.Columns().AdjustToContents(headerRow, dataRow - 1);

        if (ws.Column(2).Width  > 50) ws.Column(2).Width  = 50;
        if (ws.Column(19).Width > 30) ws.Column(19).Width = 30;
        if (ws.Column(20).Width > 30) ws.Column(20).Width = 30;
        if (ws.Column(21).Width > 30) ws.Column(21).Width = 30;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var ts = DateTime.Now.ToString("yyyyMMddHHmmss");
        return (stream.ToArray(), "Excel_Cotacao_" + model.PropostaID + "_" + ts + ".xlsx");
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static void InfoCell(IXLWorksheet ws, int row, string valor, bool bold = false)
    {
        ws.Row(row).Height = 14;
        var cell = ws.Cell(row, 1);
        cell.Value = valor;
        cell.Style
            .Font.SetBold(bold)
            .Font.SetFontSize(10)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
    }

    private static void Num(IXLWorksheet ws, int row, int col, decimal valor, string formato)
    {
        var cell = ws.Cell(row, col);
        cell.Value = valor;
        cell.Style
            .NumberFormat.SetFormat(formato)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
    }

        private string ObterCaminhoLogo(int estabelecimentoId)
        {
            var logoName = estabelecimentoId switch
            {
                1 or 5 or 9 or 11 => "logo-light.png",
                2 or 3             => "logo-sp.png",
                4                  => "logo-sc.png",
                _                  => "logo-light.png"
            };
            var logoPath = Path.Combine(env.WebRootPath, "img", logoName);

            System.Diagnostics.Debug.WriteLine($"[EXCEL] WebRootPath: {env.WebRootPath}");
            System.Diagnostics.Debug.WriteLine($"[EXCEL] Logo Name: {logoName}");
            System.Diagnostics.Debug.WriteLine($"[EXCEL] Logo Path: {logoPath}");
            System.Diagnostics.Debug.WriteLine($"[EXCEL] File Exists: {File.Exists(logoPath)}");

            return logoPath;
        }
    }
