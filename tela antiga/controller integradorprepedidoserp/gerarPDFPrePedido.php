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

if (!isset($data['PDFPrePedidoID']) || empty($data['PDFPrePedidoID'])) {
    exit;
}

$PDFPrePedidoID = $data['PDFPrePedidoID'];

$filename = 'PrePedido_' . $PDFPrePedidoID . '_' . date('YmdHis') . '.pdf';

$prePedido = $IntegradorPedidosERPS->findByID($PDFPrePedidoID);

if (empty($prePedido['PDFPrePedido']['Itens'])) {
    die(json_encode(['erro' => 'Itens não encontrados']));
}

$itens = $prePedido['PDFPrePedido']['Itens'];

$html = "
<style>
body{
    font-family: Arial;
    font-size: 11px;
}

table{
    border-collapse: collapse;
    width: 100%;
}

th{
    background:#003366;
    color:#fff;
    padding:6px;
    border:1px solid #ccc;
}

td{
    padding:6px;
    border:1px solid #ccc;
    text-align:center;
}

h2{
    margin-bottom:15px;
}
</style>

<h2>Pré Pedido Nº {$PDFPrePedidoID}</h2>

<table>
<thead>
<tr>
    <th>Item Cliente</th>
    <th>Item BrSupply</th>
    <th>Tabela Preço</th>
    <th>Quantidade</th>
    <th>Valor Unit</th>
    <th>Total</th>
</tr>
</thead>
<tbody>
";

foreach ($itens as $item) {

    $html .= "<tr>";

    $html .= "<td>".$item['PDFPrePedidoItem']['ItemCliente']."</td>";
    $html .= "<td>".$item['PDFPrePedidoItem']['ItemBrSupply']."</td>";
    $html .= "<td>".$item['PDFPrePedidoItem']['VlrTblPrecoFormat']."</td>";
    $html .= "<td>".$item['PDFPrePedidoItem']['PDFQtde']."</td>";
    $html .= "<td>R$ " . number_format($item['PDFPrePedidoItem']['PDFVlrUnit'], 2, ',', '.') . "</td>";
    $html .= "<td>".$item['PDFPrePedidoItem']['VlrTotal']."</td>";

    $html .= "</tr>";
}

$html .= "
</tbody>
</table>
";

$mpdf = new mPDF();

$mpdf->WriteHTML($html);

$mpdf->Output(PATHROOT . 'files/tmp/' . $filename);

echo json_encode([
    'file' => $filename,
    'path' => 'files/tmp/'
]);