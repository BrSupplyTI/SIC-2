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

$description = $data['description'] ?? null;
$TblPrecoId = $data['TblPrecoID'] ?? null;
$ClienteId = $data['ClienteID'] ?? null;
$EstabelecimentoId = $data['EstabelecimentoID'] ?? null;

$ItensPrePedido = $IntegradorPedidosERPS->BuscarCatalogo(array(
    'ClienteID' => $ClienteId,
    'TblPrecoID' => $TblPrecoId,
    'Descricao' => $description,
    'EstabelecimentoID' => $EstabelecimentoId
));

if (is_null($ItensPrePedido) || empty($ItensPrePedido)) {
    echo json_encode([]);
    return;
}

$nprods = array();

foreach ($ItensPrePedido as $produto) {
    if (isset($produto['Produto']['CdItem'])) {
        $img = NEWMANAGER . '/fotos/low/' . $produto['Produto']['CdItem'] . '.jpg';
        $produto['Produto']['Imagem'] = $img;
        $nprods[]['Produto'] = $produto['Produto'];
    }
}

echo json_encode($nprods);