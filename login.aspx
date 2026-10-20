<%@ Page Title="Ingreso al sistema" Language="VB" MasterPageFile="~/masters/Anonimos.master" AutoEventWireup="false" 
    CodeFile="login.aspx.vb" Inherits="login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cph_contenido" Runat="Server">
    <div class="row">

        <div class="col-sm-6 col-md-offset-3">

            <h2 class="form-signin-heading">Ingreso al sistema</h2>

            <asp:Login ID="Login2" runat="server" DestinationPageUrl="~/app/redirector.aspx" RenderOuterTable="false">
                <LayoutTemplate>
                    <p><label>Email</label>  
                    <asp:RequiredFieldValidator ID="rfv_Usr" runat="server" ErrorMessage="Completar el Email" Text="*" ControlToValidate="UserName" />
                    <asp:TextBox ID="UserName" runat="server" CssClass="form-control" MaxLength="50" ></asp:TextBox>
                    </p>
                            
                    <p><label>Contraseña</label>  
                    <asp:RequiredFieldValidator ID="rfv_Pwd" runat="server" ErrorMessage="Ingresar la contraseña" Text="*" ControlToValidate="Password" />
                    <asp:TextBox ID="Password" runat="server" TextMode="Password" CssClass="form-control" MaxLength="20"></asp:TextBox>
                    </p>
                            
                           
                             
                    <label><cc:Checkbox ID="RememberMe" runat="server" Text="Recordar"/>
                        <asp:LinkButton ID="lb_norecordar" runat="server" Visible="false" OnClick="lb_norecordarOnClick">Dejar de recordar</asp:LinkButton>
                    </label>
                            
                    <span class="pull-right">
                        <a href="recuperaClave.aspx"> Olvidé mi contraseña</a>
                    </span>
                                
                    <asp:Button ID="btn_ingresar" runat="server" Text="INGRESAR" CssClass="btn btn-lg btn-success btn-submit btn-block" OnClick="btn_ingresar_Click" />
                </LayoutTemplate>
            </asp:Login>
                    
            <asp:Panel ID="pnl_resultado" runat="server" Visible="false" EnableViewState="false" CssClass="alert alert-block alert-danger">
                <button type="button" class="close close-sm" data-dismiss="alert">
                    <i class="glyphicon glyphicon-remove"></i>
                </button>
                <p><asp:Label ID="l_Resultado" runat="server" Text="" EnableViewState="false"></asp:Label></p>                    
            </asp:Panel>  



        </div>
    </div>
</asp:Content>

