<%@ Page Title="Centros" Language="VB" MasterPageFile="~/masters/App.master" AutoEventWireup="false" 
    CodeFile="centros.aspx.vb" Inherits="app_centros" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" Runat="Server">
    <h1 class="tituloPagina">Centros
        <span class="pull-right">
            <asp:LinkButton ID="lb_exportar" runat="server" CausesValidation="false" CssClass="btn btn-sm btn-success" CommandArgument="Centros"><i class="fa fa-file-excel-o"></i> Exportar a Excel</asp:LinkButton>
            <asp:LinkButton ID="lb_nuevo" runat="server" CssClass="btn btn-sm btn-primary" CausesValidation="false"><i class="fa fa-plus"></i> Nuevo</asp:LinkButton></span>
    </h1>

    

    <asp:EntityDataSource ID="eds_listado" runat="server" ConnectionString="name=BASHorasEntities" DefaultContainerName="BASHorasEntities" 
        EnableDelete="True" EnableFlattening="False" EnableInsert="True" EnableUpdate="True" EntitySetName="CENTROS" OrderBy="it.Nombre"></asp:EntityDataSource>

    <asp:EntityDataSource ID="eds_abm" runat="server" ConnectionString="name=BASHorasEntities" DefaultContainerName="BASHorasEntities" 
        EnableFlattening="False" EnableInsert="True" EntitySetName="CENTROS">
    </asp:EntityDataSource>
    
    <cc:GridView ID="gv_listado" runat="server" AutoGenerateColumns="False" DataKeyNames="CodCen" DataSourceID="eds_listado" PageSize="20">
        <Columns>
            <asp:BoundField DataField="CodCen" HeaderText="Codigo" SortExpression="CodCen" ControlStyle-CssClass="form-control input-sm" />
            <asp:BoundField DataField="Nombre" HeaderText="Nombre" SortExpression="Nombre" ControlStyle-CssClass="form-control input-sm" />
            <asp:TemplateField ShowHeader="False" HeaderStyle-CssClass="noSort" ItemStyle-CssClass="text-right">
                <EditItemTemplate>
                    <asp:LinkButton ID="lb_edit_update" runat="server" CausesValidation="True" CommandName="Update" CssClass="btn btn-xs btn-success"><i class="fa fa-check"></i></asp:LinkButton> 
                    <asp:LinkButton ID="lb_edit_cancel" runat="server" CausesValidation="False" CommandName="Cancel" CssClass="btn btn-xs btn-danger"><i class="fa fa-times"></i></asp:LinkButton>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:LinkButton ID="lb_item_edit" runat="server" CausesValidation="False" CommandName="Edit" CssClass="btn btn-xs btn-default"><i class="fa fa-pencil"></i></asp:LinkButton> 
                    <asp:LinkButton ID="lb_item_cancel" runat="server" CausesValidation="False" CommandName="Delete" CssClass="btn btn-xs btn-danger" OnClientClick="return confirm('Este registro se eliminará permanentemente');"><i class="fa fa-trash"></i></asp:LinkButton>
                </ItemTemplate>
                <HeaderStyle CssClass="noSort"></HeaderStyle>
            </asp:TemplateField>
        </Columns>
    </cc:GridView>

    <asp:FormView ID="fv_abm" runat="server" DataKeyNames="CodCen" DataSourceID="eds_abm">
        <EditItemTemplate></EditItemTemplate>
        <InsertItemTemplate>
            <p><label>Código
                <asp:RequiredFieldValidator ID="rfv_codigo" runat="server" ErrorMessage="Ingresar el código" Text="*" ControlToValidate="tb_codigo"></asp:RequiredFieldValidator>
               </label>
               <asp:TextBox ID="tb_codigo" runat="server" Text='<%# Bind("CodCen")%>'  MaxLength="50" CssClass="form-control" />
            </p>

            <p><label>Nombre
                <asp:RequiredFieldValidator ID="rfv_nombre" runat="server" ErrorMessage="Ingresar el nombre" Text="*" ControlToValidate="NombreTextBox"></asp:RequiredFieldValidator>
               </label>
               <asp:TextBox ID="NombreTextBox" runat="server" Text='<%# Bind("Nombre") %>'  MaxLength="50" CssClass="form-control" />
            </p>

            
           <fvc:AdminCtrl runat="server" ID="controles" Modo="Insert"></fvc:AdminCtrl>
        </InsertItemTemplate>
        <ItemTemplate></ItemTemplate>
    </asp:FormView>
    
</asp:Content>

