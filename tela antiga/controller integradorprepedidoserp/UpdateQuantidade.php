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

$data = $_POST;

$result = $IntegradorPedidosERPS->UpdateQuantidade(array(
    'PDFPrePedidoItemID' => $data['PDFPrePedidoItemID'],
    'Quantidade' => $data['Quantidade'],
    'ItemID' => $data['ItemID'],
    'Descricao' => $data['Descricao'],
    'PDFPrePedidoID' => $data['PDFPrePedidoID']
));

echo json_encode($result);