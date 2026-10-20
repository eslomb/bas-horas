<%@ Page Title="Usuarios registrados" Language="VB" MasterPageFile="~/masters/App.master" AutoEventWireup="false" 
    CodeFile="usuario_detalle.aspx.vb" Inherits="app_usuario_detalle" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" Runat="Server">
        <h1 class="tituloPagina">Datos de Usuario
            <span class="pull-right">
                <a href="usuarios.aspx" class="btn btn-sm btn-default"><i class="fa fa-users"> Ver usuarios</i></a>
                <asp:LinkButton ID="lb_nuevo" runat="server" CausesValidation="false" CssClass="btn btn-sm btn-primary"><i class="fa fa-user-plus"></i> Nuevo</asp:LinkButton>
            </span>
        </h1>
        <asp:MultiView ID="multiview" runat="server" ActiveViewIndex="0">
    
            <asp:View ID="v_edicion" runat="server">
                <h5>Modificación de datos</h5>
                <div class="row">
                    <div class="col-sm-6">
                        <p>
                            <label>Nombre 
                                <asp:RequiredFieldValidator ID="rfv_edicion_nombre" runat="server" ErrorMessage="Falta definir el nombre" Text="*" ControlToValidate="tb_edicion_nombre"></asp:RequiredFieldValidator></label><br />
                            <asp:TextBox ID="tb_edicion_nombre" runat="server" CssClass="form-control" Enabled="false" />
                        </p>
                        <p>
                            <label>Contraseña (solo si se modifica)</label><br />
                            <asp:TextBox ID="tb_edicion_clave" runat="server" CssClass="form-control" TextMode="Password" />
                        </p>
                        <p>
                            <label>Repetir Contraseña <asp:CompareValidator ID="cv_reclave" runat="server" ErrorMessage="La contraseña no coincide" Text="*" 
                                ControlToValidate="tb_edicion_reclave" ControlToCompare="tb_edicion_clave"></asp:CompareValidator></label><br />
                            <asp:TextBox ID="tb_edicion_reclave" runat="server" CssClass="form-control" TextMode="Password" />
                        </p>
                        <p>
                            <label>E-mail 
                                <asp:RequiredFieldValidator ID="rfv_edicion_email" runat="server" ErrorMessage="Completar el Email" Text="*" 
                                ControlToValidate="tb_edicion_email"></asp:RequiredFieldValidator>
                                <asp:RegularExpressionValidator ID="rev_edicion_email" runat="server" ErrorMessage="El Email no es válido" Text="*" 
                                    ControlToValidate="tb_edicion_email" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
                            </label><br />
                            <asp:TextBox ID="tb_edicion_email" runat="server" CssClass="form-control"></asp:TextBox>
                        </p>
                        
                        <p>
                            <label>Comentarios <span class="badge tooltips" title="No se muestran al usuario">
                                <i class="fa fa-info"></i></span></label><br />
                            <asp:TextBox ID="tb_edicion_comentarios" runat="server" TextMode="MultiLine" Rows="2" CssClass="form-control"></asp:TextBox>
                        </p>
                    </div>
                    <div class="col-sm-6">
                        <p>
                            <label>Legajo</label><br />
                            <asp:TextBox ID="tb_edicion_legajo" runat="server" CssClass="form-control"></asp:TextBox>
                        </p>
                        <p>
                            <label>Dependencia <span class="badge tooltips" title="Indica de qué Legajo depende este usuario"><i class="fa fa-info"></i></span></label>
                            <asp:TextBox ID="tb_edicion_dependencia" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                        </p>
                                                
                        <p>
                            <label>Jornada laboral (hs.)
                                <asp:RangeValidator ID="rv_edicion_modalidad" runat="server" ErrorMessage="Jornada laboral no es un número decimal válido. Debe ser inferior a 9" Text="*" ControlToValidate="tb_edicion_modalidad"
                                    MinimumValue="0" MaximumValue="9" Type="Double" ></asp:RangeValidator>
                            </label><br />
                            <asp:TextBox ID="tb_edicion_modalidad" runat="server" CssClass="form-control" MaxLength="4"></asp:TextBox>
                        </p>
                        
                        <p>
                            <label>Centro</label><br />
                            <asp:DropDownList ID="ddl_edicion_centro" runat="server" CssClass="form-control" AppendDataBoundItems="true" DataTextField="Nombre" DataValueField="CodCen">
                                <asp:ListItem Value="">Sin asignar</asp:ListItem>
                            </asp:DropDownList>
                        </p>
                        
                        <p>
                            <label>Período Abierto (días) <span class="badge tooltips" title="Cuando es 0, los usuarios del grupo Basico tienen permitido cargar durante el mes vigente">
                                <i class="fa fa-info"></i></span>
                                <asp:RangeValidator ID="rv_edicion_periodo" runat="server" ErrorMessage="El período debe ser un número" Text="*" ControlToValidate="tb_edicion_periodoAbierto"
                                    Type="Integer" MinimumValue="0" MaximumValue="999"></asp:RangeValidator></label><br />
                            <asp:TextBox ID="tb_edicion_periodoAbierto" runat="server" CssClass="form-control" MaxLength="3"></asp:TextBox>

                        </p>
                        <p>
                            <label>CUIL</label>
                            <asp:TextBox ID="tb_edicion_CUIL" runat="server" CssClass="form-control" MaxLength="13"></asp:TextBox>
                        </p>

                        <div class="row">
                            <div class="col-sm-6">
                                <p>
                                    <label>Grupos</label><br />
                                    <cc:CheckBoxList ID="cbl_edicion_roles" runat="server" RepeatLayout="Flow" RepeatDirection="Vertical"></cc:CheckBoxList>
                                </p>

                            </div>
                            <div class="col-sm-6">                                              
                                <p>
                                    <label>Habilitado</label><br />
                                    <cc:RadioButtonList ID="rbl_edicion_habilitado" runat="server" RepeatLayout="Flow" RepeatDirection="Horizontal">
                                        <asp:ListItem Value="True" Text="SI"></asp:ListItem>
                                        <asp:ListItem Value="False" Text="NO"></asp:ListItem>
                                    </cc:RadioButtonList>
                                </p>
                                <p><asp:Button ID="btn_desbloquear" runat="server" Text="Desbloquear" CssClass="btn btn-default" Visible="false" /></p>
                            </div>
                        </div>
                    </div>

                </div>
        
                <div class="text-center margin-bottom">
                    <asp:LinkButton ID="lb_edicion_actualizar" runat="server" CssClass="btn btn-success" EnableViewState="false"><i class="fa fa-floppy-o"></i> Modificar datos</asp:LinkButton>
                    <asp:LinkButton ID="lb_edicion_eliminar" runat="server" CssClass="btn btn-danger" EnableViewState="false" CausesValidation="false" OnClientClick="javascript:return confirm('Se eliminará permanentemente este usuario');">
                        <i class="fa fa-trash"></i> Eliminar</asp:LinkButton>                    
                </div>
        
            </asp:View>
    
    
            <asp:View ID="v_alta" runat="server" >
                
                <h5>Ingreso de datos</h5>
                <div class="row">
                    <div class="col-sm-6">
                        <p><label>Nombre 
                                <asp:RequiredFieldValidator ID="rfv_alta_nombre" runat="server" ErrorMessage="Falta definir el nombre" Text="*" ControlToValidate="tb_alta_nombre"></asp:RequiredFieldValidator>
                            </label><br />
                            <asp:TextBox ID="tb_alta_nombre" runat="server" CssClass="form-control"></asp:TextBox>
                        </p>
                        <p>
                            <label>Contraseña 
                                <asp:RequiredFieldValidator ID="rfv_alta_clave" runat="server" ErrorMessage="Falta ingresar una contraseña" Text="*" ControlToValidate="tb_alta_clave"></asp:RequiredFieldValidator>
                                

                            </label><br />
                            <asp:TextBox ID="tb_alta_clave" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                        </p>
                        <p>
                            <label>Repetir Contraseña 
                                <asp:RequiredFieldValidator ID="rfv_alta_reclave" runat="server" ErrorMessage="Falta repetir contraseña" Text="*" ControlToValidate="tb_alta_reclave"></asp:RequiredFieldValidator>
                                <asp:CompareValidator ID="cv_alta_reclave" runat="server" ErrorMessage="La contraseña no coincide" Text="*" 
                                ControlToValidate="tb_alta_reclave" ControlToCompare="tb_alta_clave"></asp:CompareValidator></label><br />
                            <asp:TextBox ID="tb_alta_reclave" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                        </p>
                        <p>
                            <label>E-mail 
                                <asp:RequiredFieldValidator ID="rfv_alta_email" runat="server" ErrorMessage="Completar el Email" Text="*" 
                                ControlToValidate="tb_alta_email"></asp:RequiredFieldValidator>
                                <asp:RegularExpressionValidator ID="re_alta_email" runat="server" ErrorMessage="El Email no es válido" Text="*" 
                                    ControlToValidate="tb_alta_email" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
                            </label><br />
                            <asp:TextBox ID="tb_alta_email" runat="server" CssClass="form-control"></asp:TextBox>
                        </p>
                        
                        <p>
                            <label>Comentarios <span class="badge tooltips" title="No se muestran al usuario">
                                <i class="fa fa-info"></i></span></label><br />
                            <asp:TextBox ID="tb_alta_comentarios" runat="server" TextMode="MultiLine" Rows="2" CssClass="form-control"></asp:TextBox>
                        </p>
                        
                    </div>

                    <div class="col-sm-6">
                        <p>
                            <label>Legajo</label><br />
                            <asp:TextBox ID="tb_alta_legajo" runat="server" CssClass="form-control"></asp:TextBox>
                        </p>

                        <p>
                            <label>Dependencia <span class="badge tooltips" title="Indica de qué Legajo depende este usuario"><i class="fa fa-info"></i></span></label>
                            <asp:TextBox ID="tb_alta_dependencia" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                        </p>
                                                
                        <p>
                            <label>Jornada laboral (hs.)
                                <asp:RangeValidator ID="rv_alta_modalidad" runat="server" ErrorMessage="Jornada laboral no es un número decimal válido. Debe ser inferior a 9" Text="*" ControlToValidate="tb_alta_modalidad"
                                    MinimumValue="0" MaximumValue="9" Type="Double" ></asp:RangeValidator>
                            </label><br />
                            <asp:TextBox ID="tb_alta_modalidad" runat="server" CssClass="form-control" MaxLength="4"></asp:TextBox>
                        </p>
                        
                        <p>
                            <label>Centro</label><br />
                            <asp:DropDownList ID="ddl_alta_centro" runat="server" CssClass="form-control" AppendDataBoundItems="true" DataTextField="Nombre" DataValueField="CodCen">
                                <asp:ListItem Value="">Sin asignar</asp:ListItem>
                            </asp:DropDownList>
                        </p>
                        
                        <p>
                            <label>Período Abierto (días) <span class="badge tooltips" title="Cuando es 0, los usuarios del grupo Basico tienen permitido cargar durante el mes vigente">
                                <i class="fa fa-info"></i></span>
                                <asp:RangeValidator ID="rv_alta_periodo" runat="server" ErrorMessage="El período debe ser un número" Text="*" ControlToValidate="tb_alta_periodoAbierto"
                                    Type="Integer" MinimumValue="0" MaximumValue="999"></asp:RangeValidator>
                            </label><br />
                            <asp:TextBox ID="tb_alta_periodoAbierto" runat="server" CssClass="form-control" MaxLength="3"></asp:TextBox>
                            
                        </p>
                        <p>
                            <label>CUIL</label>
                            <asp:TextBox ID="tb_alta_CUIL" runat="server" CssClass="form-control" MaxLength="13"></asp:TextBox>
                        </p>


                        <div class="row">
                            <div class="col-sm-6">

                                <p>
                                    <label>Grupos</label><br />
                                    <cc:CheckBoxList ID="cbl_alta_roles" runat="server"  RepeatLayout="Flow" RepeatDirection="Vertical">
                                    </cc:CheckBoxList>
                                </p>
                            </div>
                            <div class="col-sm-6">
                                <p>
                                    <label>Habilitado</label><br />
                                    <cc:RadioButtonList ID="rbl_alta_habilitado" runat="server" RepeatLayout="Flow" RepeatDirection="Horizontal">
                                        <asp:ListItem Value="True" Text="SI" Selected="true"></asp:ListItem>
                                        <asp:ListItem Value="False" Text="NO"></asp:ListItem>
                                    </cc:RadioButtonList>
                                </p>
                            </div>
                        </div>
                        
                    </div>
                </div>
                
                <div class="text-center margin-bottom">
                    <asp:LinkButton ID="lb_alta_aceptar" runat="server" CssClass="btn btn-success" EnableViewState="false"><i class="fa fa-floppy-o"></i> Crear usuario</asp:LinkButton>
                    <asp:LinkButton ID="lb_alta_cancelar" runat="server" CssClass="btn btn-danger" EnableViewState="false" CausesValidation="false"><i class="fa fa-times"></i> Cancelar</asp:LinkButton>
                    
                </div>
         
        
            </asp:View>
        </asp:MultiView>

    
</asp:Content>