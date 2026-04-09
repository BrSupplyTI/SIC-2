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
$PDFPrePedidoID = $vars[0];
$result = $IntegradorPedidosERPS->CancelarPrePedido($PDFPrePedidoID);

if ($result) {
    $Html->setFlash('Pré-pedido cancelado com sucesso!', array('class' => 'success'));
} else {
    $Html->setFlash('Erro ao cancelar pré-pedido! ', array('class' => 'error'));
}

echo json_encode($result);