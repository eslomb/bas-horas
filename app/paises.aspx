<%@ Page Title="Paises" Language="VB" MasterPageFile="~/masters/App.master" AutoEventWireup="false" 
    CodeFile="paises.aspx.vb" Inherits="app_paises" Trace="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" Runat="Server">
    <h1 class="tituloPagina">Paises 
        <span class="pull-right"><asp:LinkButton ID="lb_nuevo" runat="server" CssClass="btn btn-sm btn-primary" CausesValidation="false"><i class="fa fa-plus"></i> Nuevo</asp:LinkButton></span>
    </h1>


    <asp:EntityDataSource ID="eds_listado" runat="server" ConnectionString="name=BASHorasEntities" DefaultContainerName="BASHorasEntities" 
        EnableDelete="True" EnableFlattening="False" EnableInsert="True" EnableUpdate="True" EntitySetName="PAISES" OrderBy="it.Pais"></asp:EntityDataSource>

    <asp:EntityDataSource ID="eds_abm" runat="server" ConnectionString="name=BASHorasEntities" DefaultContainerName="BASHorasEntities" 
        EnableFlattening="False" EnableInsert="True" EntitySetName="PAISES">
    </asp:EntityDataSource>
    
    <cc:GridView ID="gv_listado" runat="server" AutoGenerateColumns="False" DataKeyNames="Codigo" DataSourceID="eds_listado">
        <Columns>
            <asp:BoundField DataField="Codigo" HeaderText="Codigo" ReadOnly="True" SortExpression="Codigo" />
            <asp:BoundField DataField="Pais" HeaderText="Pais" SortExpression="Pais" ControlStyle-CssClass="form-control input-sm" />
            <asp:CheckBoxField DataField="Preferido" HeaderText="Preferido" />
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

    <asp:FormView ID="fv_abm" runat="server" DataKeyNames="Codigo" DataSourceID="eds_abm">
        <EditItemTemplate></EditItemTemplate>
        <InsertItemTemplate>
            <p><label>Código
                <asp:RequiredFieldValidator ID="rfv_codigo" runat="server" ErrorMessage="Ingresar el código de 2 letras" Text="*" ControlToValidate="CodigoTextBox"></asp:RequiredFieldValidator>
               </label>
                <asp:TextBox ID="CodigoTextBox" runat="server" Text='<%# Bind("Codigo") %>' MaxLength="2" CssClass="form-control" />
            </p>

            <p><label>Nombre País
            <asp:RequiredFieldValidator ID="rfv_pais" runat="server" ErrorMessage="Ingresar el nombre de país" Text="*" ControlToValidate="PaisTextBox"></asp:RequiredFieldValidator>
               </label>
                <asp:TextBox ID="PaisTextBox" runat="server" Text='<%# Bind("Pais") %>' MaxLength="50" CssClass="form-control" />
            </p>

            <p><label></label>
                <cc:CheckBox ID="PreferidoCheckBox" runat="server" Checked='<%# Bind("Preferido") %>' Text="Preferido" />
            </p>
            <fvc:AdminCtrl runat="server" ID="controles" Modo="Insert"></fvc:AdminCtrl>

        </InsertItemTemplate>
        <ItemTemplate></ItemTemplate>
    </asp:FormView>
    
</asp:Content>

