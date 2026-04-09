<?php
/**
 *
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

$data = $_POST;

$result = $IntegradorPedidosERPS->AdicionarItens(array(
    'PDFPrePedidoID'    => $data['PDFPrePedidoID'],
    'CodItemBR'         => $data['CodItemBR'],
    'DescrItemBR'       => $data['DescrItemBR'],
    'Quantidade'        => $data['Quantidade'],
    'PrecoTbl'          => $data['PrecoTbl'],
    'ItemDePara'        => $data['ItemDePara'],
    'ItemID'            => $data['ItemID'],
    'OrdemCompra'       => $data['OrdemCompra']
));
if ($result) {
    echo json_encode(['status' => 'sucesso','message' => 'Adicionado com sucesso','result' => true]);
} else {
    echo json_encode(['status' => 'erro', 'message' => 'Não foi possivel adicionar', 'result' => false]);
}
