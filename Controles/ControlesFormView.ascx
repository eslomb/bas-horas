<%@ Control Language="VB" AutoEventWireup="false" CodeFile="ControlesFormView.ascx.vb" Inherits="Controles_ControlesFormView" %>


<div class="well well-small controles-abm">
<asp:LinkButton ID="InsertButton" runat="server" CausesValidation="True" CommandName="Insert" CssClass="btn btn-success" Visible="false">
    <i class="fa fa-check"></i> Aceptar</asp:LinkButton> 
<asp:LinkButton ID="UpdateButton" runat="server" CausesValidation="True" CommandName="Update" CssClass="btn btn-success" Visible="false">
    <i class="fa fa-check"></i> Aceptar</asp:LinkButton> 

<asp:LinkButton ID="CancelButton" runat="server" CausesValidation="False" CommandName="Cancel" CssClass="btn btn-danger">
    <i class="fa fa-times"></i> Cancelar</asp:LinkButton>
</div>     