<div class="panel panel-default">
    <div class="panel-heading">
        <h3 class="panel-title panel-buttons">
            <?php echo $title; ?>
        </h3>
        <div class="panel-actions">
            <?php
            echo $Html->link(
                'Filtro',
                '#',
                array(
                    'icon'        => 'glyphicon-search',
                    'class'       => 'btn btn-default ' . ($filtroAplicado ? 'active' : ''),
                    'data-toggle' => 'modal',
                    'data-target' => '#filtro',
                )
            );
            echo $Html->link(
                'Limpar Filtro',
                '#',
                array(
                    'icon'  => 'glyphicon-repeat',
                    'class' => 'btn btn-default reset',
                )
            );
            echo $Html->link(
                'Atualizar',
                '#',
                array(
                    'icon'  => 'glyphicon-refresh',
                    'class' => 'btn btn-default reload',
                )
            );
            echo $Html->link(
                'Voltar',
                '#',
                array(
                    'icon'  => 'glyphicon-chevron-left',
                    'class' => 'btn btn-default to-back',
                )
            );
            ?>
        </div>
    </div>
    <div class="panel-body">
        <div class="row"></div>
        <div class="row">
            <div class="col-sm-12">
                <table id="my-table-basic-fixed-header" class="table table-striped table-bordered">
                    <thead>
                        <tr>
                            <th class="text-center">Arquivo</th>
                            <th class="text-left">Cliente</th>
                            <th class="text-left">Ordem de Compra</th>
                            <th class="text-left">CNPJ</th>
                            <th class="text-left">Pedido BRS</th>
                            <th class="text-left">Status</th>
                            <th class="text-center">Criado Em</th>
                            <th class="text-center no-sort">Ações</th>
                        </tr>
                    </thead>
                    <tbody>
                        <?php if (!empty($dados)) : ?>
                            <?php foreach ($dados as $row) : ?>
                                <tr>
                                    <td class="text-center"><?php echo $row['PDFPrePedidoID']; ?></td>
                                    <td class="text-left"><?php echo $row['NmCliente']; ?></td>
                                    <td class="text-left"><?php echo $row['OrdemCompra']; ?></td>
                                    <td class="text-left"><?php echo $row['CNPJ']; ?></td>
                                    <td class="text-left">
                                        <?php if ($row['CotacaoID'] > 0) : ?>
                                            <a alt="Ver Pedido" title="Ver Pedido" href="../../../Intranet/html/pedidos.php?pedido=<?php echo $row['CotacaoID']; ?>" class="btn btn-success btn-xs">
                                                <i class="fas fa-file"></i> <?php echo $row['CotacaoID']; ?>
                                            </a>
                                        <?php endif; ?>
                                    </td>
                                    <td class="text-left"><?php echo $row['StatusDescricao']; ?></td>
                                    <td class="text-center"><?php echo $row['CriadoEm']; ?></td>
                                    <td class="text-center">
                                        <?php
                                        echo $Html->link(
                                            '<i class="fas fa-th-list"></i>',
                                            '/integradorpedidoerps/PrePedido/' . $row['PDFPrePedidoID'],
                                            array(
                                                'title' => 'Ver Detalhes',
                                                'alt' => 'Ver Detalhes',
                                            )
                                        );
                                        ?>
                                    </td>
                                </tr>
                            <?php endforeach; ?>
                        <?php else : ?>
                            <tr>
                                <td colspan="8" class="text-center">Nenhum registro encontrado.</td>
                            </tr>
                        <?php endif; ?>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
    <div class="panel-footer small">
        <div class="row">
            <div class="col-xs-12 col-sm-12 col-md-12 col-lg-12">
                <?php if ($filtroAplicado) : ?>
                    <strong>Filtro Aplicado:</strong>
                    <br>
                    Status: <u><?php echo $statusFormat; ?></u>
                <?php else : ?>
                    <span class="leg-table">
                        * Use o filtro para listar pré-pedidos por status.
                    </span>
                <?php endif; ?>
            </div>
        </div>
    </div>
</div>

<div id="filtro" class="modal fade" tabindex="-1" role="dialog">
    <div class="modal-dialog" role="document">
        <div class="modal-content modal-md">
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
                <h4 class="modal-title">Filtros</h4>
            </div>
            <div class="modal-body row">
                <?php
                    echo $Form->create('PDFPrePedido', '/integradorpedidoerps/List', array(
                        'method' => 'POST',
                        'class' => 'filter'
                    ));
                ?>
                <div class="col-sm-12">
                    <div class="form-group">
                        <label for="">Status</label>
                        <select name="data[Status]" class="form-control" data-placeholder="Selecione um status">
                            <option value="0">Todos</option>
                            <?php foreach ($statusPrePedido as $key) : ?>
                                <option value="<?php echo $key['StatusPrePedidoId']; ?>" <?php echo (isset($data['Status']) && $data['Status'] == $key['StatusPrePedidoId']) ? 'selected' : ''; ?>>
                                    <?php echo $key['Descricao']; ?>
                                </option>
                            <?php endforeach; ?>
                        </select>

                        <label for="">Código do cliente</label>
                        <input type="text" name="data[CdExtCliente]" class="form-control" value="<?php echo isset($data['CdExtCliente']) ? $data['CdExtCliente'] : ''; ?>" placeholder="Código do cliente">

                        <label for="">Data Inicial</label>
                        <input type="date" name="data[DataInicial]" class="form-control" value="<?php echo isset($data['DataInicial']) ? $data['DataInicial'] : ''; ?>" placeholder="Data Inicial">

                        <label for="">Data Final</label>
                        <input type="date" name="data[DataFinal]" class="form-control" value="<?php echo isset($data['DataFinal']) ? $data['DataFinal'] : ''; ?>" placeholder="Data Final">
                    </div>
                </div>

            </div>
            <div class="modal-footer">
                <button type="submit" class="btn btn-default btn-filter">
                    <span class="glyphicon glyphicon-search" aria-hidden="true"></span>
                    Buscar
                </button>
                <!-- <button type="reset" class="btn btn-default reset">
                    <span class="glyphicon glyphicon-repeat" aria-hidden="true"></span>
                    Limpar
                </button> -->
            </div>
            <?php echo $Form->end(); ?>
        </div>
    </div>
</div>