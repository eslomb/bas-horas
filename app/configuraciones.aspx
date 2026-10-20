<%@ Page Title="Configuraciones generales" Language="VB" MasterPageFile="~/masters/App.master" AutoEventWireup="false"
    CodeFile="configuraciones.aspx.vb" Inherits="app_configuraciones" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" runat="Server">
    <h1 class="tituloPagina">Configuraciones generales</h1>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>

            <div class="row">
                <div class="col-md-4">

                    <div class="panel">
                        <div class="panel-heading">
                            <b>Permitir carga</b>
                        </div>
                        <div class="panel-body">
                            <p>Permitir carga de actividades para todos los usuarios del sistema</p>
                            <div class="form-inline">
                                <div class="form-group">
                                    <div class="input-group">
                                        <cc:RadioButtonList ID="rbl_permitir" runat="server" RepeatDirection="Horizontal">
                                            <asp:ListItem Value="True" Text="SI"></asp:ListItem>
                                            <asp:ListItem Value="False" Text="NO"></asp:ListItem>
                                        </cc:RadioButtonList>
                                        <div class="input-group-addon">
                                            <asp:LinkButton ID="lb_habilitar_cargas" runat="server"><i class="fa fa-refresh"></i></asp:LinkButton>
                                        </div>
                                    </div>
                                </div>
                            </div>

                        </div>

                    </div>

                </div>
                <div class="col-md-4">
                    <div class="panel">
                        <div class="panel-heading">
                            <b>Período abierto global</b> <a href="javascript:;" class="tooltips" title="Todos los usuarios que tengan asignado un valor individual menor, tomarán este parámetro como referencia">
                                <span class="badge"><i class="fa fa-question"></i></span></a>
                            <asp:RangeValidator ID="rv_periodoCarga" runat="server" ErrorMessage="Ingreasar un número entero entre 0 y 180" ControlToValidate="tb_periodoCarga" Text="*"
                                Type="Integer" MinimumValue="0" MaximumValue="180"></asp:RangeValidator>
                        </div>
                        <div class="panel-body">
                            <p>Cantidad de días que se le abren a todos los usuarios para cargar tareas</p>
                            <div class="form-inline">
                                <div class="form-group">
                                    <div class="input-group">
                                        <asp:TextBox ID="tb_periodoCarga" runat="server" CssClass="form-control"></asp:TextBox>

                                        <div class="input-group-addon">
                                            <asp:LinkButton ID="lb_actualizar_periodo" runat="server"><i class="fa fa-refresh"></i></asp:LinkButton>
                                        </div>

                                    </div>
                                </div>
                            </div>

                        </div>

                    </div>
                </div>
                <div class="col-md-4">

                    <div class="panel">
                        <div class="panel-heading">
                            <b>Editar Fecha Proyecto Inicio</b>
                        </div>
                        <div class="panel-body">
                            <p>Habilita la edición de la fecha de inicio para un proyecto de la página <a href="Productos.aspx">Productos</a></p>
                            <div class="form-inline">
                                <div class="form-group">
                                    <div class="input-group">
                                        <cc:RadioButtonList ID="rbl_FPIni" runat="server" RepeatDirection="Horizontal">
                                            <asp:ListItem Value="True" Text="SI"></asp:ListItem>
                                            <asp:ListItem Value="False" Text="NO"></asp:ListItem>
                                        </cc:RadioButtonList>
                                        <div class="input-group-addon">
                                            <asp:LinkButton ID="lb_habilitar_FPIni" runat="server"><i class="fa fa-refresh"></i></asp:LinkButton>
                                        </div>
                                    </div>
                                </div>
                            </div>

                        </div>

                    </div>

                </div>
            </div>


            <div class="row">

                <div class="col-md-4">

                    <div class="panel">
                        <div class="panel-heading">
                            <b>Actividades con Observacion</b>
                        </div>
                        <div class="panel-body">
                            <p>Lista csv de Actividades que requieren completar la Observación</p>
                            <div class="form-inline">
                                <div class="form-group">
                                    <div class="input-group">
                                        <asp:TextBox ID="tb_activ_Observacion" runat="server" CssClass="form-control"></asp:TextBox>

                                        <div class="input-group-addon">
                                            <asp:LinkButton ID="lb_activ_Observacion" runat="server"><i class="fa fa-refresh"></i></asp:LinkButton>
                                        </div>

                                    </div>
                                </div>
                            </div>

                        </div>

                    </div>

                </div>
            </div>
            <!-- /.row -->


        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

