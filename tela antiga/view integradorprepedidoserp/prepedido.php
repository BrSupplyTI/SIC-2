<input type="hidden" id="PrePedidoID" value="<?php echo $prePedido['PDFPrePedido']['PDFPrePedidoID']; ?>"/>
<input type="hidden" id="inputOrdemCompra" value="<?php echo $prePedido['PDFPrePedido']['OrdemCompra'] ?>" />
<div class="panel panel-default">
    <div class="panel-heading">
        <h3 class="panel-title panel-buttons">
            <?php echo $title ?>
        </h3>
        <div class="panel-actions">
            <?php
            echo $Html->link(
                'Lista de Pré-Pedidos',
                '/integradorpedidoerps/List',
                array(
                    'icon'  =>  'glyphicon-list-alt',
                    'class' =>  'btn btn-default',
                )
            );

            echo $Html->link(
                'Voltar',
                '/integradorpedidoerps/List',
                array(
                    'icon'  =>  'glyphicon-chevron-left',
                    'class' =>  'btn btn-default',
                )
            );
            ?>
        </div>
    </div>
    <div class="panel-body">
        <!-- Resumo do Pedido -->
        <div class="prepedido-resumo">
            <div class="resumo-section">
                <h5 class="resumo-section-title"><i class="fas fa-file-alt"></i> Dados do Pedido</h5>
                <div class="resumo-grid">
                    <div class="resumo-campo">
                        <label>Ordem de Compra</label>
                        <span class="resumo-valor destaque"><?php echo $prePedido['PDFPrePedido']['OrdemCompra']; ?></span>
                    </div>
                    <div class="resumo-campo">
                        <label>Status</label>
                        <span class="resumo-valor">
                            <span class="status-badge status-<?php echo $prePedido['PDFPrePedido']['Status']; ?>">
                                <?php echo $prePedido['PDFPrePedido']['StatusDescricao']; ?>
                            </span>
                        </span>
                    </div>
                    <div class="resumo-campo">
                        <label>Tipo OV SAP</label>
                        <span class="resumo-valor"><?php echo $prePedido['PDFPrePedido']['TipoOVSAP']; ?></span>
                    </div>
                    <div class="resumo-campo">
                        <label>Cond. Pagto</label>
                        <span class="resumo-valor"><?php echo $prePedido['PDFPrePedido']['CondPagto']; ?></span>
                    </div>
                    <div class="resumo-campo">
                        <label>Canal de Venda</label>
                        <span class="resumo-valor"><?php echo $prePedido['PDFPrePedido']['CanalVenda']; ?></span>
                    </div>
                    <div class="resumo-campo">
                        <label>Tabela de Preço</label>
                        <span class="resumo-valor"><?php echo $prePedido['PDFPrePedido']['TabelaPreco']; ?></span>
                    </div>
                    <?php if ($prePedido['PDFPrePedido']['CotacaoID'] != '') : ?>
                        <div class="resumo-campo">
                            <label>Cotação ID</label>
                            <span class="resumo-valor"><?php echo $prePedido['PDFPrePedido']['CotacaoID']; ?></span>
                        </div>
                    <?php endif; ?>
                </div>
            </div>

            <div class="resumo-section">
                <h5 class="resumo-section-title"><i class="fas fa-building"></i> Cliente &amp; Entrega</h5>
                <div class="resumo-grid">
                    <div class="resumo-campo">
                        <label>Cliente</label>
                        <span class="resumo-valor"><?php echo $prePedido['PDFPrePedido']['Cliente']; ?></span>
                    </div>
                    <div class="resumo-campo">
                        <label>Estabelecimento</label>
                        <span class="resumo-valor"><?php echo $prePedido['PDFPrePedido']['Estabelecimento']; ?></span>
                    </div>
                    <div class="resumo-campo">
                        <label>CNPJ Faturamento</label>
                        <select class="form-control chosen-select input-sm" id="selectCNPJ">
                            <option value="">-- Não mapeado --</option>
                            <?php foreach ($prePedido['PDFPrePedido']['ListCNPJ'] as $cnpj) : ?>
                                <option value="<?php echo $cnpj['CPFCNPJ']; ?>"
                                    <?php echo ($cnpj['CPFCNPJ'] == $prePedido['PDFPrePedido']['CNPJ']) ? 'selected' : ''; ?>>
                                    <?php echo $cnpj['CPFCNPJ']; ?>
                                </option>
                            <?php endforeach; ?>
                        </select>
                    </div>
                    <div class="resumo-campo">
                        <label>Endereço</label>
                        <select class="form-control chosen-select input-sm" id="selectClienteEndereco">
                            <option value="">-- Não mapeado --</option>
                            <?php foreach ($prePedido['PDFPrePedido']['ListEnderecos'] as $local) : ?>
                                <option value="<?php echo $local['ClienteEnderecoID']; ?>"
                                    <?php echo $local['ClienteLocarEnderecoID'] == $local['PDFPrePedido']['ClienteLocarEnderecoID'] ? 'selected' : ''; ?>>
                                    <?php echo $local['Logradouro']; ?>
                                </option>
                            <?php endforeach; ?>
                        </select>
                    </div>
                    <div class="resumo-campo">
                        <label>Local de Entrega</label>
                        <select class="form-control chosen-select input-sm" id="selectLocalEntrega">
                            <option value="">-- Não mapeado --</option>
                            <?php foreach ($prePedido['PDFPrePedido']['ListLocaisEntrega'] as $local) : ?>
                                <option value="<?php echo $local['ClienteLocalEntregaID']; ?>" <?php echo ($local['ClienteLocalEntregaID'] == $prePedido['PDFPrePedido']['LocalEntregaID']) ? 'selected' : ''; ?>>
                                    <?php echo $local['CdControle']." - ".$local['NmLocalEntrega']; ?>
                                </option>
                            <?php endforeach; ?>
                        </select>
                    </div>
                </div>
            </div>

            <div class="resumo-section resumo-valores">
                <h5 class="resumo-section-title"><i class="fas fa-dollar-sign"></i> Valores</h5>
                <div class="resumo-grid valores-grid">
                    <div class="resumo-campo valor-card" id="vlrminimo">
                        <label>Valor Min. Pedido</label>
                        <span class="resumo-valor valor-monetario"><?php echo 'R$ ' . $Format->moeda($prePedido['VlrMinimoBloqueioPedido']); ?></span>
                    </div>
                    <div class="resumo-campo valor-card" id="vlrtotalpedido">
                        <label>Valor Total do Pedido</label>
                        <span class="resumo-valor valor-monetario valor-total"><?php echo $prePedido['PDFPrePedido']['Itens'][0]['PDFPrePedidoItem']['VlrTotalPedido']; ?></span>
                    </div>
                </div>
            </div>
        </div>

        <!-- Ações -->
        <div class="prepedido-acoes text-center">
            <?php
                if (empty($prePedido['PDFPrePedido']['CotacaoID'])) {
                    echo $Html->link(
                        'Reprocessar',
                        'javascript:void(0)',
                        array(
                            'icon'  =>  'glyphicon-refresh',
                            'class' =>  'btn btn-default reprocessar',
                            'data-ordemcompra'  =>  $prePedido['PDFPrePedido']['OrdemCompra'],
                            'data-cdextcliente' =>  $prePedido['PDFPrePedido']['CdExtCliente'],
                        )
                    );
                }
            ?>
            &nbsp;
            <button type="button" class="btn btn-default" id="conteudoArquivo">
                <i class="fas fa-search my-mr-05"></i>Conteúdo do Arquivo
            </button>
            &nbsp;
            <?php if (in_array($prePedido['PDFPrePedido']['Status'], array('1', '2', '3', '7'))): ?>
                <?php
                    echo $Html->link(
                        '<span style="color:black;">Adicionar Item</span>',
                        '#;',
                        array(
                            'icon'  => 'glyphicon glyphicon-plus black',
                            'class' => 'btn btn-default',
                            'data-toggle' => 'modal',
                            'data-target' => '#modalAdicionarItem',
                            'alt'   => 'Adiciona Item a Proposta',
                            'title' => 'Adiciona Item a Proposta',
                            'style' => 'font-size: 14px;'
                        )
                    );
                ?>
                &nbsp;
                <button type="button" class="btn btn-default" id="cancelarPrePedido">
                    <i class="fas fa-ban my-mr-05"></i>Cancelar Pré-Pedido
                </button>
                &nbsp;
                <?php
                $dadosPedidoValidos = ($prePedido['PDFPrePedido']['CNPJ'] != '' &&$prePedido['PDFPrePedido']['Endereco'] != '' &&$prePedido['PDFPrePedido']['NmLocalEntrega'] != ''); $itensValidos = true;
                foreach ($prePedido['PDFPrePedido']['Itens'] as $item) { 
                    if (
                        $item['PDFPrePedidoItem']['ItemCliente'] == '' || $item['PDFPrePedidoItem']['ItemBrSupply'] == '' || $item['PDFPrePedidoItem']['PDFQtde'] == 0 || $item['PDFPrePedidoItem']['VlrTblPrecoFormat'] == '') {
                        $itensValidos = false;
                        break;
                    }
                }
                ?>
                <?php if ($dadosPedidoValidos && $itensValidos): ?>
                    <button type="button" class="btn btn-default" id="gerarPedido">
                        <i class="fas fa-play my-mr-05"></i>Aceitar Pedido
                    </button>
                <?php endif; ?>
            <?php endif; ?>
        </div>
    </div>
    <div class="row">
        <div class="col-xs-12 col-sm-12 col-md-12 col-lg-12">
            <table id="my-table-basic-fixed-header" class="table table-hover">
                <thead>
                    <tr>
                        <th class="">Item Cliente</th>
                        <th class="">Item BrSupply</th>
                        <th class="">Vrl.Unit. (PDF)</th>
                        <th class="">Qt.Item</th>
                        <th class="">Vlr.Unit. (Tabela)</th>
                        <th class="no-sort">Vlr.Total</th>
                        <th class="text-right no-sort">Ações</th>
                    </tr>
                </thead>
                    <?php foreach ($prePedido['PDFPrePedido']['Itens'] as $item) { ?>
                        <tr id="row_<?php echo $item['PDFPrePedidoItem']['PDFPrePedidoItemID']; ?>"
                            data-PDFPrePedidoID="<?php echo $PDFPrePedidoID; ?>"
                            data-PDFPrePedidoItemID="<?php echo $item['PDFPrePedidoItem']['PDFPrePedidoItemID']; ?>">

                            <td class="text-left">
                                <?php echo $item['PDFPrePedidoItem']['ItemCliente']; ?>
                            </td>
                            <td class="text-left">
                                <a href="../Intranet/html/produtos.php?codigo=<?php echo ($item['PDFPrePedidoItem']['ItemBrSupplyCdItem']); ?>" target="_blank">
                                    <?php echo ($item['PDFPrePedidoItem']['ItemBrSupply']); ?>
                                    &nbsp;&nbsp;&nbsp;<i class="fas fa-search"></i>
                                </a>
                            </td>
                            <td class="text-left">
                                <?php if ($item['PDFPrePedidoItem']['VlrTblPrecoFormat'] == ''): ?>
                                    <?php echo 'R$&nbsp;' . $Format->moeda($item['PDFPrePedidoItem']['PDFVlrUnit']); ?>
                                <?php else : ?>
                                    <?php echo $item['PDFPrePedidoItem']['VlrTblPrecoFormat']; ?>
                                <?php endif; ?>
                            </td>
                            <td class="text-left">
                                <?php if ($prePedido['PDFPrePedido']['StatusDescricao'] == 'Aceito'): ?>
                                    <?php echo $item['PDFPrePedidoItem']['PDFQtde']; ?>
                                <?php else: ?>
                                    <input type="text" id="quant" class="form-control input-table quantid" style="text-align: right;"
                                    value="<?php echo $item['PDFPrePedidoItem']['PDFQtde']; ?>" 
                                    data-default="<?php echo $item['PDFPrePedidoItem']['PDFPrePedidoItemID']; ?>"
                                    data-itemid="<?php echo $item['PDFPrePedidoItem']['ItemID']; ?>"
                                    data-descricao="<?php echo $item['PDFPrePedidoItem']['ItemBrSupply']; ?>">
                                <?php endif; ?>
                            </td>
                            <td class="text-left">
                                <?php if ($item['PDFPrePedidoItem']['VlrTblPrecoFormat'] != ''): ?>
                                    <?php echo 'R$&nbsp;' . $Format->moeda($item['PDFPrePedidoItem']['PDFVlrUnit']); ?>
                                <?php endif; ?>
                            </td>
                            <td class="text-left">
                                <?php echo $item['PDFPrePedidoItem']['VlrTotal']; ?>
                            </td>
                            <td class="text-right">
                                <a href="javascript:void(0);" alt="Troca de Item" title="Troca de Item"  id="trocarItem" class="glyphicon glyphicon-refresh" 
                                data-pdfprepedidoitemid="<?= $item['PDFPrePedidoItem']['PDFPrePedidoItemID'] ?>"
                                data-itemcliente="<?= $item['PDFPrePedidoItem']['ItemCliente'] ?>"
                                data-descricao="<?= $item['PDFPrePedidoItem']['Descricao'] ?>"
                                data-itembrsupply="<?= $item['PDFPrePedidoItem']['ItemBrSupply']?>"
                                data-pdfqtde="<?= $item['PDFPrePedidoItem']['PDFQtde'] ?>"
                                data-vlrtblprecoformat="<?= $item['PDFPrePedidoItem']['VlrTblPrecoFormat']?>"
                                data-itemid="<?=  $item['PDFPrePedidoItem']['ItemID'] ?>"
                                data-familiaid="<?=  $item['FamiliaID'] ?>"
                                data-segmentoid="<?=  $item['SegmentoID'] ?>"
                                data-tblpreco="<?php echo $prePedido['PDFPrePedido']['TblPrecoID']?>"></a>
                                <a href="javascript:void(0);" alt="Excluir item?" title="Excluir Item?" class="glyphicon glyphicon-trash excluiritem" data-toggle="confirmation" data-btn-ok-label="Sim" data-btn-cancel-label="Não"
                                 data-title="Excluir Item?" data-pdfprepedidoitemid="<?= $item['PDFPrePedidoItem']['PDFPrePedidoItemID'] ?>" 
                                 data-itemid="<?= $item['PDFPrePedidoItem']['ItemID'] ?>" 
                                 data-descricao="<?= $item['PDFPrePedidoItem']['ItemBrSupply'] ?>"></a>
                            </td>
                        </tr>
                    <?php } ?>
            </table>
        </div>
    </div>
    <div class="panel-footer text-right">
        <a href="javascript:void(0);" class="btn btn-default geara-excel-prepedido" data-prepedido-id="<?php echo $PDFPrePedidoID; ?>" style="font-size:12px;">Excel</a>
        <a href="#" class="btn btn-default gerar-pdf-prepedido" data-prepedido-id="<?php echo $PDFPrePedidoID; ?>" style="font-size:12px;">PDF</a>
    </div>
</div>

<hr>

<div class="panel panel-default" id="Logs">
    <div class="panel-heading">
        <h3 class="panel-title panel-buttons">
            Logs do Pré-Pedido
        </h3>
        <div class="panel-actions">
            &nbsp;
        </div>
    </div>
    <div class="panel-body">
        <div class="row">
            <div class="col-xs-12 col-sm-12 col-md-12 col-lg-12">
                <table id="my-table-basic-fixed-header2" class="table table-striped table-hover">
                    <thead>
                        <tr>
                            <th class="text-left">Log</th>
                            <th class="text-left">Data e hora</th>
                        </tr>
                    </thead>
                    <tbody>
                        <?php foreach ($prePedido['PDFPrePedidoLogs'] as $log) { ?>
                            <tr>
                                <td class="text-left">
                                    <?php echo $log['Mensagem']; ?>
                                </td>
                                <td class="text-left">
                                    <?php echo $log['CriadoEmFormatado']; ?>
                                </td>
                            </tr>
                        <?php } ?>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
    <div class="panel-footer">
        &nbsp;
    </div>
</div>

<div class="modal fade" id="modalTrocarItem">
    <div class="modal-dialog modal-md">
        <div class="modal-content modal-trocar-item">
            <input type="hidden">
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal">&times;</button>
                <h4 class="modal-title">Trocar Item</h4>
            </div>
            <div class="modal-body">
                <div class="linha-info">
                    <strong>Código:</strong>
                    <span class="valor-info" id="codigoTrocaItem"></span>
                </div>
                <div class="linha-info">
                    <strong>Quantidade:</strong>
                    <span class="valor-info" id="quantidadeTrocaItem"></span>
                </div>
                <div class="linha-info">
                    <strong>Valor Tabela:</strong>
                    <span class="valor-info" id="valorTrocaItem"></span>
                </div>
                <div class="campo-form">
                    <input type="hidden" id="ItemIDAntigo">
                    <input type="hidden" id="DescricaoAntiga">
                    <input type="hidden" id="ValorAntigo">
                    <input type="hidden" id="CdItemAntigo">
                    
                    <input type="hidden" id="itemIDNovo">
                    <input type="hidden" id="nmItemNovo">
                    <input type="hidden" id="tblPrecoNovo">
                    <input type="hidden" id="cdItemNovo">
                    <label for="itemSubstituto">Item Substituto:</label>
                    <select class="form-control campo-custom chosen-select" id="itemSubstituto">
                        <option value="">Selecione</option>
                    </select>
                </div>
                <div class="campo-form">
                    <label for="motivoTrocaItem">Motivo da alteração:</label>
                    <input type="text" class="form-control campo-custom" id="motivoTrocaItem">
                </div>
            </div>
            <div class="modal-footer">
                <button class="btn btn-default pull-right" id="gravar">Gravar</button>
            </div>
        </div>
    </div>
</div>

<div class="modal fade" id="modalVerConteudoArquivo">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                <h4 class="modal-title">Conteúdo do Arquivo</h4>
            </div>
            <div class="modal-body">
                <div class="form-group">
                    <textarea class="form-control" rows="25" readonly><?php echo $prePedido['PDFPrePedido']['Conteudo']; ?></textarea>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-default" data-dismiss="modal">Fechar</button>
                </div>
            </div>
        </div>
    </div>
</div>

<div class="modal fade modalitem" id="modalAdicionarItem">
    <div class="modal-dialog modal-lg" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h4 class="modal-title" id="modalAdicionarItemLabel">Adicionar Novo Item 
                    <button type="button" class="close" data-dismiss="modal" aria-label="Fechar">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </h4>
            </div>
            <div class="modal-body">
                <div class="row">
                    <div class="col-sm-12" style="margin-bottom: 5px;">
                        <form id="formSearchProds" action="#">
                            <div class="input-group">
                                <input type="hidden" id="inputTblPrecoID" value="<?php echo $prePedido['PDFPrePedido']['TblPrecoID']; ?>">
                                <input type="hidden" id="inputClienteID" value="<?php echo $prePedido['PDFPrePedido']['ClienteID']; ?>">
                                <input type="hidden" id="inputEstabelecimentoID" value="<?php echo $prePedido['PDFPrePedido']['EstabelecimentoID']; ?>">
                                <input type="text" id="inputSearchProds" autocomplete="off" value="" style="height:35px;" class="form-control" placeholder="Descrição/Código do Produto">
                                <span class="input-group-btn">
                                    <button id="search-product-calcs" class="btn btn-default" type="submit">
                                        <span class="glyphicon glyphicon-search" aria-hidden="true"></span>
                                        Pesquisar
                                    </button>
                                </span>
                            </div>
                        </form>
                    </div>

                    <div class="col-sm-12">
                        <table class="search add tablesorter table table-bordered table-striped" width="100%" id="tablesorterCotacao">
                            <thead>
                                <tr>
                                    <th width="5%" class="text-left sorter-false">&nbsp;</th>
                                    <th width="5%" class="text-left sort">Cód.<br>Item</th>
                                    <th class="text-left">Produto</th>
                                    <th width="7%" class="text-right">Tbl.<br>Preço</th>
                                    <th width="7%" class="text-right">Qtd.<br>Aquisição</th>
                                    <th width="7%" class="text-right">Qtd.<br>Disp</th>
                                </tr>
                            </thead>
                            <tbody id="tbodyItens" class="tbodyitens">
                                <tr>
                                    <td colspan="6" class="text-center">...</td>
                                </tr>
                            </tbody>
                            <tfoot>
                                <tr>
                                    <td colspan="6" class="text-left divAddItensMarcados hide">
                                        <button type="button" class="btn btn-primary" id="addItensMarcados">Adicionar Itens</button>
                                    </td>
                                </tr>
                            </tfoot>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<div id="modalQuantidade" class="modal fade" tabindex="-1" role="dialog">
    <div class="modal-dialog modal-sm" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Qual a quantidade?
                    <button type="button" class="close" data-dismiss="modal" aria-label="Fechar">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </h5>
            </div>
            <div class="modal-body">
                <input type="number" id="quantid" class="form-control" value="1" min="1">
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-primary btnGravar">
                    Adicionar
                </button>
            </div>
        </div>
    </div>
</div>

<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/xlsx/0.18.5/xlsx.full.min.js"></script>