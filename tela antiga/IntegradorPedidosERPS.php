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

class IntegradorPedidosERPS extends Model
{
    public function findByID($PDFPrePedidoID = null)
    {
        $Conn = new Conn();

        $sql = "SELECT 
                    pdfPP.PDFPrePedidoID AS PDFPrePedido__PDFPrePedidoID,
                    pdfAPP.Arquivo AS PDFPrePedido__Arquivo,
                    REPLACE(pdfAPP.Arquivo, '.PDF', '') AS PDFPrePedido__ArquivoFormat,
                    pdfPP.DataOrdemCompra AS PDFPrePedido__OrdemCompraDataHoraFormat,
                    PPC.ClienteUsuarioID AS PDFPrePedido__CadastroUsuarioID,
                    U.NmUsuario AS PDFPrePedido__CadastroNmUsuario,
                    pdfPP.StatusPrePedidoID AS PDFPrePedido__Status,
                    pdfPP.CotacaoID AS PDFPrePedido__CotacaoID,
                    pdfPP.OrdemCompra AS PDFPrePedido__OrdemCompra,
                    pdfPP.CNPJ AS PDFPrePedido__CNPJ,
                    pdfPP.ClienteLocalEntregaID AS PDFPrePedido__LocalEntregaID,
                    pdfPP.ClienteEnderecoID AS PDFPrePedido__ClienteEnderecoID,
                    (C.CdExtCliente + ' - ' + C.NmCliente) AS PDFPrePedido__Cliente,
                    E.NmEstabelecimento AS PDFPrePedido__Estabelecimento,
                    E.EstabelecimentoID AS PDFPrePedido__EstabelecimentoID,
                    (Ed.Logradouro) AS PDFPrePedido__Endereco,
                    (L.CdControle + ' - ' + L.NmLocalEntrega) AS PDFPrePedido__NmLocalEntrega,
                    endCp.NmCondPagto AS PDFPrePedido__CondPagto,
                    IIF(
                        ISNULL(locV.CanalVendaID, 0) > 0,
                        locV.NmCanalVenda,
                        V.NmCanalVenda
                    ) AS PDFPrePedido__CanalVenda,
                    C.tipoOVSAP AS PDFPrePedido__TipoOVSAP,
                    T.NmTblPreco AS PDFPrePedido__TabelaPreco,
                    PPS.Descricao AS PDFPrePedido__StatusDescricao,
                    C.CdExtCliente AS PDFPrePedido__CdExtCliente,
                    C.ClienteID AS PDFPrePedido__ClienteID,
                    pdfPP.TblPrecoID AS PDFPrePedido__TblPrecoID,
                    C.LogoCliente AS PDFPrePedido__LogoCliente,
                    C.NmCliente AS PDFPrePedido__NmCliente,
                    C.VlrMinimoBloqueioPedido
                FROM Integracao_Clientes.dbo.PDF_PrePedido pdfPP (NOLOCK)
                INNER JOIN Integracao_Clientes.dbo.PDF_ArquivoPrePedido pdfAPP (NOLOCK) ON pdfAPP.PDFArquivoPrePedidoID = pdfPP.ArquivoPrePedidoId
                INNER JOIN Integracao_Clientes.dbo.PPedido_ProcessadorPedidoConfiguracao PPC (NOLOCK) ON PPC.ClienteID = pdfPP.ClienteID
                LEFT JOIN Integracao_Clientes.dbo.PPedido_StatusPrePedido PPS (NOLOCK) ON PPS.StatusPrePedidoID = pdfPP.StatusPrePedidoID
                LEFT JOIN BrSupply.dbo.BR_Usuario U (NOLOCK) ON U.UsuarioID = PPC.ClienteUsuarioID
                LEFT JOIN BrSupply.dbo.BR_Cliente C (NOLOCK) ON C.ClienteID = pdfPP.ClienteID
                LEFT JOIN BrSupply.dbo.BR_Estabelecimento E (NOLOCK) ON E.EstabelecimentoID = C.EstabelecimentoID
                LEFT JOIN BrSupply.dbo.BR_ClienteLocalEntrega L (NOLOCK) ON L.ClienteLocalEntregaID = pdfPP.ClienteLocalEntregaID
                LEFT JOIN BrSupply.dbo.BR_ClienteEndereco Ed (NOLOCK) ON Ed.ClienteEnderecoID = pdfPP.ClienteEnderecoID
                LEFT JOIN BrSupply.dbo.BR_CondPagto endCp (NOLOCK) ON endCp.CondPagtoID = Ed.CondPagtoID
                LEFT JOIN BrSupply.dbo.BR_CanalVenda locV (NOLOCK) ON locV.CanalVendaID = L.CanalVendaID
                LEFT JOIN BrSupply.dbo.BR_CanalVenda V (NOLOCK) ON V.CanalVendaID = C.CanalVendaID
                LEFT JOIN BrSupply.dbo.BR_TblPreco T (NOLOCK) ON T.TblPrecoID = Ed.TblPrecoID
                WHERE pdfPP.PDFPrePedidoID = $PDFPrePedidoID
                ORDER BY pdfPP.PDFPrePedidoID DESC";

        $result = $Conn->query($sql);

        if (count($result) > 0) {
            $result[0]['PDFPrePedido']['Itens'] = $this->getItens($PDFPrePedidoID);

            return $result[0];
        }
        return null;
    }

    public function getItens($PDFPrePedidoID = 0)
    {
        $Conn = new Conn();

        $sql = "SELECT 
                    pdfPI.PDFPrePedidoItemID AS PDFPrePedidoItem__PDFPrePedidoItemID,
                    pdfPI.PDFPrePedidoID AS PDFPrePedidoItem__PDFPrePedidoID,
                    pdfPI.Sequencia AS PDFPrePedidoItem__PDFSeqItem,
                    CONVERT(INT, ROUND(pdfPI.Quantidade, 0)) AS PDFPrePedidoItem__PDFQtde,
                    pdfPI.CdItemCliente + ' - ' + pdfPI.Descricao AS PDFPrePedidoItem__ItemCliente,
                    pdfPI.Descricao AS PDFPrePedidoItem__Descricao,
                    I.CdItem AS PDFPrePedidoItem__ItemID,
                    (
                        I.CdItem
                        + ' - ' + I.NmItem
                    ) AS PDFPrePedidoItem__ItemBrSupply,
                    I.SegmentoID,
                    I.FamiliaID,
                    FORMAT(TI.VlrUnit, 'C', 'pt-br') AS PDFPrePedidoItem__VlrTblPrecoFormat,
                    REPLACE(pdfPI.ValorUnitario, '.', ',') AS PDFPrePedidoItem__PDFVlrUnit,
                    FORMAT((
                        pdfPI.Quantidade * pdfPI.ValorUnitario
                    ), 'C', 'pt-br') AS PDFPrePedidoItem__VlrTotal,
                    FORMAT(
                        SUM(pdfPI.Quantidade * pdfPI.ValorUnitario) 
                        OVER (PARTITION BY pdfPI.PDFPrePedidoID),
                        'C', 'pt-br'
                    ) AS PDFPrePedidoItem__VlrTotalPedido
                FROM Integracao_Clientes.dbo.PDF_PrePedidoItem pdfPI (NOLOCK)
                LEFT JOIN BrSupply.dbo.BR_Item I (NOLOCK) ON I.ItemID = pdfPI.ItemID
                LEFT JOIN Integracao_Clientes.dbo.PDF_PrePedido pdfPP (NOLOCK) ON pdfPP.PDFPrePedidoID = pdfPI.PDFPrePedidoID
                LEFT JOIN BrSupply.dbo.BR_Cliente C (NOLOCK) ON C.ClienteID = pdfPP.ClienteID
                LEFT JOIN BrSupply.dbo.BR_ClienteEndereco E (NOLOCK) ON E.ClienteEnderecoID = pdfPP.ClienteEnderecoID
                LEFT JOIN BrSupply.dbo.BR_TblPreco T (NOLOCK) ON T.TblPrecoID = E.TblPrecoID
                LEFT JOIN BrSupply.dbo.BR_TblPrecoVig V (NOLOCK) ON V.TblPrecoID = T.TblPrecoID
                LEFT JOIN BrSupply.dbo.BR_TblPrecoItem TI (NOLOCK) ON TI.TblPrecoVigID = V.TblPrecoVigID AND TI.ItemID = I.ItemID
                LEFT JOIN BrSupply.dbo.BR_PrecoEstoque PE (NOLOCK) ON PE.EstabelecimentoID = C.EstabelecimentoID AND PE.ItemID = I.ItemID
                WHERE pdfPI.PDFPrePedidoID = $PDFPrePedidoID
                ORDER BY pdfPI.Sequencia ASC";

        $result = $Conn->query($sql);

        return $result;
    }

    public function getStatusPrePedido()
    {
        $arrayStatus = array();

        $arrayStatus[0] = array('StatusPrePedidoId' => 1,
            'Descricao' => 'Aguardando');
        
        $arrayStatus[1] = array('StatusPrePedidoId' => 4,
            'Descricao' => 'Aceito');

        $arrayStatus[2] = array('StatusPrePedidoId' => 5,
            'Descricao' => 'Recusado');

        $arrayStatus[3] = array('StatusPrePedidoId' => 6,
            'Descricao' => 'Erro');

        return $arrayStatus;
    }

    public function GetConteudoArquivoPedido($params = array())
    {
        $url = "https://punchout.brsupply.com.br/storage/processadorERP/". $params['CdExtCliente']."/". $params['OrdemCompra'] .".json";

        $ch = curl_init($url);

        curl_setopt($ch, CURLOPT_GET, true);

        curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);

        curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);

        $output = curl_exec($ch);

        $jsonDecoded = json_decode($output, true);
        $jsonFormatado = json_encode($jsonDecoded, JSON_PRETTY_PRINT | JSON_UNESCAPED_UNICODE);
        
        return $jsonFormatado;
    }

    public function ReprocessarPedido($jsonPedido = '')
    {
        $url ="https://api.brsupply.com.br/v1/homologacao/IntegradorERP/Pedido";

        $ch = curl_init($url);
        curl_setopt($ch, CURLOPT_POST, true);
        curl_setopt($ch, CURLOPT_POSTFIELDS, $jsonPedido);
        curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
        curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);

        curl_setopt($ch, CURLOPT_HTTPHEADER, [
            'Content-Type: application/json',
            'Content-Length: ' . strlen($jsonPedido)
        ]);
        $output = curl_exec($ch);

        $jsonDecoded = json_decode($output, true);

        return $jsonDecoded;
        
    }

    public function GetEnderecos($params = array())
    {
        $Conn = new Conn();

        $sql = "SELECT ClienteEnderecoID, Logradouro FROM BrSupply.dbo.BR_ClienteEndereco WHERE ClienteID = {$params['ClienteID']}
                AND FlagAtivo = 1";

        $result = $Conn->query($sql);

        return $result;
    }

    public function GetLocaisEntrega($params = array())
    {
        $Conn = new Conn();

        $sql = "SELECT ClienteLocalEntregaID, NmLocalEntrega, CdControle FROM BrSupply.dbo.BR_ClienteLocalEntrega 
                WHERE ClienteEnderecoID = {$params['ClienteEnderecoID']} AND FlagAtivo = 1 ";

        $result = $Conn->query($sql);

        return $result;
    }

    public function GetListCNPJCliente($params = array())
    {
        $Conn = new Conn();

        $sql = "SELECT ClienteEnderecoID, CPFCNPJ FROM BrSupply.dbo.BR_ClienteEndereco WHERE ClienteID = {$params['ClienteID']} AND FlagAtivo = 1";

        $result = $Conn->query($sql);
        
        return $result;
    }

    public function getLogsErro($params = array())
    {
        $Conn = new Conn();

        $sql = "SELECT COUNT(*) AS Registros FROM Integracao_Clientes.dbo.PDF_PrePedidoLog WHERE PDFPrePedidoID = {$params['PDFPrePedidoID']} AND TIPO = 'Erro' ";

        $result = $Conn->query($sql);
        
        return $result;
    }

    public function getLogs($params = array())
    {
        $Conn = new Conn();

        $sql = "SELECT Mensagem,
                CONVERT(VARCHAR(10), CriadoEm, 103) + ' ' + CONVERT(VARCHAR(8), CriadoEm, 108) AS CriadoEmFormatado
                FROM Integracao_Clientes.dbo.PDF_PrePedidoLog WHERE PDFPrePedidoID = {$params['PDFPrePedidoID']}";

        $result = $Conn->query($sql);

        return $result;
    }

    public function BuscarCatalogo($params)
    {
        $Conn = new Conn();

        $sql = "SET NOCOUNT ON
                DECLARE @Tbl TABLE 
                (
                    ItemID INT, 
                    FlagTipo INT, 
                    Prioridade INT, 
                    Probabilidade INT, 
                    CdItem VARCHAR(100), 
                    NmItem VARCHAR(1000),
                    NmFornecedor VARCHAR(100),
                    ProdutoMarcaID INT,
                    Marca VARCHAR(1000),
                    Premium INT,
                    Standard INT,
                    Basic INT,
                    VlrTabela DECIMAL (18,2)
                )
                INSERT @Tbl 
                EXEC BrSupply.dbo.BRS_sp_PesquisaCatalogo_V2 '{$params['Descricao']}', 0, {$params['ClienteID']}, {$params['TblPrecoID']}, 0, 1, 0, 1, 200, 1, 0
                SELECT 
                    T.ItemID AS Produto__ItemID
                    , I.CdItem AS Produto__CdItem
                    , I.NmItem AS Produto__NmItem
                    , S.SegmentoID AS Produto__SegmentoID
                    , S.NmSegmento AS Produto__NmSegmento
                    , F.FamiliaID AS Produto__FamiliaID
                    , F.NmFamilia AS Produto__NmFamilia
                    , SF.SubFamiliaID AS Produto__SubFamiliaID
                    , SF.NmSubFamilia AS Produto__NmSubFamilia
                    , PE.EstabelecimentoID AS Produto__EstabelecimentoID
                    , ISNULL(PE.Curva,'') AS Produto__Curva
                    , CAST((ISNULL(PE.QtDispEstoque,0) - ISNULL(PE.QtAlocadaSemOV,0)) AS INT) AS Produto__QtdDisponivel
                    , CONVERT(INT,(ISNULL(PE.QtDispEstoque,0) - ISNULL(PE.QtAlocadaSemOV,0))) AS PropostaItem__QtEstoqueSIC
                    , IIF(ISNULL(I.FlagAtivo,0) = 1, 'SIM', 'NÃO') AS Produto__Ativo
                    , FORMAT(ISNULL(PE.VlrCustoAquisicao,0),'N','pt-br') AS Produto__VlrCustoAquisicao
                    , FORMAT(ISNULL(PE.VlrCustoMedio,0),'N','pt-br') AS Produto__VlrCustoMedio
                    , COALESCE(
                        NULLIF(T.VlrTabela, 0),
                        NULLIF(PE.VlrCustoAquisicao, 0),
                        PE.VlrCustoMedio,
                        0
                    ) AS Produto__VlrTabela
                    , CASE 
                        WHEN ISNULL(PE.FlagOutlet, 0) = 1 THEN 'Y'
                        ELSE CASE
                            WHEN ISNULL(I.FlagSobDemanda, 0) = 1 THEN 'Z'
                            ELSE 'X'
                        END
                    END AS Produto__Criticidade
                    , FORMAT((
                        ISNULL((
                            T.VlrTabela
                        ), 0)
                    ), 'N', 'pt-br') AS Produto__TabelaPreco
					, DP.ItemCli1 AS Produto__ItemDePara
                FROM @Tbl T
                INNER JOIN BrSupply.dbo.BR_Item I (NOLOCK) ON I.ItemID = T.ItemID
                INNER JOIN BrSupply.dbo.BR_Segmento S (NOLOCK) ON S.SegmentoID = I.SegmentoID
                INNER JOIN BrSupply.dbo.BR_Familia F (NOLOCK) ON F.FamiliaID = I.FamiliaID
                INNER JOIN BrSupply.dbo.BR_SubFamilia SF (NOLOCK) ON SF.SubFamiliaID = I.SubFamiliaID
                INNER JOIN BrSupply.dbo.BR_PrecoEstoque PE (NOLOCK) ON PE.ItemID = I.ItemID
                LEFT JOIN BrSupply.dbo.BR_ProdutoMarca M (NOLOCK) ON M.ProdutoMarcaID = I.ProdutoMarcaID
                INNER JOIN Integracao_Clientes.dbo.BR_Itens_DePara DP ON DP.ItemBR = I.CdItem AND DP.ClienteID = {$params['ClienteID']}
                WHERE 1=1
                    AND PE.EstabelecimentoID = {$params['EstabelecimentoID']}
					AND T.VlrTabela <> 0
                ORDER BY
                    T.Probabilidade DESC
                    , ISNULL(S.FlagConsultaProduto,99) ASC
                    , I.FlagAtivo DESC
                    , ISNULL(M.FlagTipoMarca,'zzz') ASC
                    , ISNULL(PE.FlagOutlet, 0) ASC";

        $result = $Conn->query($sql);

        return $result;
    }

    public function AtualizarEndereco($params = array())
    {
        $Conn = new Conn();

        $sql = "UPDATE Integracao_Clientes.dbo.PDF_PrePedido SET ClienteEnderecoID = {$params['ClienteEnderecoID']} WHERE PDFPrePedidoID = {$params['PDFPrePedidoID']}
        
                INSERT INTO Integracao_Clientes..PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo)
                VALUES ('Endereço atualizado para: {$params['ClienteEnderecoID']} - {$params['Logradouro']}', GETDATE(), {$params['PDFPrePedidoID']}, 'Atualização')";

        $result = $Conn->query($sql);

        return $result;
    }

    public function AtualizarLocalEntrega($params = array())
    {
        $Conn = new Conn();

        $sql = "UPDATE Integracao_Clientes.dbo.PDF_PrePedido SET ClienteLocalEntregaID = {$params['ClienteLocalEntregaID']} WHERE PDFPrePedidoID = {$params['PDFPrePedidoID']}
        
                INSERT INTO Integracao_Clientes..PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo)
                VALUES ('Local de entrega atualizado para: {$params['ClienteLocalEntregaID']} - {$params['NmLocalEntrega']}', GETDATE(), {$params['PDFPrePedidoID']}, 'Atualização')";

        $result = $Conn->query($sql);

        return $result;
    }

    public function AtualizarCNPJ($params = array())
    {
        $Conn = new Conn();

        $sql = "UPDATE Integracao_Clientes.dbo.PDF_PrePedido SET CNPJ = {$params['CNPJ']} WHERE PDFPrePedidoID = {$params['PDFPrePedidoID']}
        
                INSERT INTO Integracao_Clientes..PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo)
                VALUES ('CNPJ atualizado para: {$params['CNPJ']}', GETDATE(), {$params['PDFPrePedidoID']}, 'Atualização')";

        $result = $Conn->query($sql);

        return $result;
    }

    public function AdicionarItens($params = array())
    {
        $Conn = new Conn();

        $sqlSequencia = "SELECT TOP 1 ISNULL(ITEM.Sequencia, 0) AS Sequencia
                        FROM Integracao_Clientes.dbo.PDF_PrePedidoItem AS ITEM
                        WHERE ITEM.PDFPrePedidoID = {$params['PDFPrePedidoID']}";

        $resultSequencia = $Conn->query($sqlSequencia);

        $params['Sequencia'] = $resultSequencia[0]['Sequencia'] + 1;
        
        $sql = "INSERT INTO Integracao_Clientes.dbo.PDF_PrePedidoItem(PDFPrePedidoID, CdItem, CdItemCliente, ItemID, Descricao, Quantidade, Sequencia, OrdemCliente, ValorUnitario, TblPrecoValorUnitario)
                VALUES({$params['PDFPrePedidoID']}, '{$params['CodItemBR']}', '{$params['ItemDePara']}', '{$params['ItemID']}', '{$params['DescrItemBR']}', {$params['Quantidade']}, {$params['Sequencia']},
                 '{$params['OrdemCompra']}', {$params['PrecoTbl']}, {$params['PrecoTbl']})
                 
                INSERT INTO Integracao_Clientes..PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo) 
                VALUES ('Item adicionado: {$params['CodItemBR']} - {$params['DescrItemBR']}', GETDATE(), {$params['PDFPrePedidoID']}, '')";

        $result = $Conn->query($sql);

        return $result;
    }

    public function ExcluirItem($params = array())
    {
        $Conn = new Conn();

        $PDFPrePedidoItemId = $params['PDFPrePedidoItemId'];
        $PDFPrePedidoID = $params['PDFPrePedidoID'];

        $sqlSelect = "SELECT PDF_PrePedidoItem.*, I.NmItem AS NmItem FROM Integracao_Clientes.dbo.PDF_PrePedidoItem 
                    LEFT JOIN BrSupply.dbo.BR_Item I (NOLOCK) ON I.ItemID = PDF_PrePedidoItem.ItemID
                    WHERE PDFPrePedidoItemId = {$PDFPrePedidoItemId} AND PDFPrePedidoId = {$PDFPrePedidoID}";

        $result = $Conn->query($sqlSelect);

        $sqlDelete="DELETE Integracao_Clientes.dbo.PDF_PrePedidoItem WHERE PDFPrePedidoItemId = {$PDFPrePedidoItemId} AND PDFPrePedidoId = {$PDFPrePedidoID}
        
                    INSERT INTO Integracao_Clientes..PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo) 
                    VALUES ('Item excluído: {$params['Descricao']}', GETDATE(), {$params['PDFPrePedidoID']}, 'Exclusão')";
        
        $Conn->query($sqlDelete);

        return $sql[0];
    }

    public function InsertLog($params = array())
    {
        $Conn = new Conn();

        $sql = "INSERT INTO Integracao_Clientes.dbo.PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo)
                VALUES ('{$params['Mensagem']}', GETDATE(), {$params['PDFPrePedidoID']}, 'Aviso')";

        $result = $Conn->query($sql);

        return $result;

    }

    public function UpdateQuantidade($params = array())
    {
        $Conn = new Conn();

        $sql = "UPDATE Integracao_Clientes.dbo.PDF_PrePedidoItem SET Quantidade = {$params['Quantidade']} WHERE PDFPrePedidoItemID = {$params['PDFPrePedidoItemID']}

                INSERT INTO Integracao_Clientes..PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo) 
                VALUES ('Quantidade atualizada para o item: {$params['Descricao']} - Nova Quantidade: {$params['Quantidade']}', GETDATE(), {$params['PDFPrePedidoID']}, 'Atualização')";

        $result = $Conn->query($sql);

        return $result;
    }

    public function CancelarPrePedido($PDFPrePedidoID = 0)
    {
        $Conn = new Conn();

        $sql = "UPDATE Integracao_Clientes.dbo.PDF_PrePedido SET StatusPrePedidoID = 5 WHERE PDFPrePedidoID = {$PDFPrePedidoID}
        
                INSERT INTO Integracao_Clientes.dbo.PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo)
                VALUES ('Pre-pedido cancelado!', GETDATE(), {$PDFPrePedidoID}, 'Aviso')
        ";

        $result = $Conn->query($sql);

        return $result;
    }

    public function SetProcessadorPraZero($PDFPrePedidoID = 0)
    {
        $Conn = new Conn();

        $sql = "DECLARE @ArquivoPrePedidoId INT

                SELECT @ArquivoPrePedidoId = ArquivoPrePedidoId FROM Integracao_Clientes.dbo.PDF_PrePedido WHERE PDFPrePedidoID = {$PDFPrePedidoID}

                UPDATE Integracao_Clientes.dbo.PDF_ArquivoPrePedido SET Processado = 0 WHERE PDFArquivoPrePedidoID = @ArquivoPrePedidoId
            
                SELECT * FROM Integracao_Clientes.dbo.PDF_ArquivoPrePedido WHERE PDFArquivoPrePedidoID = @ArquivoPrePedidoId";

        $result = $Conn->query($sql);

        return $result;
    }

    function GetIntegradorPedidosItensQuantidadeZero($PDFPrePedidoID = 0)
    {
        $Conn = new Conn();

        $sql = "SELECT COUNT(*) AS ItensQuantZero FROM Integracao_Clientes.dbo.PDF_PrePedidoItem WHERE
                PDFPrePedidoID = {$PDFPrePedidoID}
                AND (Quantidade < 1 OR ValorUnitario = 0)";

        $result = $Conn->query($sql);
    }

    public function ValidarParaAceite($PDFPrePedidoID = 0)
    {
        $Conn = new Conn();

        $sql = "SELECT CNPJ, ClienteEnderecoID, ClienteLocalEntregaID, CotacaoID
                FROM Integracao_Clientes.dbo.PDF_PrePedido where PDFPrePedidoID = $PDFPrePedidoID;";

        $result = $Conn->query($sql);

        $Campos = $result[0];

        if ($Campos['CNPJ'] == null) {
            return "CNPJ nulo";
        }

        if ($Campos['ClienteEnderecoID'] == null) {
            return "Endereço nulo";
        }

        if ($Campos['ClienteLocalEntregaID'] == null) {
            return "Local de entrega nulo";
        }

        if ($Campos['CotacaoID'] != null) {
            return "Cotação já gerada";
        }

        return "pode aceitar";
    }
    public function GetInfoItensGerarPedido($PDFPrePedidoID = 0)
    {
        $Conn = new Conn();

        $sql = "SELECT PEDIDO.CotacaoID as CotacaoID,
                    1 AS Tipo,
                    ITEM.ItemID AS ItemID, 
                    ITEM.Quantidade AS QtItem, 
                    ITEM.ValorUnitario AS VlrUnit, 
                    ITEM.CdItemCliente AS CdItemCliente, 
                    ITEM.OrdemCliente AS OrdemCliente, 
                    ITEM.Sequencia AS SeqCliente 
                FROM Integracao_Clientes.dbo.PDF_PrePedidoItem AS ITEM
                LEFT JOIN Integracao_Clientes.dbo.PDF_PrePedido AS PEDIDO ON PEDIDO.PDFPrePedidoID = ITEM.PDFPrePedidoID
                WHERE ITEM.PDFPrePedidoID = $PDFPrePedidoID";

        $result = $Conn->query($sql);

        return $result;
        
    }

    public function GetInfoGerarPedido($PDFPrePedidoID = 0)
    {
        $Conn = new Conn();

        $sql = "SELECT PDF.EstabelecimentoID,
                    PDF.ClienteID,
                    PDF.ClienteEnderecoID,
                    PDF.CNPJ, 
                    PDF.ClienteLocalEntregaID,
                    PPC.ClienteUsuarioID,
                    ISNULL(PDF.NaturezaOperacaoID, 1) AS NaturezaOperacaoID,
                    PDF.CondPagtoID, 
                    PDF.OrdemCompra,
                    PDF.ClienteCategoriaPedidoID
                FROM Integracao_Clientes.dbo.PDF_PrePedido PDF
                LEFT JOIN  Integracao_Clientes.dbo.PPedido_ProcessadorPedidoConfiguracao PPC ON PPC.ClienteID = PDF.ClienteID
                WHERE PDF.PDFPrePedidoID = $PDFPrePedidoID";

        $result = $Conn->query($sql);
                
        if (count($result) > 0) {
            return $result[0];
        }
    }

    public function GerarPedido($params = array())
    {
        $Conn = new Conn();

        $PDFPrePedidoID = $params['PDFPrePedidoID'];

        $PDFPrePedido = $this->GetInfoGerarPedido($PDFPrePedidoID);

        $clienteCategoriaPedidoID = isset($PDFPrePedido['ClienteCategoriaPedidoID']) && $PDFPrePedido['ClienteCategoriaPedidoID'] !== null
            ? $PDFPrePedido['ClienteCategoriaPedidoID']
            : 'NULL';

        $sql = "SET NOCOUNT ON
        EXEC BrSupply.dbo.BR_sp_InsertCotacao {$PDFPrePedido['EstabelecimentoID']},
        {$PDFPrePedido['ClienteID']},
        {$PDFPrePedido['ClienteEnderecoID']},
        '{$PDFPrePedido['CNPJ']}',
        {$PDFPrePedido['ClienteLocalEntregaID']},
        {$PDFPrePedido['ClienteUsuarioID']},
        {$PDFPrePedido['NaturezaOperacaoID']},
        {$PDFPrePedido['CondPagtoID']}, 
        '{$PDFPrePedido['OrdemCompra']}',
        $clienteCategoriaPedidoID
        ";

        $result = $Conn->query($sql);

        if ($result[0]['ID'] > 0){
            $sqlUpdate = "UPDATE Integracao_Clientes.dbo.PDF_PrePedido SET CotacaoID = {$result[0]['ID']}, StatusPrePedidoID = 4 WHERE PDFPrePedidoID = {$PDFPrePedidoID}";

            $Conn->query($sqlUpdate);

            $returrnnn =  $this->GerarItensPedidoBrSupply(array(
                'PDFPrePedidoID' => $PDFPrePedidoID
            ));

            return $result[0];
        }

        return $result[0];
    }

    public function GerarItensPedidoBrSupply($params = array())
    {
        $Conn = new Conn();

        $PDFPrePedidoID = $params['PDFPrePedidoID'];

        $PDFPrePedidoItens = $this->GetInfoItensGerarPedido($PDFPrePedidoID);

        $result = null;
        foreach ($PDFPrePedidoItens as $PDFPrePedidoItem) {
            $sql = "SET NOCOUNT ON
            EXEC BrSupply.dbo.BR_sp_InsertCotacaoItem {$PDFPrePedidoItem['CotacaoID']},
            {$PDFPrePedidoItem['Tipo']},
            {$PDFPrePedidoItem['ItemID']},
            {$PDFPrePedidoItem['QtItem']},
            {$PDFPrePedidoItem['VlrUnit']},
            '{$PDFPrePedidoItem['CdItemCliente']}',
            '{$PDFPrePedidoItem['OrdemCliente']}',
            {$PDFPrePedidoItem['SeqCliente']}";

            $result = $Conn->query($sql);
        }

        return $result;
    }

    public function TrocarItem($params = array())
    {
        $Conn = new Conn();

        $TblPrecoID = $params['TblPrecoID'];
        $estabelecimentoID = $params['EstabelecimentoID'];
        $SegmentoID = $params['SegmentoID'];
        $FamiliaID  = $params['FamiliaID'];
        $ItemID = $params['ItemID'];

        $sql = "SELECT I.CdItem,
                    I.NmItem AS NmItem,
                    I.ItemID,
                        CONVERT(DECIMAL(10,2),(SELECT TPI.VlrUnit
                            FROM BrSupply.dbo.BR_TblPrecoItem TPI (NOLOCK)
                            JOIN BrSupply.dbo.BR_TblPrecoVig TPV (NOLOCK) ON TPV.TblPrecoVigID = TPI.TblPrecoVigID
                            WHERE TPI.ItemID = I.ItemID
                                AND TPV.TblPrecoID = {$TblPrecoID}
                                )) AS VlrTabelaPreco
                FROM BrSupply.dbo.BR_Item I WITH(NOLOCK)
                    LEFT JOIN BrSupply.dbo.BR_PrecoEstoque E (NOLOCK) ON E.ItemID = I.ItemID AND E.EstabelecimentoID = {$estabelecimentoID}
                WHERE ISNULL(I.FlagAtivo,0) = 1
                    AND I.SegmentoID = {$SegmentoID}
                    AND I.FamiliaID = {$FamiliaID}
                    AND I.ItemID <> '{$ItemID}'
                    AND EXISTS (SELECT TPI.VlrUnit
                            FROM BrSupply.dbo.BR_TblPrecoItem TPI (NOLOCK)
                            JOIN BrSupply.dbo.BR_TblPrecoVig TPV (NOLOCK) ON TPV.TblPrecoVigID = TPI.TblPrecoVigID
                            WHERE TPI.ItemID = I.ItemID
                                AND TPV.TblPrecoID = {$TblPrecoID})
                ORDER BY I.NmItem";


        $result = $Conn->query($sql);

        return $result;
    }

    public function GravarTrocaItem($params = array())
    {
        $Conn = new Conn();

        $sql = "UPDATE Integracao_Clientes..PDF_PrePedidoItem 
                SET Cditem = {$params['CdItem']}, ItemID = '{$params['ItemID']}', Descricao = '{$params['NmItem']}', TblPrecoValorUnitario = {$params['VlrTabelaPreco']}
                WHERE PDFPrePedidoItemId = {$params['PDFPrePedidoItemId']}
                
                INSERT INTO Integracao_Clientes..PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo)
                VALUES ('Item Substituído - DE: {$params['CdItemAntigo']} - {$params['DescricaoAntiga']} - {$params['ValorAntigo']} | 
                        PARA: {$params['CdItem']} - {$params['NmItem']} - {$params['VlrTabelaPreco']}', GETDATE(), {$params['PDFPrePedidoID']}, '{$params['MotivoTrocaItem']}')";

        $result = $Conn->query($sql);

        return $result;
    }

    public function getList($data = array())
    {
        $filtro = '';

        if (isset($data['Status']) && $data['Status'] != 0) {
            $filtro = " AND PP.StatusPrePedidoID = {$data['Status']}";
        }

        if (isset($data['CdExtCliente']) && $data['CdExtCliente'] != 0) {
            $filtro .= " AND C.CdExtCliente = {$data['CdExtCliente']}";
        }

        if (isset($data['DataInicial']) && !empty($data['DataInicial'])) {
            $dataInicial = date('Y-m-d', strtotime($data['DataInicial']));
        }

        if (isset($data['DataFinal']) && !empty($data['DataFinal'])) {
            $dataFinal = date('Y-m-d', strtotime($data['DataFinal']));
        }

        if (isset($dataInicial) && isset($dataFinal)) {
            $filtro .= " AND PP.CriadoEm BETWEEN '{$dataInicial} 00:00:00' AND '{$dataFinal} 23:59:59'";
        } elseif (isset($dataInicial)) {
            $filtro .= " AND PP.CriadoEm >= '{$dataInicial} 00:00:00'";
        } elseif (isset($dataFinal)) {
            $filtro .= " AND PP.CriadoEm <= '{$dataFinal} 23:59:59'";
        }

        if (isset($data['FiltroCarteira']) && $data['FiltroCarteira'] == true) {
            $filtro .= " AND (
                (ISNULL(Carteira.BackOfficeID, 0) = {$this->UsuarioID}) OR (ISNULL(Carteira.AtendenteIntID, 0) = {$this->UsuarioID}) OR
                (ISNULL(Carteira.AtendenteExtID, 0) = {$this->UsuarioID}) OR (ISNULL(Carteira.RepresentanteID, 0) = {$this->UsuarioID}) OR
                (ISNULL(Carteira.ExecVendasID, 0) = {$this->UsuarioID}) OR (ISNULL(C.BackOfficeID, 0) = {$this->UsuarioID}) OR
                (ISNULL(C.AtendenteIntID, 0) = {$this->UsuarioID}) OR (ISNULL(C.AtendenteExtID, 0) = {$this->UsuarioID}) OR
                (ISNULL(C.RepresentanteID, 0) = {$this->UsuarioID}) OR (ISNULL(C.ExecVendasID, 0) = {$this->UsuarioID})
            )";
        }


        $Conn = new Conn();

        $sql = "SELECT PP.PDFPrePedidoID,
                AP.ClienteID,
                AP.OrdemCompra,
                PP.StatusPrePedidoID,
                PP.CotacaoID,
                AP.Arquivo,
                CONCAT(C.CdExtCliente, ' - ', C.NmCliente) AS NmCliente,
                PP.CNPJ,
                PP.CotacaoID,
                PP.StatusPrePedidoID as Status,
                PPS.Descricao as StatusDescricao,
                CONVERT(VARCHAR(10), PP.CriadoEm, 103) + ' ' + CONVERT(VARCHAR(8), PP.CriadoEm, 108) AS CriadoEm
            FROM Integracao_Clientes.dbo.PDF_PrePedido PP
            JOIN Integracao_Clientes.dbo.PDF_ArquivoPrePedido AP ON PP.ArquivoPrePedidoID = AP.PDFArquivoPrePedidoID
            LEFT JOIN Integracao_Clientes.dbo.PPedido_StatusPrePedido PPS ON PPS.StatusPrePedidoID = PP.StatusPrePedidoID
            JOIN BrSupply.dbo.BR_Cliente C ON C.ClienteID = AP.ClienteID
            LEFT JOIN BrSupply.dbo.BR_Carteira Carteira (NOLOCK) ON Carteira.CarteiraID = C.CarteiraID
            WHERE 1 = 1 {$filtro}";

        $result = $Conn->query($sql);

        return $result;
    }
}