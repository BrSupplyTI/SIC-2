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

$result = $IntegradorPedidosERPS->GravarTrocaItem(array(
    'PDFPrePedidoItemId' => $data['PDFPrePedidoItemID'],
    'PDFPrePedidoID' => $data['PDFPrePedidoID'],
    'ItemID' => $data['ItemID'],
    'NmItem' => $data['Descricao'],
    'CdItem' => $data['Cditem'],
    'VlrTabelaPreco' => $data['TblPrecoValorUnitario'],

    'ItemIDAntigo' => $data['ItemIDAntigo'],
    'DescricaoAntiga' => $data['DescricaoAntiga'],
    'CdItemAntigo' => $data['CdItemAntigo'],
    'ValorAntigo' => $data['ValorAntigo'],
    'MotivoTrocaItem' => $data['MotivoTrocaItem']
));
if ($result) {
    echo json_encode(['status' => 'sucesso','message' => 'Salvo com sucesso','result' => true]);
} else {
    echo json_encode(['status' => 'erro', 'message' => 'Não foi possivel salvar', 'result' => false]);
}