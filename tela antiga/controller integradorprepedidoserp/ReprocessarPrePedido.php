<?php

/**
 *
 * PHP Version 5
 *
 * @category Class
 * @package  NewSIC
 * @author   Bruno Bataioli <bruno.bataioli@brsupply.com.br>
 * @license  license name
 * @version  GIT: 1.0.0
 * @link     http://intranet.brsupply.com.br/NewSIC
 */
$OrdemCompra = $data['OrdemCompra'];
$CdExtCliente = $data['CdExtCliente'];
$PDFPrePedidoID = $data['PDFPrePedidoID'];

$ConteudoArquivo = $IntegradorPedidosERPS->GetConteudoArquivoPedido(array(
    'CdExtCliente' => $CdExtCliente,
    'OrdemCompra' => $OrdemCompra
));

$IntegradorPedidosERPS->SetProcessadorPraZero($PDFPrePedidoID);

$Result = $IntegradorPedidosERPS->ReprocessarPedido($ConteudoArquivo);

if ($Result['mensagem'] == 'Pedido gerado com sucesso.') {
    $Html->setFlash('Pré-pedido reprocessado com sucesso!', array('class' => 'success'));
} else {
    $Html->setFlash('Erro ao reprocessar pré-pedido! '. $Result['mensagem'], array('class' => 'error'));
}

echo json_encode(array('Mensagem' => $Result['mensagem'], 'success' => $Result ? true : false));