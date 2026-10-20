<%@ Page Title="Recuperar mi clave" Language="VB" MasterPageFile="~/masters/Anonimos.master" AutoEventWireup="false" 
    CodeFile="recuperaClave.aspx.vb" Inherits="recuperaClave" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" Runat="Server">
    <div class="row">

        <div class="col-sm-6 col-md-offset-3">

            <h2 class="form-signin-heading">Recuperación de contraseña</h2>
            
            <p>Si no recordas tu contraseña para ingresar, ingresá el tu dirección de correo.<br>
            Una vez verificada, se enviará un mensaje con la misma.</p>
            
            <p><asp:HyperLink ID="hl_inicio" runat="server" NavigateUrl="~/login.aspx">Ya conozco mi contraseña y quiero ingresar.</asp:HyperLink></p>

                                
                    <p><label>E-mail</label>  
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" Text="*" ErrorMessage="Ingresa tu Email" ControlToValidate="tb_email"></asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ErrorMessage="El Email ingresado no es válido" ControlToValidate="tb_email" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*">*</asp:RegularExpressionValidator>
                        <br>
                        <asp:TextBox ID="tb_email" runat="server" CssClass="form-control" MaxLength="100"></asp:TextBox>
                    </p>
            
                    <cc:SubmitButton ID="btn_Enviar" runat="server" Text="Aceptar"/>
                
                    <asp:Panel ID="pnl_resultado" runat="server" Visible="false" EnableViewState="false" CssClass="alert alert-block alert-danger fade in">
                        <button type="button" class="close close-sm" data-dismiss="alert">
                          <i class="fa fa-times"></i>
                        </button>
                 
                        <asp:MultiView ID="mv_respuestas" runat="server">
                    
                            <asp:View runat="server" ID="v_EmailNoExiste">
                                <p>El Email ingresado no se encuentra en el sistema.</p>
                            </asp:View>

                            <asp:View runat="server" ID="v_usuarioInactivo">
                                <p>Esta cuenta se encuentra deshabilitada y no puede ser usada.</p>
                            </asp:View>
                    
                            <asp:View runat="server" ID="v_cuentaBloqueada">
                                <!-- La cuenta esta bloqueada -->
                                <p>La cuenta se encuentra bloqueada. <br />
                                Deberás reportarlo para que un administrador la habilite nuevamente.</p>
                            </asp:View>
                                        
                            <asp:View runat="server" ID="v_claveGenerada">
                                <!-- clave generada y enviada -->
                                <p>En instantes recibirás un correo con la clave.</p>
                                <asp:Literal ID="l_resultado" runat="server" EnableViewState="false"></asp:Literal>
                            </asp:View>
                        </asp:MultiView>
                    </asp:Panel>  


        </div>
    </div>
</asp:Content>

