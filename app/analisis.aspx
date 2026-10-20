<%@ Page Title="Analisis de Actividades" Language="VB" MasterPageFile="~/masters/App.master" AutoEventWireup="false" 
    CodeFile="analisis.aspx.vb" Inherits="app_analisis" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" Runat="Server">

    <asp:ObjectDataSource ID="ods_productos" runat="server" SelectMethod="listarProductosVisibles" TypeName="Aplicacion.Logica">
    </asp:ObjectDataSource>
        
    <asp:ObjectDataSource ID="ods_paises" runat="server" SelectMethod="listarPaises" TypeName="Aplicacion.Logica">
    </asp:ObjectDataSource>
    
    <asp:ObjectDataSource ID="ods_usuarios" runat="server" SelectMethod="listarUsuarios" TypeName="Aplicacion.Logica">
    </asp:ObjectDataSource>
    
    <asp:ObjectDataSource ID="ods_centros" runat="server" SelectMethod="listarCentros" TypeName="Aplicacion.Logica">
    </asp:ObjectDataSource>

    <h1 class="tituloPagina">Análisis de Actividades</h1>
    <div class="panel">
        <div class="panel-body">
            <h5><i class="fa fa-filter"></i> Filtros</h5>
            <div class="row">
                <div class="col-md-4">
                    <p><strong>Desde:</strong> <cc:Datepicker runat="server" ID="tb_desde"></cc:Datepicker></p>
                </div>
                
                <div class="col-md-4">
                    <p><strong>Hasta:</strong> <cc:Datepicker runat="server" ID="tb_hasta"></cc:Datepicker></p>
                </div>

                <div class="col-md-4">
                    <p><br />
                        <asp:LinkButton runat="server" ID="lb_filtrar" CssClass="btn btn-primary btn-submit"><i class="fa fa-refresh"></i> Actualizar</asp:LinkButton> 
                        <asp:LinkButton runat="server" ID="lb_exportar" CssClass="btn btn-success"><i class="fa fa-file-excel-o"></i> Exportar</asp:LinkButton> 
                    </p>
                </div>
            </div>
        </div>

    </div>


    <asp:GridView ID="gvReporte" runat="server" GridLines ="None" CssClass="table footable" >

    </asp:GridView>
</asp:Content>

