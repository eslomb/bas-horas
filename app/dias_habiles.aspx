<%@ Page Title="Días hábiles" Language="VB" MasterPageFile="~/masters/App.master" AutoEventWireup="true" 
    CodeFile="dias_habiles.aspx.vb" Inherits="app_diasHabiles" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" Runat="Server">
    <style>
        .panel-abm{
            background: transparent;
            padding: 10px;
        }
        .panel-abm.edicion {
            background: #fafafa;
        }
    </style>
    <h1 class="tituloPagina">Días hábiles por mes</h1>

    <p>
        Permite asignar la cantidad de días hábiles que tiene un mes. <br />
        Este dato permite calcular el costo por tarea en cada mes que lo tenga definido.
    </p>


    <div class="panel">

        <div class="panel-body">

            <div class="row">
                <div class="col-md-4">
                
                    <!-- formulario de alta / edición -->
                    <asp:Panel ID="pnl_abm" runat="server" >
                        
                        <h5><asp:Literal ID="l_modo" runat="server"></asp:Literal></h5>

                        <div style="display: flex; flex-wrap: wrap; gap: 5px;">

                            <p style="flex:1;">
                                <label>Mes</label>
                                <asp:DropDownList ID="ddl_Mes" runat="server" CssClass="form-control" />
                            </p>
                            <p style="flex:1;">
                                <label>Año</label>
                                <asp:TextBox runat="server" ID="tb_anio" CssClass="form-control" placeholder="Año en formato aaaa"></asp:TextBox>
                            </p>
                        </div>
                        <p>
                            <label>Días hábiles</label>
                            <asp:TextBox ID="txtDias" runat="server" CssClass="form-control" MaxLength="2" placeholder="ingresar cantidad" />
                        </p>

                        <p>
                            <asp:Button ID="btn_Guardar" runat="server" CssClass="btn btn-primary" Text="Guardar" /> 
                            <asp:Button ID="btn_Cancelar" runat="server" CssClass="btn btn-secondary" Text="Cancelar"  /> 

                        </p>
                        
                        <asp:Label ID="lblError" runat="server" CssClass="text-danger" EnableViewState="false" />

                    </asp:Panel>
                    
                </div>


                <div class="col-md-4">
                
                    <!-- GRILLA -->
                    <cc:GridView ID="gvMeses" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table"
                        GridLines="None"
                        AllowPaging="true" PageSize="20"
                        UseAccessibleHeader="true">
                        <Columns>

                            <asp:TemplateField HeaderText="Mes / Año">
                                <ItemTemplate>
                                    <%# NombreMes(Eval("Mes"), Eval("Anio")) %> <%#  Eval("Anio") %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="CantDias" HeaderText="Días hábiles" />

                            <asp:TemplateField HeaderText="" ItemStyle-CssClass="acciones">
                                <ItemTemplate>
                                    
                                    <asp:LinkButton runat="server" CausesValidation="False" 
                                        CommandName="Editar" CommandArgument='<%# Eval("Id") %>'
                                        CssClass="btn btn-xs btn-default"><i class="fa fa-pencil"></i></asp:LinkButton> 

                                    <asp:LinkButton runat="server" CausesValidation="False" CommandName="Eliminar" CommandArgument='<%# Eval("Id") %>'
                                        CssClass="btn btn-xs btn-danger" OnClientClick="return confirm('Este registro se eliminará permanentemente');"><i class="fa fa-trash"></i></asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>

                        <EmptyDataTemplate>
                            <div class="bg-info">No hay registros cargados aún.</div>
                        </EmptyDataTemplate>
                    </cc:GridView>

                </div>
            </div>

        </div>        

    </div>
</asp:Content>

