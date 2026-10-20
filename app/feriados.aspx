<%@ Page Title="Feriados" Language="VB" MasterPageFile="~/masters/App.master" AutoEventWireup="false" 
    CodeFile="feriados.aspx.vb" Inherits="app_feriados" Trace="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" Runat="Server">
    <h1 class="tituloPagina">Feriados 
        <span class="pull-right"><asp:LinkButton ID="lb_nuevo" runat="server" CssClass="btn btn-sm btn-primary" CausesValidation="false"><i class="fa fa-plus"></i> Nuevo</asp:LinkButton></span>
    </h1>


    <asp:EntityDataSource ID="eds_listado" runat="server" ConnectionString="name=BASHorasEntities" DefaultContainerName="BASHorasEntities" 
        EnableDelete="True" EnableFlattening="False" EnableInsert="True" EnableUpdate="True" EntitySetName="FERIADOS" OrderBy="it.Feriado desc"></asp:EntityDataSource>

    <asp:EntityDataSource ID="eds_abm" runat="server" ConnectionString="name=BASHorasEntities" DefaultContainerName="BASHorasEntities" 
        EnableFlattening="False" EnableInsert="True" EntitySetName="FERIADOS">
    </asp:EntityDataSource>
    
    <p><a href="http://www.elproximoferiado.com/" target="_blank">Ver próximos feriados</a></p>
    <cc:GridView ID="gv_listado" runat="server" AutoGenerateColumns="False" DataKeyNames="Feriado" DataSourceID="eds_listado">
        <Columns>
            <asp:BoundField DataField="Feriado" HeaderText="Feriado" ReadOnly="True" SortExpression="Feriado" DataFormatString="{0:d}" />
            <asp:BoundField DataField="Motivo" HeaderText="Motivo" SortExpression="Motivo" ControlStyle-CssClass="form-control input-sm" />
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

    <asp:FormView ID="fv_abm" runat="server" DataKeyNames="Feriado" DataSourceID="eds_abm">
        <EditItemTemplate></EditItemTemplate>
        <InsertItemTemplate>
            
            <p><label>Feriado
                <asp:RequiredFieldValidator ID="rfv_feriado" runat="server" ErrorMessage="Ingresar la fecha del feriado" Text="*" ControlToValidate="tb_feriado"></asp:RequiredFieldValidator>
               </label>
                <cc:Datepicker ID="tb_feriado" runat="server" Text='<%# Bind("Feriado", "{0:d}")%>' CssClass="form-control default-date-picker"></cc:Datepicker>
            </p>

            <p><label>Motivo</label>
                <asp:TextBox ID="MotivoTextBox" runat="server" Text='<%# Bind("Motivo") %>' MaxLength="50" CssClass="form-control" />
            </p>

          
            <fvc:AdminCtrl runat="server" ID="controles" Modo="Insert"></fvc:AdminCtrl>

        </InsertItemTemplate>
        <ItemTemplate></ItemTemplate>
    </asp:FormView>
    
</asp:Content>

