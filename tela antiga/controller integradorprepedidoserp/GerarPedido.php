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
$ItensQuantidadeZero = $IntegradorPedidosERPS->GetIntegradorPedidosItensQuantidadeZero($PDFPrePedidoID);

if ($ItensQuantidadeZero != 0){
    $Html->setFlash('Existem itens com quantidade ou valor unitario zero!', array('class' => 'error'));
    echo json_encode(array('id' => $PDFPrePedidoID, 'success' => false, 'ItensQuantidadeZero' => true));
    return;
}

$validacao = $IntegradorPedidosERPS->ValidarParaAceite($PDFPrePedidoID);

if ($validacao != 'pode aceitar'){
    $Html->setFlash('Erro ao gerar pedido! ' . $validacao, array('class' => 'error'));
    echo json_encode(array('id' => $PDFPrePedidoID, 'success' => false, 'ItensQuantidadeZero' => false));
    return;
}

$result = $IntegradorPedidosERPS->GerarPedido(array(
    'PDFPrePedidoID' => $PDFPrePedidoID
));

if ($result['Mensagem'] == 'OK'){
    $Html->setFlash('Pedido gerado com sucesso!', array('class' => 'success'));
} else {
    $Html->setFlash('Erro ao gerar pedido! ' . $result['Mensagem'], array('class' => 'error'));
}

echo json_encode(array('Mensagem' => $result['Mensagem'], 'success' => $result ? true : false, 'ItensQuantidadeZero' => false));