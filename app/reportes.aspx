<%@ Page Title="Reportes" Language="VB" MasterPageFile="~/masters/App.master" AutoEventWireup="false" 
    CodeFile="reportes.aspx.vb" Inherits="app_reportes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" Runat="Server">
    <h1 class="tituloPagina">Reportes
        <span class="pull-right"><asp:LinkButton ID="lb_nuevo" runat="server" CssClass="btn btn-sm btn-primary" CausesValidation="false"><i class="fa fa-plus"></i> Nuevo</asp:LinkButton></span>
    </h1>
    
    <asp:ObjectDataSource ID="ods_productos" runat="server" SelectMethod="listarProductosVisibles" TypeName="Aplicacion.Logica">
    </asp:ObjectDataSource>
    
    <asp:ObjectDataSource ID="ods_actividades" runat="server" SelectMethod="listarActividadesHabilitadas" TypeName="Aplicacion.Logica">
    </asp:ObjectDataSource>
    
    <asp:ObjectDataSource ID="ods_paises" runat="server" SelectMethod="listarPaises" TypeName="Aplicacion.Logica">
    </asp:ObjectDataSource>
    
    <asp:ObjectDataSource ID="ods_usuarios" runat="server" SelectMethod="listarUsuarios" TypeName="Aplicacion.Logica">
    </asp:ObjectDataSource>
    
    <asp:ObjectDataSource ID="ods_centros" runat="server" SelectMethod="listarCentros" TypeName="Aplicacion.Logica">
    </asp:ObjectDataSource>

    <asp:EntityDataSource ID="eds_listado" runat="server" ConnectionString="name=BASHorasEntities" DefaultContainerName="BASHorasEntities" 
        EnableDelete="True" EnableFlattening="False" EnableInsert="False" EnableUpdate="True" EntitySetName="REPORTES" AutoGenerateWhereClause="true">
        <WhereParameters>
            <asp:ControlParameter ControlID="ddl_usuarios" PropertyName="SelectedValue" Name="Usuario" Type="String" />
        </WhereParameters>
    </asp:EntityDataSource>

    <asp:EntityDataSource ID="eds_abm" runat="server" ConnectionString="name=BASHorasEntities" DefaultContainerName="BASHorasEntities" 
        EnableFlattening="False" EnableInsert="true" EnableUpdate="false" EntitySetName="REPORTES" >
    </asp:EntityDataSource>


    <div class="row">
        <div class="col-sm-6">
            <p>
            <asp:DropDownList ID="ddl_usuarios" runat="server" CssClass="form-control chosen" AutoPostBack="true">
            </asp:DropDownList></p>
        </div>
    </div>
    

    <br />

    <asp:GridView ID="gv_listado" runat="server" AutoGenerateColumns="False" DataKeyNames="IDRep" DataSourceID="eds_listado" 
        AllowPaging="False" AllowSorting="False" CssClass="table table-striped" GridLines="None">
        <Columns>
            <asp:BoundField DataField="IDRep" HeaderText="IDRep" ReadOnly="True" SortExpression="IDRep" Visible="false" />

            <asp:TemplateField HeaderText="Reporte" SortExpression="Titulo">
                <EditItemTemplate>
                    <asp:TextBox ID="tb_titulo" runat="server" MaxLength="150" Text='<%# Bind("Titulo")%>' CssClass="form-control"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfv_ttitulo" runat="server" ErrorMessage="Falta Ingresar el título" Text="*" ControlToValidate="tb_titulo" Display="None"></asp:RequiredFieldValidator>
                    <br />
                    <cc:Datepicker ID="tb_desde" runat="server" Text='<%# Bind("Desde", "{0:d}")%>' placeholder="Desde"></cc:Datepicker>
                    <br />
                    <cc:Datepicker ID="tb_hasta" runat="server" Text='<%# Bind("Hasta", "{0:d}")%>' placeholder="Hasta"></cc:Datepicker>
                </EditItemTemplate>
                <ItemTemplate>
                    <h5 class="sin-margen resaltado"><b><asp:Label ID="l_titulo" runat="server" Text='<%# Bind("Titulo")%>'></asp:Label></b></h5>
                    <br />
                    <asp:Label ID="l_desde" runat="server" Text='<%# Eval("Desde", "<b>Fecha Desde:</b> {0:d}")%>'></asp:Label>
                    <br />
                    <asp:Label ID="l_hasta" runat="server" Text='<%# Eval("Hasta", "<b>Fecha Hasta:</b> {0:d}")%>'></asp:Label>

                </ItemTemplate>
            </asp:TemplateField>
            
            <asp:TemplateField HeaderText="Parámetros">
                <ItemTemplate>
                    <p><b>Producto:</b> <asp:Label ID="l_producto" runat="server" Text='<%# NombreProducto(Eval("Producto_id"))%>'></asp:Label>
                        <br />
                        <b>Actividad:</b> <asp:Label ID="Label1" runat="server" Text='<%# NombreActividad(Eval("Actividad_id"))%>'></asp:Label>
                        <br />
                        <b>País:</b> <asp:Label ID="Label3" runat="server" Text='<%# NombrePais(Eval("CodPais"))%>'></asp:Label></p>
                </ItemTemplate>
                <EditItemTemplate>
                    <p> 
                        <asp:DropDownList ID="ddl_productos" runat="server" CssClass="form-control chosen" AppendDataBoundItems="True" 
                            DataSourceID="ods_productos" DataValueField="IDPro" DataTextField="Producto" SelectedValue='<%# Bind("Producto_id") %>'>
                            <asp:ListItem Value="">Todos los productos</asp:ListItem>
                        </asp:DropDownList></p>

                    <p>
                        <asp:DropDownList ID="ddl_actividades" runat="server" CssClass="form-control chosen" AppendDataBoundItems="True" 
                            DataSourceID="ods_actividades" DataValueField="IDAct" DataTextField="Actividad"  SelectedValue='<%# Bind("Actividad_id") %>'>
                            <asp:ListItem Value="">Todas las actividades</asp:ListItem>
                        </asp:DropDownList></p>
                    <p>
                        <asp:DropDownList ID="ddl_paises" runat="server" CssClass="form-control chosen"  AppendDataBoundItems="True" 
                            DataSourceID="ods_paises" DataValueField="Codigo" DataTextField="Pais" SelectedValue='<%# Bind("CodPais") %>'>
                            <asp:ListItem Value="">Todos los paises</asp:ListItem>
                        </asp:DropDownList></p>
                </EditItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Usuario" HeaderStyle-CssClass="hidden" ItemStyle-CssClass="hidden">
                <EditItemTemplate>
                    <asp:DropDownList ID="ddl_usuarioReporte" runat="server" AppendDataBoundItems="True" CssClass="form-control chosen" 
                            DataSourceID="ods_usuarios" DataValueField="UserName" DataTextField="UserName" SelectedValue='<%# Bind("DeUsuario")%>'> 
                        <asp:ListItem Value="">Todos los usuarios</asp:ListItem>
                    </asp:DropDownList>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="l_deUsuario" runat="server" Text='<%# Eval("DeUsuario")%>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Centro" HeaderStyle-CssClass="hidden" ItemStyle-CssClass="hidden">
                <EditItemTemplate>
                    <asp:DropDownList ID="ddl_deCentro" runat="server" AppendDataBoundItems="True"  CssClass="form-control chosen" 
                            DataSourceID="ods_centros" DataValueField="CodCen" DataTextField="Nombre" SelectedValue='<%# Bind("DeCentro")%>' OnDataBound="ddl_deCentro_DataBound">
                        <asp:ListItem Value="">Todos los Centros</asp:ListItem>
                    </asp:DropDownList>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="l_deCentro" runat="server" Text='<%# Eval("DeCentro")%>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            
            <asp:TemplateField ShowHeader="False" HeaderStyle-CssClass="noSort" ItemStyle-CssClass="text-right">
                <EditItemTemplate>
                    <asp:LinkButton ID="lb_edit_update" runat="server" CausesValidation="True" CommandName="Update" CssClass="btn btn-sm btn-success"><i class="fa fa-check"></i></asp:LinkButton> 
                    <asp:LinkButton ID="lb_edit_cancel" runat="server" CausesValidation="False" CommandName="Cancel" CssClass="btn btn-sm btn-danger"><i class="fa fa-times"></i></asp:LinkButton>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:HyperLink ID="hl_detalle" runat="server" CssClass="btn btn-sm btn-default tooltips" ToolTip="Ver reporte"  NavigateUrl='<%# "reporte.aspx?idrep=" & Eval("IDRep").ToString %>'><i class="fa fa-search"></i></asp:HyperLink>
                    <asp:LinkButton ID="lb_item_edit" runat="server" CausesValidation="False" CommandName="Edit" CssClass="btn btn-sm btn-info"><i class="fa fa-pencil"></i></asp:LinkButton> 
                    <asp:LinkButton ID="lb_item_cancel" runat="server" CausesValidation="False" CommandName="Delete" CssClass="btn btn-sm btn-danger" OnClientClick="return confirm('Este Reporte se eliminará permanentemente');"><i class="fa fa-trash"></i></asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
            <h5>No hay reportes para mostrar. Deberás crear uno nuevo.</h5>
        </EmptyDataTemplate>

        <PagerStyle CssClass="paginador"></PagerStyle>

        <SortedAscendingHeaderStyle CssClass="paginador-sort-asc"></SortedAscendingHeaderStyle>

        <SortedDescendingHeaderStyle CssClass="paginador-sort-desc"></SortedDescendingHeaderStyle>
    </asp:GridView>

    <asp:FormView ID="fv_abm" runat="server" DataKeyNames="IDRep" DataSourceID="eds_abm">
        <EditItemTemplate>

        </EditItemTemplate>
        <InsertItemTemplate>
            <div class="row">
                <div class="col-md-5">
                    <p><label>Título
                        <asp:RequiredFieldValidator ID="rfv_titulo" runat="server" ErrorMessage="Ingresar el Titulo del reporte" Text="*" ControlToValidate="TituloTextBox"></asp:RequiredFieldValidator>
                        </label>
                        <asp:TextBox ID="TituloTextBox" runat="server" Text='<%# Bind("Titulo") %>' MaxLength="150" CssClass="form-control" />
                    </p>
                        
                    <p><label>Fecha Desde </label>
                        <cc:Datepicker ID="tb_Desde" runat="server" Text='<%# Bind("Desde")%>'  />
                    </p>

                    <p><label>Fecha Hasta </label>
                        <cc:Datepicker ID="tb_Hasta" runat="server" Text='<%# Bind("Hasta")%>'  />
                    </p>
                </div>
                <div class="col-md-7">
                    <p>Parámetros</p>
                    <p> 
                        <asp:DropDownList ID="ddl_productos" runat="server" CssClass="form-control chosen" AppendDataBoundItems="True" 
                            DataSourceID="ods_productos" DataValueField="IDPro" DataTextField="Producto" SelectedValue='<%# Bind("Producto_id")%>'>
                            <asp:ListItem Value="">Todos los productos</asp:ListItem>
                        </asp:DropDownList></p>

                    <p>
                        <asp:DropDownList ID="ddl_actividades" runat="server" CssClass="form-control chosen" AppendDataBoundItems="True" 
                            DataSourceID="ods_actividades" DataValueField="IDAct" DataTextField="Actividad" SelectedValue='<%# Bind("Actividad_id")%>'>
                            <asp:ListItem Value="">Todas las actividades</asp:ListItem>
                        </asp:DropDownList></p>
                    <p>
                        <asp:DropDownList ID="ddl_paises" runat="server" CssClass="form-control chosen"  AppendDataBoundItems="True" 
                            DataSourceID="ods_paises" DataValueField="Codigo" DataTextField="Pais">
                            <asp:ListItem Value="">Todos los paises</asp:ListItem>
                        </asp:DropDownList></p>

                    <asp:Panel ID="pnl_insertUsuarioReporte" runat="server" OnLoad="pnl_insertUsuarioReporte_Load" Visible="false">
                        <p><asp:DropDownList ID="ddl_usuarioReporte" runat="server" AppendDataBoundItems="True" CssClass="form-control chosen" 
                                DataSourceID="ods_usuarios" DataValueField="UserName" DataTextField="UserName" SelectedValue='<%# Bind("DeUsuario")%>'> 
                            <asp:ListItem Value="">Todos los usuarios</asp:ListItem>
                        </asp:DropDownList></p>

                    </asp:Panel>

                    <asp:Panel ID="pnl_insertCentroReporte" runat="server" OnLoad="pnl_insertCentroReporte_Load" Visible="false">
                        <p><asp:DropDownList ID="ddl_deCentro" runat="server" AppendDataBoundItems="True"  CssClass="form-control chosen" 
                                DataSourceID="ods_centros" DataValueField="CodCen" DataTextField="Nombre" SelectedValue='<%# Bind("DeCentro")%>'>
                            <asp:ListItem Value="">Todos los Centros</asp:ListItem>
                        </asp:DropDownList></p>

                    </asp:Panel>
                </div>
            </div>
            

            <asp:HiddenField ID="hf_usuario" Value='<%# Bind("Usuario") %>' OnLoad="CampoUsuario_Load" runat="server" />
           <fvc:AdminCtrl runat="server" ID="controles" Modo="Insert"></fvc:AdminCtrl>
            
        </InsertItemTemplate>
        
        <ItemTemplate></ItemTemplate>
       
    </asp:FormView>

</asp:Content>

