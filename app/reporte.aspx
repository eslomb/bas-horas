<%@ Page Title="Detalle del reporte" Language="VB" MasterPageFile="~/masters/App.master" AutoEventWireup="false"
    CodeFile="reporte.aspx.vb" Inherits="app_reporte" EnableEventValidation="false" %>

<%@ Register TagPrefix="cc" TagName="Tarea" Src="~/Controles/Tarea.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" runat="Server">
    <script>
        $(function () {
            $('#seleccionar_todos').click(function (e) {
                var table = $(e.target).closest('table');
                $('td input:checkbox', table).prop('checked', this.checked);
            });
        })
    </script>
    
    <asp:ObjectDataSource ID="ods_usuarios" runat="server" SelectMethod="listarUsuarios" TypeName="Aplicacion.Logica">
    </asp:ObjectDataSource>
    
    <asp:ObjectDataSource ID="ods_centros" runat="server" SelectMethod="listarCentros" TypeName="Aplicacion.Logica">
    </asp:ObjectDataSource>

    <asp:ObjectDataSource ID="ods_listado" runat="server" DeleteMethod="eliminarRegistro" SelectMethod="reporteGeneral" TypeName="Aplicacion.Logica">
        <DeleteParameters>
            <asp:Parameter Name="idReg" Type="Int32" />
        </DeleteParameters>
        <SelectParameters>
            <asp:Parameter Name="Desde" Type="String" ConvertEmptyStringToNull="true" />
            <asp:Parameter Name="Hasta" Type="String" ConvertEmptyStringToNull="true" />
            <asp:Parameter Name="Usuario" Type="String" ConvertEmptyStringToNull="true" />
            <asp:Parameter Name="IdProd" Type="Int32" ConvertEmptyStringToNull="true" />
            <asp:Parameter Name="IdAct" Type="Int32" ConvertEmptyStringToNull="true" />
            <asp:Parameter Name="CodPais" Type="String" ConvertEmptyStringToNull="true" />
            <asp:Parameter Name="Centro" Type="String" ConvertEmptyStringToNull="true" />
        </SelectParameters>
    </asp:ObjectDataSource>

    <h1 class="tituloPagina">Detalle del reporte <small><asp:Literal ID="l_tituloReporte" runat="server"></asp:Literal></small></h1>
    
    <p>
        <label for="ddlPageSize">Registros por página: </label>
        <asp:DropDownList ID="ddlPageSize" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged">
            <asp:ListItem Text="10" Value="10"></asp:ListItem>
            <asp:ListItem Text="20" Value="20" Selected="True"></asp:ListItem>
            <asp:ListItem Text="50" Value="50"></asp:ListItem>
            <asp:ListItem Text="100" Value="100"></asp:ListItem>
        </asp:DropDownList>
    </p>

    <cc:GridView ID="gv_reporte" runat="server" AutoGenerateColumns="false" DataKeyNames="IDReg" AllowPaging="true" AllowSorting="false" PageSize="10">
        <Columns>
            <asp:TemplateField>
                <HeaderTemplate>
                    <input type="checkbox" id="seleccionar_todos" />
                </HeaderTemplate>
                <ItemTemplate>
                    <asp:CheckBox ID="cb_eliminar" runat="server" CssClass="registro_cb" />
                    <asp:HiddenField ID="hf_eliminar" runat="server" Value='<%# Eval("IDReg") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="FECHA_ACTIVIDAD" DataFormatString="{0:d}" HeaderText="Fecha" />
            <asp:BoundField DataField="USUARIO" HeaderText="Usuario" />
            <asp:TemplateField HeaderText="Legajo">
                <ItemTemplate>
                    <asp:Literal runat="server" Text='<%# verLegajo(Eval("USUARIO")) %>'></asp:Literal>
                </ItemTemplate>
            </asp:TemplateField>            
            <asp:TemplateField HeaderText="Centro">
                <ItemTemplate>
                    <asp:Literal runat="server" Text='<%# verCentro(Eval("USUARIO")) %>'></asp:Literal>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="COD_ACTIVIDAD" HeaderText="Cod"  />
            <asp:BoundField DataField="ACTIVIDAD" HeaderText="Actividad"  />
            <asp:BoundField DataField="PAIS" HeaderText="Pais"  />
            <asp:BoundField DataField="PRODUCTO" HeaderText="Producto"  />
            <asp:BoundField DataField="HORAS" HeaderText="Horas" />
            <asp:BoundField DataField="MINUTOS" HeaderText="Minutos" />
            <asp:TemplateField HeaderStyle-CssClass="columna-operaciones">
                <ItemTemplate>
                    <asp:HyperLink runat="server" ID="Observaciones" ToolTip='<%# Eval("Observaciones") %>' 
                        Visible='<%# If(String.IsNullOrEmpty(Eval("Observaciones")), False, True) %>' 
                        CssClass="btn btn-sm">
                        <i class="text-primary fa fa-comment"></i>
                    </asp:HyperLink>

                    <asp:LinkButton ID="lb_editar" runat="server"  CssClass="btn btn-sm btn-info tooltips" CausesValidation="false" ToolTip="Modificar"
                        OnLoad="lb_editar_Load" OnClick="lb_editar_Click" 
                        CommandArgument='<%# Eval("IDReg") %>'>
                        <i class="fa fa-pencil"></i></asp:LinkButton>
                    
                    <asp:LinkButton ID="lb_eliminar" runat="server" CssClass="btn btn-sm btn-danger tooltips" CausesValidation="False" ToolTip="Eliminar"
                        CommandName="Eliminar" CommandArgument='<%# Eval("IDReg") %>' 
                        OnClick="lb_eliminar_Click" OnClientClick="return confirm('Esta tarea se eliminará permanentemente');">
                        <i class="fa fa-trash"></i></asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
            <h5>No hay tareas para mostrar</h5>
        </EmptyDataTemplate>
    </cc:GridView>
    
    <asp:GridView ID="gv_export" runat="server" AutoGenerateColumns="false" AllowPaging="false" AllowSorting="false" EnableViewState="false" >
        <Columns>
            <asp:BoundField DataField="FECHA_ACTIVIDAD" DataFormatString="{0:d}" HeaderText="Fecha" />
            <asp:BoundField DataField="USUARIO" HeaderText="Usuario"  HtmlEncode="false"  />
            <asp:TemplateField HeaderText="Legajo">
                <ItemTemplate>
                    <asp:Literal runat="server" Text='<%# verLegajo(Eval("USUARIO")) %>'></asp:Literal>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Jornada">
                <ItemTemplate>
                    <asp:Literal runat="server" Text='<%# verModalidad(Eval("USUARIO")) %>'></asp:Literal>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Dias.habil">
                <ItemTemplate>
                    <asp:Literal ID="l_diasHabiles" runat="server" Text='<%# CantidadDiasHabiles(Eval("FECHA_ACTIVIDAD")) %>'></asp:Literal>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Centro">
                <ItemTemplate>
                    <asp:Literal runat="server" Text='<%# verCentro(Eval("USUARIO")) %>'></asp:Literal>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="COD_ACTIVIDAD" HeaderText="Cod"  />
            <asp:BoundField DataField="ACTIVIDAD" HeaderText="Actividad"  />
            <asp:BoundField DataField="PAIS" HeaderText="Pais"  />
            <asp:BoundField DataField="PRODUCTO" HeaderText="Producto"  />
            <asp:BoundField DataField="HORAS" HeaderText="Horas" />
            <asp:BoundField DataField="MINUTOS" HeaderText="Minutos" />
            <asp:BoundField DataField="Duracion" HeaderText="Durac.tarea" />
            <asp:BoundField DataField="REMUNERACION" HeaderText="Remuneracion"  />
            <asp:BoundField DataField="Costo" HeaderText="Costo Tarea" DataFormatString="{0:N}"  />
            <asp:BoundField DataField="OBSERVACIONES" HeaderText="Observaciones"  /><asp:TemplateField HeaderText="CUIL">
                <ItemTemplate>
                    <asp:Literal runat="server" Text='<%# verCUIL(Eval("USUARIO")) %>'></asp:Literal>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>


    <asp:Panel runat="server" ID="pnl_controles" CssClass="well text-center">
        <p>
            <asp:LinkButton ID="lb_exportar" runat="server" CausesValidation="false" CssClass="btn btn-success"><i class="fa fa-file-excel-o"></i> Exportar a Excel</asp:LinkButton>
            <asp:LinkButton ID="lb_eliminarSeleccion" runat="server" CausesValidation="false" CssClass="btn btn-danger"
                OnClientClick="return confirm('Se intentará eliminar las tareas seleccionadas permanentemente');"><i class="fa fa-trash"></i> Eliminar seleccionados</asp:LinkButton>
        </p>
    </asp:Panel>
    

    <asp:Panel runat="server" ID="pnl_edicion">
            <cc:Tarea runat="server" ID="ctrlTarea" />
        <div class="row">
            <div class="col-xs-12">
                <asp:LinkButton ID="lb_actualizar" runat="server" CssClass="btn btn-success"><i class="glyphicon glyphicon-ok"></i> Aceptar</asp:LinkButton>
                <asp:LinkButton ID="lb_cancelarActualizacion" runat="server" CausesValidation="false" CssClass="btn btn-danger">Cancelar</asp:LinkButton>
            </div>

        </div>
    </asp:Panel>
</asp:Content>



