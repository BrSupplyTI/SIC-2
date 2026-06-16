using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Web.Models.InteligenciaDeBusca;
using SIC.Web.Services.Abreviacoes;
using SIC.Web.Services.InteligenciaDeBusca;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace SIC.Web.Controllers;

[Authorize]
[Route("InteligenciaDeBusca")]
public sealed class InteligenciaDeBuscaController(
    IHttpClientFactory httpClientFactory,
    AbreviacaoApiClient abreviacaoApiClient,
    CategorizacaoApiClient categorizacaoApiClient) : Controller
{
    private const string HistoricoUrl = "https://brsupply.vortigo.tech/history/";

    [HttpGet("")]
    [HttpGet("InteligenciaDeBusca")]
    public IActionResult InteligenciaDeBusca() => View();

    [HttpGet("CategorizacaoDosItens")]
    public async Task<IActionResult> CategorizacaoDosItens(CancellationToken cancellationToken)
    {
        var taskItens      = categorizacaoApiClient.GetItensCategorizadosAsync(null, cancellationToken);
        var taskSemCat     = categorizacaoApiClient.GetItensSemCategoriaAsync(cancellationToken);
        var taskCategorias = categorizacaoApiClient.GetCategoriasAsync(cancellationToken);

        await Task.WhenAll(taskItens, taskSemCat, taskCategorias);

        var vm = new CategorizacaoPageViewModel
        {
            Itens             = taskItens.Result,
            ItensSemCategoria = taskSemCat.Result,
            Categorias        = taskCategorias.Result,
        };
        return View(vm);
    }

    [HttpGet("CategorizacaoDosItens/ExportarExcel")]
    public async Task<IActionResult> ExportarCategorizacaoExcel(CancellationToken cancellationToken)
    {
        var itens = await categorizacaoApiClient.GetItensCategorizadosAsync(null, cancellationToken);

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Categorizacao");

        ws.Cell(1, 1).Value = "Estabelecimento";
        ws.Cell(1, 2).Value = "Cód. Item";
        ws.Cell(1, 3).Value = "Nome do Item";
        ws.Cell(1, 4).Value = "Criticidade";
        ws.Cell(1, 5).Value = "Custo Aquisição";
        ws.Cell(1, 6).Value = "Estoque Disp.";
        ws.Cell(1, 7).Value = "Categoria";
        ws.Cell(1, 8).Value = "Prioridade";

        var header = ws.Range(1, 1, 1, 8);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#2c3e50");
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var row = 2;
        foreach (var item in itens)
        {
            ws.Cell(row, 1).Value = item.NmEstabelecimento;
            ws.Cell(row, 2).Value = item.CdItem;
            ws.Cell(row, 3).Value = item.NmItem;
            ws.Cell(row, 4).Value = item.Criticidade;
            ws.Cell(row, 5).Value = item.VlrCustoAquisicaoFormat;
            ws.Cell(row, 6).Value = item.QtDispEstoque;
            ws.Cell(row, 7).Value = item.Categoria ?? "Sem categoria";
            ws.Cell(row, 8).Value = item.Prioridade?.ToString() ?? "";
            row++;
        }

        ws.Columns().AdjustToContents();
        ws.RangeUsed()!.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.RangeUsed()!.Style.Border.InsideBorder  = XLBorderStyleValues.Hair;

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"categorizacao_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpPost("CategorizacaoDosItens/SalvarCategoria")]
    public async Task<IActionResult> SalvarCategoria([FromBody] SalvarCategoriaRequest request, CancellationToken cancellationToken)
    {
        var ok = await categorizacaoApiClient.SalvarCategoriaAsync(request.ItemID, request.PesquisaTipoListaID, cancellationToken);
        return Ok(new { result = ok });
    }

    [HttpDelete("CategorizacaoDosItens/RemoverCategoria")]
    public async Task<IActionResult> RemoverCategoria([FromBody] RemoverCategoriaRequest request, CancellationToken cancellationToken)
    {
        var ok = await categorizacaoApiClient.RemoverCategoriaAsync(request.ItemID, cancellationToken);
        return Ok(new { result = ok });
    }

    [HttpGet("Abreviacoes")]
    public async Task<IActionResult> Abreviacoes(CancellationToken cancellationToken)
    {
        var dados = await abreviacaoApiClient.BuscarDadosAsync(cancellationToken);
        return View(dados);
    }

    [HttpPost("Abreviacoes/Gravar")]
    public async Task<IActionResult> GravarAbreviacao([FromBody] GravarAbreviacaoRequest request, CancellationToken cancellationToken)
    {
        var usuarioId = int.TryParse(User.FindFirstValue("sic_usuarioid"), out var uid) ? uid : 0;
        var ok = await abreviacaoApiClient.GravarAsync(request.Texto, request.Abreviacao, usuarioId, cancellationToken);
        return Ok(new { result = ok });
    }

    [HttpDelete("Abreviacoes/Excluir/{id:int}")]
    public async Task<IActionResult> ExcluirAbreviacao(int id, CancellationToken cancellationToken)
    {
        var ok = await abreviacaoApiClient.ExcluirAsync(id, cancellationToken);
        return Ok(new { result = ok });
    }

    [HttpPost("EnviarHistoricoItem")]
    public async Task<IActionResult> EnviarHistoricoItem([FromBody] HistoricoItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var client  = httpClientFactory.CreateClient();
            var payload = JsonSerializer.Serialize(new { query = request.Query, reference_id = request.ReferenceId });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(HistoricoUrl, content, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return StatusCode((int)response.StatusCode, new { ok = response.IsSuccessStatusCode, status = (int)response.StatusCode, response = body });
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { ok = false, error = ex.Message });
        }
    }
}

public sealed record GravarAbreviacaoRequest(string Texto, string Abreviacao);
public sealed record HistoricoItemRequest(string Query, string ReferenceId);
public sealed record RemoverCategoriaRequest(int ItemID);