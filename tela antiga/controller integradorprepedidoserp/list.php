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

$filtroAplicado = isset($_POST['data']) && !empty($_POST['data']);

if (!$filtroAplicado) {
    $data = array(
        'Status' => 1,
        'DataInicial' => date('Y-m-d', strtotime('-1 month')),
        'DataFinal' => date('Y-m-d'),
    );
} else {
    $data = array(
        'Status' => $_POST['data']['Status'] ?? 1,
        'CdExtCliente' => $_POST['data']['CdExtCliente'] ?? '',
        'DataInicial' => $_POST['data']['DataInicial'] ?? '',
        'DataFinal' => $_POST['data']['DataFinal'] ?? '',
    );
}

$statusFormat = '';
switch ($data['Status']) {
    case 1: $statusFormat = 'Aguardando'; break;
    case 4: $statusFormat = 'Aceito'; break;
    case 5: $statusFormat = 'Recusado'; break;
    case 6: $statusFormat = 'Erro'; break;
    case 0: $statusFormat = 'Todos'; break;
}

$title = 'Lista de Pré-Pedidos (Status: ' . $statusFormat . ')';

$dados = $IntegradorPedidosERPS->getList($data);

$statusPrePedido = $IntegradorPedidosERPS->getStatusPrePedido();

echo $Html->setTitle($title);

require "app/views/integradorpedidoerps/list.php";