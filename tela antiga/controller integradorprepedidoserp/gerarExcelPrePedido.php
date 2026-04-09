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

$PDFPrePedidoID = $data['PDFPrePedidoID'];
$filename = 'PrePedido_' . $PDFPrePedidoID . '_' . date('YmdHis') . '.xlsx';

$prePedido = $IntegradorPedidosERPS->findByID($PDFPrePedidoID);

if (empty($prePedido['PDFPrePedido']['Itens'])) {
    die(json_encode(['erro' => 'Itens não encontrados']));
}

$itens = $prePedido['PDFPrePedido']['Itens'];
$objExcel = new PHPExcel();

$objExcel->getProperties()->setCreator("BR Supply")
                         ->setTitle("Pré-Pedido " . $PDFPrePedidoID)
                         ->setDescription("Itens do Pré-Pedido");

$objExcel->setActiveSheetIndex(0);
$sheet = $objExcel->getActiveSheet();
$sheet->setTitle('Pré-Pedido');

$header = ['Item Cliente', 'Item BrSupply', 'Tabela Preço', 'Quantidade', 'Valor Unit', 'Total'];
$col = 0;
foreach ($header as $h) {
    $cell = PHPExcel_Cell::stringFromColumnIndex($col) . '1';
    $sheet->setCellValue($cell, $h);
    $sheet->getStyle($cell)->getFont()->setBold(true)->getColor()->setRGB('FFFFFF');
    $sheet->getStyle($cell)->getFill()
          ->setFillType(PHPExcel_Style_Fill::FILL_SOLID)
          ->getStartColor()->setRGB('003366');
    $col++;
}

$row = 2;
foreach ($itens as $item) {
    $sheet->setCellValue('A' . $row, $item['PDFPrePedidoItem']['ItemCliente']);
    $sheet->setCellValue('B' . $row, $item['PDFPrePedidoItem']['ItemBrSupply']);
    $sheet->setCellValue('C' . $row, $item['PDFPrePedidoItem']['VlrTblPrecoFormat']);
    $sheet->setCellValue('D' . $row, $item['PDFPrePedidoItem']['PDFQtde']);
    $sheet->setCellValue('E' . $row, 'R$ ' . number_format($item['PDFPrePedidoItem']['PDFVlrUnit'], 2, ',', '.'));
    $sheet->setCellValue('F' . $row, $item['PDFPrePedidoItem']['VlrTotal']);
    $row++;
}

foreach (range(0, count($header)-1) as $i) {
    $sheet->getColumnDimension(PHPExcel_Cell::stringFromColumnIndex($i))->setAutoSize(true);
}

$objWriter = PHPExcel_IOFactory::createWriter($objExcel, 'Excel2007');
$objWriter->save(PATHROOT . 'files/tmp/' . $filename);

echo json_encode([
    'file' => $filename,
    'path' => 'files/tmp/'
]);