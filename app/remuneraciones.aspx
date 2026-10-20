<%@ Page Title="Remuneraciones" Language="VB" MasterPageFile="~/masters/App.master" AutoEventWireup="false" 
    CodeFile="remuneraciones.aspx.vb" Inherits="app_remuneraciones" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" Runat="Server">
     <h1 class="tituloPagina">Remuneraciones por mes/año
        <span class="pull-right">
            <asp:LinkButton ID="lb_exportar" runat="server" CausesValidation="false" CssClass="btn btn-sm btn-success" CommandArgument="Remuneraciones"><i class="fa fa-file-excel-o"></i> Exportar a Excel</asp:LinkButton>
            <asp:LinkButton ID="lb_nuevo" runat="server" CssClass="btn btn-sm btn-primary" CausesValidation="false"><i class="fa fa-plus"></i> Nuevo</asp:LinkButton>
        </span>
    </h1>

    <asp:EntityDataSource ID="eds_listado" runat="server" ConnectionString="name=BASHorasEntities" DefaultContainerName="BASHorasEntities" 
        EntitySetName="REMUNERACIONES" EnableDelete="true" EnableUpdate="true" EnableInsert="false"
        Where="(it.Mes=@Mes or @Mes is NULL) and (it.Anio=@Anio or @Anio is null)" OrderBy="it.Anio Desc, it.Mes Desc, it.Usuario" EntityTypeFilter="REMUNERACIONES">
        <WhereParameters>
            <asp:ControlParameter Name="Mes" ControlID="ddl_mes" DbType="Int16" ConvertEmptyStringToNull="true" />
            <asp:ControlParameter Name="Anio" ControlID="ddl_anio" DbType="Int16"  ConvertEmptyStringToNull="true" />
        </WhereParameters>
    </asp:EntityDataSource>

    <asp:EntityDataSource ID="eds_abm" runat="server" ConnectionString="name=BASHorasEntities" DefaultContainerName="BASHorasEntities" 
        EnableFlattening="False" EnableInsert="True" EnableUpdate="true" EnableDelete="true" EntitySetName="REMUNERACIONES">
    </asp:EntityDataSource>

    <asp:ObjectDataSource ID="ods_usuarios" runat="server" SelectMethod="listarUsuarios" TypeName="Aplicacion.Logica">
    </asp:ObjectDataSource>

    <div class="row">
        <div class="col-sm-4">
            <!-- Filtros -->
            <div class="panel panel-default">
                <div class="panel-heading">
                    <h4>Filtros </h4>
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="col-sm-6">
                            <p>                        
                                <asp:DropDownList ID="ddl_mes" runat="server" CssClass="form-control seleccionar-actual" OnLoad="ddl_meses_OnLoad" AppendDataBoundItems="true">
                                    <asp:ListItem Value="">Todos</asp:ListItem>
                                </asp:DropDownList>
                            </p>
                        </div>
                        <div class="col-sm-6">
                            <p>                        
                                <asp:DropDownList ID="ddl_anio" runat="server" SelectedValue='<%# Bind("Anio")%>' CssClass="form-control seleccionar-actual" OnLoad="ddl_anios_OnLoad" AppendDataBoundItems="true">
                                    <asp:ListItem Value="">Todos</asp:ListItem>
                                </asp:DropDownList>
                            </p>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-sm-6">
                            <p>
                                <asp:LinkButton ID="lb_filtrar" runat="server" CssClass="btn btn-primary"><i class="fa fa-filter"></i> Aplicar</asp:LinkButton>
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>


        <div class="col-sm-8">
            <!-- Importacion -->
            <div class="panel panel-default">
                <div class="panel-heading">
                    <h4>Importación en lotes</h4> 
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="col-sm-6">
                            <p>1. Exportar listado de los usuarios activos.<br />
                                Luego modificar las remuneraciones; dejarlas en blanco o cero si no requieren importar.<br />
                                Los datos se exportan con el mes/año actual y las últimas remuneraciones conocidas para cada usuario.
                            </p>
                            <p>
                                <asp:LinkButton ID="lb_exportar_activos" runat="server" CssClass="btn btn-info" CausesValidation="false">
                                    <i class="fa fa-download" ></i> Exportar
                                </asp:LinkButton>
                            </p>
                        </div>
                        <div class="col-sm-6">
                            <p>2. Importar las remuneraciones desde la planilla exportada.</p>

                            <p>
                                <asp:FileUpload ID="fup_excel" runat="server" /> <br />
                                <asp:LinkButton ID="lb_importar" runat="server" CssClass="btn btn-success" CausesValidation="false"><i class="fa fa-upload"></i> Importar planilla</asp:LinkButton>
                            </p>
                        </div>
                    </div>
                </div>        
            </div>
        </div>
    </div>


    <asp:GridView ID="gv_exportacion" runat="server" Visible="false" AutoGenerateColumns="false" EnableViewState="false">
        <Columns>
            <asp:BoundField DataField="Mes" HeaderText="Mes" />
            <asp:BoundField DataField="Anio" HeaderText="Anio" />
            <asp:BoundField DataField="Remuneracion" HeaderText="Remuneracion" />
            <asp:BoundField DataField="Usuario" HeaderText="Usuario" HtmlEncode="false" />            
            <asp:BoundField DataField="Legajo" HeaderText="Legajo" />            
        </Columns>
     </asp:GridView>

    <cc:GridView ID="gv_listado" runat="server" AutoGenerateColumns="False" DataKeyNames="IDRem" DataSourceID="eds_listado" AllowPaging="True" AllowSorting="True" CssClass="table table-striped" GridLines="None">
        <Columns>            
            <asp:BoundField DataField="IDRem" HeaderText="ID" ReadOnly="True" SortExpression="IDRem" ControlStyle-CssClass="hidden" HeaderStyle-CssClass="hidden" ItemStyle-CssClass="hidden" >
                <ControlStyle CssClass="hidden"></ControlStyle>
                <HeaderStyle CssClass="hidden"></HeaderStyle>
                <ItemStyle CssClass="hidden"></ItemStyle>
            </asp:BoundField>
            <asp:TemplateField HeaderText="Anio" SortExpression="Anio">
                <EditItemTemplate>
                    <asp:DropDownList ID="ddl_anio" runat="server" SelectedValue='<%# Bind("Anio")%>' CssClass="form-control" OnLoad="ddl_anios_OnLoad"></asp:DropDownList>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Literal ID="Label1" runat="server" Text='<%# Bind("Anio") %>'></asp:Literal>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Mes" SortExpression="Mes">
                <EditItemTemplate>
                    <asp:DropDownList ID="ddl_mes" runat="server" SelectedValue='<%# Bind("Mes")%>' CssClass="form-control" OnLoad="ddl_meses_OnLoad"></asp:DropDownList>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Literal ID="Label2" runat="server" Text='<%# Bind("Mes") %>'></asp:Literal>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Legajo" HeaderText="Legajo"  SortExpression="Legajo"  ControlStyle-CssClass="form-control" />
            <asp:TemplateField HeaderText="Usuario" SortExpression="Usuario">
                <EditItemTemplate>
                    <asp:DropDownList ID="ddl_usuarios" runat="server" CssClass="form-control chosen" SelectedValue='<%# Bind("Usuario")%>' DataSourceID="ods_usuarios"></asp:DropDownList>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Literal ID="Label3" runat="server" Text='<%# Bind("Usuario") %>'></asp:Literal>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Remuneracion" HeaderText="Remuneracion"  SortExpression="Remuneracion"  ControlStyle-CssClass="form-control" >
            
<ControlStyle CssClass="form-control"></ControlStyle>
            </asp:BoundField>
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
                <ItemStyle CssClass="text-right"></ItemStyle>
           </asp:TemplateField>
        </Columns>

        <PagerStyle CssClass="paginador"></PagerStyle>

        <SortedAscendingHeaderStyle CssClass="paginador-sort-asc"></SortedAscendingHeaderStyle>

        <SortedDescendingHeaderStyle CssClass="paginador-sort-desc"></SortedDescendingHeaderStyle>
     </cc:GridView>


     <asp:FormView ID="fv_abm" runat="server" DataKeyNames="IDRem" DataSourceID="eds_abm"> 
         <EditItemTemplate></EditItemTemplate>        
         <InsertItemTemplate>

             <div class="row">
                 <div class="col-sm-6">
                    <p><label>Mes</label>
                        <asp:DropDownList ID="ddl_mes" runat="server" SelectedValue='<%# Bind("Mes")%>' CssClass="form-control seleccionar-actual" OnLoad="ddl_meses_OnLoad"></asp:DropDownList>
                    </p>

                    <p><label>Año</label>
                        <asp:DropDownList ID="ddl_anio" runat="server"  SelectedValue='<%# Bind("Anio")%>' CssClass="form-control" OnLoad="ddl_anios_OnLoad"></asp:DropDownList>
                    </p>
                 </div>
                 <div class="col-sm-6">

                    <p><label>Usuario</label>
                        <asp:DropDownList ID="ddl_usuarios" runat="server" CssClass="form-control chosen"  SelectedValue='<%# Bind("Usuario")%>' DataSourceID="ods_usuarios"></asp:DropDownList>                        
                    </p>
                    <p><label>Remuneración
                        <asp:RequiredFieldValidator ID="rfv_remun" runat="server" ErrorMessage="Ingresar la Remuneración" Text="*" ControlToValidate="RemuneracionTextBox"></asp:RequiredFieldValidator>
                        <asp:RangeValidator ID="rv_remun" runat="server" ErrorMessage="La remuneración no es un número válido"  Text="*"
                            MinimumValue="0" MaximumValue="1000000" Type="Currency" ControlToValidate="RemuneracionTextBox"></asp:RangeValidator>
                       </label>
                        <asp:TextBox ID="RemuneracionTextBox" runat="server" Text='<%# Bind("Remuneracion") %>' MaxLength="10" CssClass="form-control" />
                    </p>
                 </div>
             </div>

             <fvc:AdminCtrl runat="server" ID="controles" Modo="Insert"></fvc:AdminCtrl>
         </InsertItemTemplate>
         <ItemTemplate></ItemTemplate>        
     </asp:FormView>
</asp:Content>

