<%@ Page Title="Restablecer mi clave" Language="VB" MasterPageFile="~/masters/Anonimos.master" AutoEventWireup="false"
    CodeFile="restablecerClave.aspx.vb" Inherits="restablecerClave" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" Runat="Server">
    <div class="row">

        <div class="col-sm-6 col-md-offset-3">

            <h2 class="form-signin-heading">Nueva contraseña</h2>

            <asp:MultiView ID="mv_estado" runat="server" ActiveViewIndex="0">

                <asp:View ID="v_formulario" runat="server">
                    <asp:Panel ID="pnl_formulario" runat="server" DefaultButton="btn_Guardar">
                        <p>Elegí tu nueva contraseña. Tiene que tener al menos <asp:Literal ID="l_minimo" runat="server" /> caracteres.</p>

                        <p><label>Nueva contraseña</label>
                            <asp:RequiredFieldValidator ID="rfv_nueva" runat="server" Text="*" ErrorMessage="Ingresá la nueva contraseña" ControlToValidate="tb_nueva" Display="Dynamic" />
                            <asp:TextBox ID="tb_nueva" runat="server" TextMode="Password" CssClass="form-control" MaxLength="128"></asp:TextBox>
                        </p>

                        <p><label>Repetir contraseña</label>
                            <asp:RequiredFieldValidator ID="rfv_repetir" runat="server" Text="*" ErrorMessage="Repetí la nueva contraseña" ControlToValidate="tb_repetir" Display="Dynamic" />
                            <asp:CompareValidator ID="cv_repetir" runat="server" Text="Las contraseñas no coinciden" ErrorMessage="Las contraseñas no coinciden" ControlToValidate="tb_repetir" ControlToCompare="tb_nueva" Display="Dynamic" />
                            <asp:TextBox ID="tb_repetir" runat="server" TextMode="Password" CssClass="form-control" MaxLength="128"></asp:TextBox>
                        </p>

                        <cc:SubmitButton ID="btn_Guardar" runat="server" Text="Guardar"/>
                    </asp:Panel>

                    <asp:Panel ID="pnl_error" runat="server" Visible="false" EnableViewState="false" CssClass="alert alert-block alert-danger">
                        <p><asp:Literal ID="l_error" runat="server" EnableViewState="false" /></p>
                    </asp:Panel>
                </asp:View>

                <asp:View ID="v_tokenInvalido" runat="server">
                    <div class="alert alert-block alert-danger">
                        <p>El link no es válido, ya se usó o venció.</p>
                    </div>
                    <p><asp:HyperLink ID="hl_recuperar" runat="server" NavigateUrl="~/recuperaClave.aspx">Pedir un link nuevo</asp:HyperLink></p>
                </asp:View>

                <asp:View ID="v_listo" runat="server">
                    <div class="alert alert-block alert-success">
                        <p>Tu contraseña se cambió correctamente.</p>
                    </div>
                    <p><asp:HyperLink ID="hl_login" runat="server" NavigateUrl="~/login.aspx">Ingresar al sistema</asp:HyperLink></p>
                </asp:View>

            </asp:MultiView>

        </div>
    </div>
</asp:Content>
