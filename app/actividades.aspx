<%@ Page Title="Actividades" Language="VB" MasterPageFile="~/masters/App.master" AutoEventWireup="false" 
    CodeFile="actividades.aspx.vb" Inherits="app_actividades" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" Runat="Server">
    <h1 class="tituloPagina">Actividades 
        <span class="pull-right">
            <asp:LinkButton ID="lb_exportar" runat="server" CausesValidation="false" CssClass="btn btn-sm btn-success" CommandArgument="Actividades"><i class="fa fa-file-excel-o"></i> Exportar a Excel</asp:LinkButton>
            <asp:LinkButton ID="lb_nuevo" runat="server" CssClass="btn btn-sm btn-primary" CausesValidation="false"><i class="fa fa-plus"></i> Nueva</asp:LinkButton></span>
    </h1>


    <asp:EntityDataSource ID="eds_listado" runat="server" ConnectionString="name=BASHorasEntities" DefaultContainerName="BASHorasEntities" 
        EnableDelete="True" EnableFlattening="False" EnableInsert="True" EnableUpdate="True" EntitySetName="ACTIVIDADES" OrderBy="it.Actividad"></asp:EntityDataSource>

    <asp:EntityDataSource ID="eds_abm" runat="server" ConnectionString="name=BASHorasEntities" DefaultContainerName="BASHorasEntities" 
        EnableFlattening="False" EnableInsert="True" EntitySetName="ACTIVIDADES">
    </asp:EntityDataSource>
    
    <cc:GridView ID="gv_listado" runat="server" AutoGenerateColumns="False" DataKeyNames="IDAct" DataSourceID="eds_listado" CssClass="table table-striped dataTable" GridLines="None">
        <Columns>
            <asp:BoundField DataField="IDAct" HeaderText="IDAct" ReadOnly="True" SortExpression="IDAct" ControlStyle-CssClass="hidden" HeaderStyle-CssClass="hidden" ItemStyle-CssClass="hidden" />
            <asp:BoundField DataField="Codigo" HeaderText="Código" SortExpression="Codigo" ControlStyle-CssClass="form-control input-sm" />
            <asp:BoundField DataField="Actividad" HeaderText="Actividad" SortExpression="Actividad" ControlStyle-CssClass="form-control input-sm" />
            <asp:CheckBoxField DataField="Habilitada" HeaderText="Habilitada" SortExpression="Habilitada"  />
            <asp:TemplateField ShowHeader="False" HeaderStyle-CssClass="noSort" ItemStyle-CssClass="text-right">
                <EditItemTemplate>
                    <asp:LinkButton ID="lb_edit_update" runat="server" CausesValidation="True" CommandName="Update" CssClass="btn btn-xs btn-success"><i class="fa fa-check"></i></asp:LinkButton> 
                    <asp:LinkButton ID="lb_edit_cancel" runat="server" CausesValidation="False" CommandName="Cancel" CssClass="btn btn-xs btn-danger"><i class="fa fa-times"></i></asp:LinkButton>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:LinkButton ID="lb_item_edit" runat="server" CausesValidation="False" CommandName="Edit" CssClass="btn btn-xs btn-default"><i class="fa fa-pencil"></i></asp:LinkButton> 
                    <asp:LinkButton ID="lb_item_cancel" runat="server" CausesValidation="False" CommandName="Delete" CssClass="btn btn-xs btn-danger" OnClientClick="return confirm('Este registro se eliminará permanentemente');"><i class="fa fa-trash"></i></asp:LinkButton>
                </ItemTemplate>
           </asp:TemplateField>
        </Columns>
        
    </cc:GridView>

    <asp:FormView ID="fv_abm" runat="server" DataKeyNames="IDAct" DataSourceID="eds_abm">
        <EditItemTemplate></EditItemTemplate>
        <InsertItemTemplate>
            <p><label>Código
                <asp:RequiredFieldValidator ID="rfv_codigo" runat="server" ErrorMessage="Ingresar el código de la actividad" Text="*" ControlToValidate="CodigoTextBox"></asp:RequiredFieldValidator>
               </label>
                <asp:TextBox ID="CodigoTextBox" runat="server" Text='<%# Bind("Codigo") %>' MaxLength="10" CssClass="form-control" />
            </p>

            <p><label>Actividad
            <asp:RequiredFieldValidator ID="rfv_producto" runat="server" ErrorMessage="Ingresar el nombre de la actividad" Text="*" ControlToValidate="ActividadTextBox"></asp:RequiredFieldValidator>
               </label>
                <asp:TextBox ID="ActividadTextBox" runat="server" Text='<%# Bind("Actividad")%>' MaxLength="400" CssClass="form-control" />
            </p>

            <p><label>Habilitada</label>
                <asp:CheckBox ID="HabilitadaTextBox" Text="" Checked='<%#Bind("Habilitada")%>' runat="server" />
            </p>
           
            <fvc:AdminCtrl runat="server" ID="controles" Modo="Insert"></fvc:AdminCtrl>
        </InsertItemTemplate>
        <ItemTemplate></ItemTemplate>
        </asp:FormView>
</asp:Content>

