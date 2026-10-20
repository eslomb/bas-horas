<%@ Page Title="Mi contraseña" Language="VB" MasterPageFile="~/masters/App.master" AutoEventWireup="false" 
    CodeFile="password.aspx.vb" Inherits="app_password" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" Runat="Server">
    <h1 class="tituloPagina">Modificación de contraseña</h1>
    <p>Completando los siguientes datos podrás modificar la contraseña con la que ingresas al sitio.<br />
    Recordar que se diferencian mayúsculas de minúsculas.</p>
    <br />
    
    <fieldset>
        <div class="row">
            <div class="col-md-4">

                <p><label>Contraseña actual 
                        <asp:RequiredFieldValidator ID="rfv1" runat="server" ErrorMessage="Completar la contraseña actual" ControlToValidate="tb_claveActual">*</asp:RequiredFieldValidator></label> 
                    <asp:TextBox ID="tb_claveActual" runat="server" TextMode="Password" MaxLength="15" CssClass="form-control"></asp:TextBox></p>
        
                <p><label>Nueva contraseña 
                    <asp:RequiredFieldValidator ID="rfv2" runat="server" ErrorMessage="Ingresar una nueva contraseña" ControlToValidate="tb_claveNueva">*</asp:RequiredFieldValidator></label> 
                    <asp:TextBox ID="tb_claveNueva" runat="server" TextMode="Password" MaxLength="15" CssClass="form-control"></asp:TextBox></p>
        
                <p><label>Confirmar contraseña <asp:RequiredFieldValidator ID="rfv3" runat="server" ErrorMessage="Falta confirmar la contraseña" ControlToValidate="tb_claveConfirm">*</asp:RequiredFieldValidator>
                    <asp:CompareValidator ID="compval" runat="server" ErrorMessage="No conincide las contraseñas" ControlToValidate="tb_claveConfirm" ControlToCompare="tb_claveNueva">*</asp:CompareValidator>
                    </label> 
                    <asp:TextBox ID="tb_claveConfirm" runat="server" TextMode="Password" MaxLength="15" CssClass="form-control"></asp:TextBox></p>
        
        
        
                <p><cc:SubmitButton ID="btnAceptar" runat="server" Text="Aceptar" CssClass="btn btn-success" /></p>
            </div>
        </div>
    </fieldset>
</asp:Content>

