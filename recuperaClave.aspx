<%@ Page Title="Recuperar mi clave" Language="VB" MasterPageFile="~/masters/Anonimos.master" AutoEventWireup="false"
    CodeFile="recuperaClave.aspx.vb" Inherits="recuperaClave" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" Runat="Server">
    <div class="row">

        <div class="col-sm-6 col-md-offset-3">

            <h2 class="form-signin-heading">Recuperación de contraseña</h2>

            <p>Si no recordás tu contraseña, ingresá tu dirección de correo.<br>
            Te vamos a enviar un link para que elijas una nueva.</p>

            <p><asp:HyperLink ID="hl_inicio" runat="server" NavigateUrl="~/login.aspx">Ya conozco mi contraseña y quiero ingresar.</asp:HyperLink></p>

            <asp:Panel ID="pnl_formulario" runat="server" DefaultButton="btn_Enviar">
                <p><label>E-mail</label>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" Text="*" ErrorMessage="Ingresa tu Email" ControlToValidate="tb_email"></asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ErrorMessage="El Email ingresado no es válido" ControlToValidate="tb_email" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*">*</asp:RegularExpressionValidator>
                    <br>
                    <asp:TextBox ID="tb_email" runat="server" CssClass="form-control" MaxLength="100"></asp:TextBox>
                </p>

                <cc:SubmitButton ID="btn_Enviar" runat="server" Text="Aceptar"/>
            </asp:Panel>

            <asp:Panel ID="pnl_enviado" runat="server" Visible="false" EnableViewState="false" CssClass="alert alert-block alert-success">
                <p>Si el email está registrado, en instantes vas a recibir un correo con un link para restablecer la contraseña.</p>
                <p>El link sirve una sola vez y vence a los <asp:Literal ID="l_minutos" runat="server" EnableViewState="false" /> minutos. Si no llega, revisá la carpeta de correo no deseado.</p>
            </asp:Panel>

            <asp:Panel ID="pnl_error" runat="server" Visible="false" EnableViewState="false" CssClass="alert alert-block alert-danger">
                <p>No se pudo enviar el correo. Intentá nuevamente más tarde.</p>
            </asp:Panel>

        </div>
    </div>
</asp:Content>
