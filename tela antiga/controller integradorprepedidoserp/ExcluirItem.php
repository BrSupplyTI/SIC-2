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

$PDFPrePedidoItemId = (isset($vars[0]) ? $vars[0] : 0);
$PDFPrePedidoID = (isset($vars[1]) ? $vars[1] : 0);

$item = $IntegradorPedidosERPS->ExcluirItem(array(
    'PDFPrePedidoItemId' => $PDFPrePedidoItemId,
    'PDFPrePedidoID' => $PDFPrePedidoID,
    'ItemID' => (isset($_POST['ItemID']) ? $_POST['ItemID'] : ''),
    'Descricao' => (isset($_POST['Descricao']) ? $_POST['Descricao'] : '')
));

echo json_encode(array(
    'success' => $item ? true : false,
    'data' => $item
));

echo json_encode($result);