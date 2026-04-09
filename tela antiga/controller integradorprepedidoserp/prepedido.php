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

$title = 'Pedido ERP';

$estabelecimentoID = $_SESSION['estabelecimentoid'];

$PDFPrePedidoID = (isset($vars[0]) ? $vars[0] : false);

$prePedido = $IntegradorPedidosERPS->findByID($PDFPrePedidoID);

if ($PDFPrePedidoID) {
    $prePedido = $IntegradorPedidosERPS->findByID($PDFPrePedidoID);

    if (is_null($prePedido)) {
        $Controller->redirect('/integradorpedidoserp/List');
        return;
    }

    $statusPrePedido = $IntegradorPedidosERPS->getStatusPrePedido();
    $prePedido['StatusFormat'] = $statusPrePedido['Descricao'];

} else {
    $Controller->redirect('/integradorpedidoserp/List');
    return;
}

$prePedido['PDFPrePedidoLogs'] = $IntegradorPedidosERPS->GetLogs(array(
    'PDFPrePedidoID' => $PDFPrePedidoID
));

echo $Html->setTitle($title . ': ' . $prePedido['PDFPrePedido']['OrdemCompra']);

$statusPrePedido = $IntegradorPedidosERPS->getStatusPrePedido();
$prePedido['StatusFormat'] = $statusPrePedido['Descricao'];

$ConteudoArquivo = $IntegradorPedidosERPS->GetConteudoArquivoPedido(array(
    'CdExtCliente' => $prePedido['PDFPrePedido']['CdExtCliente'],
    'OrdemCompra' => $prePedido['PDFPrePedido']['OrdemCompra']
));

$prePedido['PDFPrePedido']['Conteudo'] = $ConteudoArquivo;

$clienteID = $prePedido['PDFPrePedido']['ClienteID'];

$prePedido['PDFPrePedido']['ListCNPJ'] = $IntegradorPedidosERPS->GetListCNPJCliente(array(
    'ClienteID' => $clienteID
));

$prePedido['PDFPrePedido']['ListEnderecos'] = $IntegradorPedidosERPS->GetEnderecos(array(
    'ClienteID' => $clienteID
));

$prePedido['PDFPrePedido']['ListLocaisEntrega'] = array();

if (!empty($prePedido['PDFPrePedido']['ClienteEnderecoID'])) {
    $prePedido['PDFPrePedido']['ListLocaisEntrega'] = $IntegradorPedidosERPS->GetLocaisEntrega(array(
        'ClienteEnderecoID' => $prePedido['PDFPrePedido']['ClienteEnderecoID']
    ));
}

$logPrePedido = $IntegradorPedidosERPS->getLogsErro(array(
    'PDFPrePedidoID' => $PDFPrePedidoID
));

require_once "app/views/integradorpedidoerps/prepedido.php";