<%@ Page Title="Usuarios registrados" Language="VB" MasterPageFile="~/masters/App.master" AutoEventWireup="false" 
    CodeFile="usuarios.aspx.vb" Inherits="app_usuarios" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" Runat="Server">
    <h1 class="tituloPagina">Usuarios registrados en el sistema
        <span class="pull-right">
            <asp:LinkButton ID="lb_exportar" runat="server" CausesValidation="false" CssClass="btn btn-sm btn-success" CommandArgument="Usuarios"><i class="fa fa-file-excel-o"></i> Exportar a Excel</asp:LinkButton>
            <asp:HyperLink ID="hl_nuevo_usr" runat="server" CssClass="btn btn-sm btn-primary" NavigateUrl="~/app/usuario_detalle.aspx"><i class="fa fa-user-plus"></i> Nuevo</asp:HyperLink>
            
        </span>
    </h1>


    <div class="row">
        <div class="col-md-7">
            <p>              
                <cc:RadioButtonList ID="rbl_grupo" runat="server" AutoPostBack="true" RepeatLayout="Flow" RepeatDirection="Horizontal" AppendDataBoundItems="True">
                    <asp:ListItem Value="">Todos</asp:ListItem>
                </cc:RadioButtonList>
            </p>
            <p><strong><asp:Literal ID="l_totalPorGrupo" runat="server"></asp:Literal></strong></p>
        </div>
        
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
        <cc:GridView ID="gvUsuarios" runat="server" AutoGenerateColumns="False" EsDatatable="True" EnableViewState="true">
            <Columns>
                <asp:BoundField HeaderText="Nombre" DataField="Nombre"  />
                <asp:BoundField HeaderText="Legajo" DataField="Legajo"  />
                <asp:BoundField HeaderText="Centro" DataField="Centro"  />
                <asp:BoundField HeaderText="Dependencia" DataField="Dependencia" />  
                <asp:BoundField HeaderText="Jorn. Lab." DataField="Modalidad" DataFormatString="{0:N}"  />
                <asp:BoundField HeaderText="Período" DataField="Periodo" DataFormatString="{0:N}"  /> 
                <asp:BoundField HeaderText="CUIL" DataField="CUIL" />  
                <asp:BoundField HeaderText="Habilitado" DataField="Habilitado" /> 
                <asp:BoundField HeaderText="Comentario" DataField="Comentarios" />   
                <asp:TemplateField HeaderText="Acciones" HeaderStyle-CssClass="noSort">
                    <ItemTemplate>
                        <asp:HyperLink ID="hl_ver" runat="server" NavigateUrl='<%# Eval("Ver", "usuario_detalle.aspx?email={0}") %>' ToolTip="Ver"><i class="fa fa-search"></i></asp:HyperLink> | 
                        <asp:LinkButton ID="lb_eliminarUsuario" runat="server" ToolTip="Eliminar" OnClientClick="javascript:return confirm('¿Eliminar este usuario?');" OnClick="lb_eliminarUsuario_Click" CommandArgument='<%# Eval("Nombre") %>'><i class="fa fa-trash"></i></asp:LinkButton>
                        
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </cc:GridView>

    </div>
</asp:Content>

