<%@ Page Title="Sistema de carga de horas" Language="VB" MasterPageFile="~/masters/App.master" AutoEventWireup="false"
    CodeFile="cargar_horas.aspx.vb" Inherits="app_cargar_horas" Trace="false" %>

<%@ Register TagPrefix="cc" TagName="Tarea" Src="~/Controles/Tarea.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" runat="Server">

    <h1 class="tituloPagina">Sistema de carga de horas</h1>

    <asp:Panel runat="server"  ID="pnl_controles" CssClass="row">
        <div class="col-md-4">
            <asp:DropDownList ID="ddl_usuarios" runat="server" CssClass="form-control chosen" AutoPostBack="true">
            </asp:DropDownList>
        </div>
        <div class="col-md-4">
        </div>
        <div class="col-md-4 text-right">
            <button type="button" class="btn btn-default tooltips" data-toggle="modal" data-target="#calendario_precarga" title="Precarga rápida de actividades">
                <i class="fa fa-flash"></i>
            </button>
        </div>
    </asp:Panel>


    <asp:Panel runat="server" ID="pnl_cargas" CssClass="panel panel-default">
        <div class="panel-body">
            <div class="row cabecera-tareas hidden-sm hidden-xs">
                <div class="col-sm-2">
                    <b>Fecha</b>
                </div>

                <div class="col-sm-3">
                    <b>Actividad</b> <a class="tooltips" title="Comenzar a tipear y seleccionarla de la lista" href="javascript:;"><span class="badge"><i class="fa fa-question"></i></span></a>
                </div>

                <div class="col-sm-2">
                    <b>Producto</b> <a class="tooltips" title="Comenzar a tipear y seleccionarlo de la lista" href="javascript:;"><span class="badge"><i class="fa fa-question"></i></span></a>
                </div>

                <div class="col-sm-1">
                    <b>País</b>
                </div>

                <div class="col-sm-2">
                    <b>Horas/minutos</b>
                </div>
                <div class="col-sm-2">
                </div>

            </div>
            <!-- /.cabecera-actividad -->

            <cc:Tarea runat="server" ID="Tarea1" />
            <cc:Tarea runat="server" ID="Tarea2" />
            <cc:Tarea runat="server" ID="Tarea3" />
            <cc:Tarea runat="server" ID="Tarea4" />
            <cc:Tarea runat="server" ID="Tarea5" />

            <asp:LinkButton ID="lb_agregarFila" runat="server" CssClass="btn btn-success tooltips" ToolTip="Borrar datos y agregar fila" Visible="false"><i class="fa fa-plus"></i> Agregar fila</asp:LinkButton>
            <hr />
            <p class="text-center">
                <cc:SubmitButton ID="lb_cargar" runat="server" CssClass="btn btn-success">
                    <i class="glyphicon glyphicon-ok"></i> Cargar datos</cc:SubmitButton>
            </p>
        </div>
        <!-- /.panel-body -->
    </asp:Panel>
    <br /><br />
    <p>
        <asp:Literal ID="l_result" runat="server" EnableViewState="false"></asp:Literal>
    </p>

    <!-- Modal -->
    <div class="modal fade" id="calendario_precarga" tabindex="-1" role="dialog" aria-labelledby="Precarga_rapida" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Cerar"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title" id="Precarga_rapida">Precarga rápida de actividades</h4>
                </div>
                <div class="modal-body">
                    <p>Seleccionar una fecha para precargar las actividades que se ingresaron anteriormente.</p>

                    <div class="form-inline">
                        <div class="form-group">
                            <asp:DropDownList ID="ddl_CargaPordia" runat="server" CssClass="form-control">
                                <asp:ListItem Value="">Seleccionar fecha</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <asp:LinkButton ID="lb_precargarFecha" runat="server" CssClass="btn btn-success">Aceptar</asp:LinkButton>
                    </div>

                    
                </div>

            </div>
        </div>
    </div>
</asp:Content>

