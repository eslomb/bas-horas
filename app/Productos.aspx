<%@ Page Title="Productos" Language="VB" MasterPageFile="~/masters/App.master" AutoEventWireup="false" 
    CodeFile="Productos.aspx.vb" Inherits="app_Productos" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" Runat="Server">
    
    <script>
        $(function () {
            $('.datepicker-ini').datepicker()
                .on('changeDate', function (e) {
                    $(this).datepicker('update', firstDay(e.date));
                });
            $('.datepicker-fin').datepicker()
                .on('changeDate', function (e) {
                    $(this).datepicker('update', lastDay(e.date));
                });
        });
        function firstDay(dateObject) {
            return new Date(dateObject.getFullYear(), dateObject.getMonth() , 1);
        }
        function lastDay(dateObject) {
            return new Date(dateObject.getFullYear(), dateObject.getMonth() + 1, 0);
        }

    </script>

    <h1 class="tituloPagina">Productos 
        <span class="pull-right">
            <asp:LinkButton ID="lb_exportar" runat="server" CausesValidation="false" CssClass="btn btn-sm btn-success" CommandArgument="Productos"><i class="fa fa-file-excel-o"></i> Exportar a Excel</asp:LinkButton>
            <asp:LinkButton ID="lb_nuevo" runat="server" CssClass="btn btn-sm btn-primary" CausesValidation="false"><i class="fa fa-plus"></i> Nuevo</asp:LinkButton></span>
    </h1>


    <asp:EntityDataSource ID="eds_listado" runat="server" ConnectionString="name=BASHorasEntities" DefaultContainerName="BASHorasEntities" 
        EnableDelete="True" EnableFlattening="False" EnableInsert="True" EnableUpdate="True" EntitySetName="PRODUCTOS" OrderBy="it.Producto"></asp:EntityDataSource>

    <asp:EntityDataSource ID="eds_abm" runat="server" ConnectionString="name=BASHorasEntities" DefaultContainerName="BASHorasEntities" 
        EnableFlattening="False" EnableInsert="True" EntitySetName="PRODUCTOS">
    </asp:EntityDataSource>
    
    <cc:GridView ID="gv_listado" runat="server" AutoGenerateColumns="False" 
        DataKeyNames="IDPro" DataSourceID="eds_listado" AllowPaging="True" AllowSorting="True" 
        CssClass="table table-striped" GridLines="None" 
        >
        <Columns>
            <asp:BoundField DataField="IDPro" HeaderText="IDPro" ReadOnly="True" SortExpression="IDPro" ControlStyle-CssClass="hidden" HeaderStyle-CssClass="hidden" ItemStyle-CssClass="hidden" ></asp:BoundField>
            <asp:BoundField DataField="Codigo" HeaderText="Código" SortExpression="Codigo" ControlStyle-CssClass="form-control input-sm" ></asp:BoundField>
            <asp:BoundField DataField="Producto" HeaderText="Producto" SortExpression="Producto" ControlStyle-CssClass="form-control input-sm" ></asp:BoundField>
            <asp:CheckBoxField  DataField="Activo" HeaderText="Activo" SortExpression="Activo"  />
            <asp:TemplateField HeaderText="FP Inicio" SortExpression="FProyIni">
                <EditItemTemplate>
                    <cc:DatePicker ID="tb_FProyIni" runat="server" Text='<%# Bind("FProyIni", "{0:d}")%>' 
                        Enabled='<%# FPIniEditable or Eval("FProyIni") is Nothing %>' 
                        CssClass="form-control default-date-picker datepicker-ini"></cc:DatePicker>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="l_FProyIni" runat="server" Text='<%# Bind("FProyIni", "{0:d}")%>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="FP Fin" SortExpression="FProyFin">
                <EditItemTemplate>
                    <cc:DatePicker ID="tb_FProyFin" runat="server" Text='<%# Bind("FProyFin", "{0:d}")%>' 
                        CssClass="form-control default-date-picker datepicker-fin"></cc:DatePicker>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="l_FProyFin" runat="server" Text='<%# Bind("FProyFin", "{0:d}")%>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            
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

<PagerStyle CssClass="paginador"></PagerStyle>

<SortedAscendingHeaderStyle CssClass="paginador-sort-asc"></SortedAscendingHeaderStyle>

<SortedDescendingHeaderStyle CssClass="paginador-sort-desc"></SortedDescendingHeaderStyle>
    </cc:GridView>

    <asp:FormView ID="fv_abm" runat="server" DataKeyNames="IDPro" DataSourceID="eds_abm">
        <EditItemTemplate></EditItemTemplate>
        <InsertItemTemplate>
            <div class="row">
                <div class="col-md-6">
                    <p><label>Código
                        <asp:RequiredFieldValidator ID="rfv_codigo" runat="server" ErrorMessage="Ingresar el código de producto" Text="*" ControlToValidate="CodigoTextBox"></asp:RequiredFieldValidator>
                        </label>
                        <asp:TextBox ID="CodigoTextBox" runat="server" Text='<%# Bind("Codigo") %>' MaxLength="10" CssClass="form-control" />
                    </p>
                </div>
                <div class="col-md-6">
                        <p><label>Producto
                        <asp:RequiredFieldValidator ID="rfv_producto" runat="server" ErrorMessage="Ingresar el nombre del producto" Text="*" ControlToValidate="ProductoTextBox"></asp:RequiredFieldValidator>
                           </label>
                            <asp:TextBox ID="ProductoTextBox" runat="server" Text='<%# Bind("Producto")%>' MaxLength="400" CssClass="form-control" />
                        </p>
            
                        <p><label>Activo</label>
                            <asp:CheckBox ID="cb_activo" runat="server" Checked='<%# Bind("Activo") %>'  />
                        </p>
                </div>
            </div>
            
            
            <h4>Proyecto</h4>

            <div class="row">
                
                <div class="col-md-6">

                    <p><label>Fecha Proyecto Inicio </label>
                        <cc:Datepicker ID="dp_FProyIni" runat="server"  Text='<%# Bind("FProyIni", "{0:d}")%>' CssClass="form-control default-date-picker datepicker-ini" ></cc:Datepicker> 
                        <em>una vez asignada no se podrá editar</em>
                    </p>
            
                </div>
                <div class="col-md-6">

                    <p><label>Fecha Proyecto Fin</label>
                        <cc:Datepicker ID="dp_FProyFin" runat="server" Text='<%# Bind("FProyFin", "{0:d}")%>' CssClass="form-control default-date-picker datepicker-fin" ></cc:Datepicker>      
                    </p>
            
                    <asp:CompareValidator ID="cv_fechas" runat="server" 
                    ControlToValidate = "dp_FProyIni" ControlToCompare = "dp_FProyFin" Operator = "LessThan" Type="Date" ForeColor="Red"
                    ErrorMessage="La fecha de fin del proyecto debe no puede ser menor a la de inicio"></asp:CompareValidator>
                    <br />
                </div>
            </div>



            <fvc:AdminCtrl runat="server" ID="controles" Modo="Insert"></fvc:AdminCtrl>
        </InsertItemTemplate>
        <ItemTemplate></ItemTemplate>
    </asp:FormView>
</asp:Content>

