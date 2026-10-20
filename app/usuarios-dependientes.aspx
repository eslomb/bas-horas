<%@ Page Title="Usuarios básicos" Language="VB" MasterPageFile="~/masters/App.master" AutoEventWireup="false" 
    CodeFile="usuarios-dependientes.aspx.vb" Inherits="app_usuarios_dependientes" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" Runat="Server">
    <style>
        table .form-control.periodo-individual{width: 48px;}
    </style>
    <h1 class="tituloPagina">Usuarios básicos dependientes de Legajo <asp:Literal ID="l_legajo" runat="server"></asp:Literal></h1>
        
    <p>El Período es la cantidad de días que se le habilita a un usuario para cargar tareas.<br />
        Cuando este valor es cero, se le habilita desde el primer día del mes corriente</p>

    
    <div class="panel">
        <div class="panel-body">
            <div class="row">
                <div class="col-md-4">
                    <p><strong>Período global:</strong> <asp:RangeValidator ID="RangeValidator1" runat="server" ErrorMessage="Periodo no válido" Text="*" ControlToValidate="tb_periodo_general"
                            Type="Integer" MinimumValue="0" MaximumValue="365" ForeColor="Red"></asp:RangeValidator>
                        
                        <asp:TextBox ID="tb_periodo_general" CssClass="form-control periodo-general" runat="server" MaxLength="3" EnableViewState="false" Text=""></asp:TextBox>
                        
                        <span class="text-muted">Permite establecer rápidamente el mismo período para todos los usuarios de la lista. 
                            Ingresando un valor aquí, se copiará a todos los Periodos individuales.</span>
                    </p>
                </div>
                
                <div class="col-md-4">
                    <asp:Panel runat="server" ID="pnl_centros">
                        <p>
                            <strong>Usuario: </strong>
                            <asp:DropDownList ID="ddl_usuarios" runat="server" AutoPostBack="true" CssClass="form-control" 
                                DataTextField="UserName" DataValueField="Legajo"></asp:DropDownList>
                        </p>
                    </asp:Panel>
                    
                </div>

                <div class="col-md-4">
                    <p><br />
                        <asp:LinkButton ID="lb_actualizar_datos" runat="server" CssClass="btn btn-primary"><i class="fa fa-save"></i> Guardar Periodos</asp:LinkButton>
                    </p>
                </div>
            </div>
        </div>

    </div>




    <div>
        <asp:Literal ID="l_resultado" runat="server" EnableViewState="false"></asp:Literal>
    </div>
    <div class="row">
        <div class="col-md-5 text-right">
            <div runat="server" id="filtro" visible="false">
                <div class="form-inline">
                    <div class="form-group">
                        <asp:TextBox ID="tb_nombreUsuario" runat="server" CssClass="form-control input-sm"></asp:TextBox> 
                        <asp:LinkButton ID="lb_buscar" CssClass="btn btn-default btn-sm" runat="server">Buscar nombre</asp:LinkButton>
                    </div>
                </div>
            </div>
        </div>
    </div>
    
    <div class="form-inline">
        <cc:GridView ID="gvUsuarios" runat="server" AutoGenerateColumns="False" EsDatatable="False" EnableViewState="true" AllowPaging="false" AllowSorting="false" PageSize="1000" >
            <Columns>
                <asp:BoundField HeaderText="Nombre" DataField="userName"  />
                <asp:BoundField HeaderText="Legajo" DataField="Legajo"  />
                <asp:BoundField HeaderText="Jorn. Lab." DataField="Modalidad" DataFormatString="{0:N}"  />
                <asp:TemplateField HeaderText="Período" >
                    <ItemTemplate>
                        <asp:TextBox ID="tb_periodo" runat="server" CssClass="form-control periodo-individual" Text='<%#Eval("PeriodoAbierto")%>'></asp:TextBox>
                        <asp:RangeValidator ID="rv_periodo" runat="server" ErrorMessage="Periodo no válido" Text="*" ControlToValidate="tb_periodo" 
                            Type="Integer" MinimumValue="0" MaximumValue="365" ForeColor="Red"></asp:RangeValidator>
                        <asp:HiddenField ID="hf_periodo" runat="server" Value='<%#Eval("PeriodoAbierto")%>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField HeaderText="Habilitado" DataField="Habilitado" /> 
                <asp:BoundField HeaderText="Comentario" DataField="Comentario" />                
            </Columns>
            <EmptyDataTemplate>
                <h5>No se encontraron usuarios Básicos dependientes de este Usuario.</h5>
            </EmptyDataTemplate>
        </cc:GridView>

    </div>

    <script>
        $(function () {
            $(".periodo-general").keyup(function (e) {
                $(".periodo-individual").val($(this).val());
            });
        })
    </script>
</asp:Content>

