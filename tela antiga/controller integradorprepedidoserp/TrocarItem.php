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

$TblPrecoID = $_POST['TblPrecoID'];
$SegmentoID = $_POST['SegmentoID'];
$FamiliaID  = $_POST['FamiliaID'];
$ItemID     = $_POST['ItemID'];
$estabelecimentoID = $_SESSION['estabelecimentoid'];

$result = $IntegradorPedidosERPS->TrocarItem(array(
    'TblPrecoID' => $TblPrecoID,
    'EstabelecimentoID' => $estabelecimentoID,
    'SegmentoID' => $SegmentoID,
    'FamiliaID' => $FamiliaID,
    'ItemID' => $ItemID
));

echo json_encode($result);