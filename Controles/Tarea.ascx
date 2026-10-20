<%@ Control Language="VB" AutoEventWireup="false" CodeFile="Tarea.ascx.vb" Inherits="Ctrl_tarea" %>

<div class="row datos-tarea">

    <div runat="server" id="filaDatos" role="alert" >

        <div class="col-sm-2">
            <div class="input-group date">
                <asp:TextBox ID="tb_fecha" runat="server" CssClass="form-control input-sm" MaxLength="10" placeholder="Fecha"></asp:TextBox>
                <span class="input-group-addon btn-sm"><i class=" fa fa-th"></i></span>
                <asp:RequiredFieldValidator ID="rfv_fecha" runat="server" ErrorMessage="Falta completar la fecha" Text="*" Display="None" ControlToValidate="tb_fecha"></asp:RequiredFieldValidator>
            </div>            
        </div>
            
        <div class="col-sm-3">
            <asp:TextBox ID="tb_actividad" runat="server" CssClass="form-control input-sm input-actividad" placeholder="Actividad"></asp:TextBox>
            <asp:HiddenField ID="hf_idActividad" runat="server" />
        </div>
            
        <div class="col-sm-2">
            <asp:TextBox ID="tb_producto" runat="server" CssClass="form-control input-sm input-producto" placeholder="Producto"></asp:TextBox>
            <asp:HiddenField ID="hf_idproducto" runat="server"></asp:HiddenField>
        </div>
            
        <div class="col-sm-1">
            <asp:DropDownList ID="ddl_pais" runat="server" CssClass="form-control input-sm" DataValueField="Codigo" DataTextField="Codigo"></asp:DropDownList>
        </div>
            
        <div class="col-sm-2">
            <div class="form-inline">
                <div class="form-group">
                    <div class="input-group">
                        <asp:TextBox ID="tb_Horas" runat="server" CssClass="form-control input-sm" MaxLength="1" placeholder="h" Text="0"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfv_horas" runat="server" ErrorMessage="Falta completar las horas de la tarea" Text="*" ControlToValidate="tb_Horas" Display="None"></asp:RequiredFieldValidator>
                        <asp:RangeValidator ID="rv_horas" runat="server" Display="None" 
                            ErrorMessage="El numero de horas no puede superar 9" ControlToValidate="tb_Horas" Type="Integer" MaximumValue="9" MinimumValue="0">*</asp:RangeValidator>
                        <ajaxToolkit:FilteredTextBoxExtender ID="ftbe_Horas" runat="server" TargetControlID="tb_Horas" FilterType="Numbers">
                        </ajaxToolkit:FilteredTextBoxExtender>

                        <div class="input-group-addon">:</div>

                        <asp:TextBox ID="tb_minutos" runat="server" CssClass="form-control input-sm" MaxLength="2" placeholder="mm" Text="00"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfv_minutos" runat="server" ErrorMessage="Falta completar los minutos de la tarea" Text="*" ControlToValidate="tb_minutos" Display="None"></asp:RequiredFieldValidator>
                        <asp:RangeValidator ID="rv_minutos" runat="server" Display="None" 
                            ErrorMessage="El numero de minutos no puede superar 59" ControlToValidate="tb_minutos" Type="Integer" MaximumValue="59" MinimumValue="0">*</asp:RangeValidator>
                        <ajaxToolkit:FilteredTextBoxExtender ID="ftbe_minutos" runat="server" TargetControlID="tb_minutos" FilterType="Numbers">
                        </ajaxToolkit:FilteredTextBoxExtender>
                    </div>
                </div>
            </div>
        </div>
        <div class="col-sm-2">
            <asp:LinkButton ID="lb_limpiar" runat="server" CssClass="btn btn-default btn-sm tooltips" ToolTip="limpiar datos de esta tarea" CausesValidation="false"><i class="fa fa-refresh"></i></asp:LinkButton>
            <a class="btn btn-default btn-sm btn_obs tooltips" title="mostrar observaciones"><i class="fa fa-list-alt"></i></a>
        </div>
        
        
        <div class="observaciones">
            <p><label>Observaciones (max: <% Response.Write(maxLengthObs.ToString) %> caracteres)</label>
            <asp:TextBox ID="tb_observaciones" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox></p>
            
        </div>

        <asp:Panel ID="pnl_resultado" runat="server" Visible="false" EnableViewState="false" CssClass="resultado-tarea">
            <h5><asp:Literal ID="l_resultado" runat="server" EnableViewState="false"></asp:Literal></h5>
        </asp:Panel>
    
    </div>
</div>

