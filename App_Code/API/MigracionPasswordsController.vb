Imports System.Configuration
Imports System.Net
Imports System.Web.Http
Imports Auth
Imports Services.Interfaces

' Migración única de contraseñas Clear -> Hashed. Solo existe mientras appSettings MigracionPasswordsHabilitada
' = true (si no, 404). Borrar este controller (y su service/repo) cuando todos los ambientes estén migrados.
<JwtAuthorize(Roles:="Administrador")>
Public Class MigracionPasswordsController
    Inherits ControladorApiBase

    Private ReadOnly servicio As IMigracionPasswordService

    Public Sub New(servicio As IMigracionPasswordService)
        Me.servicio = servicio
    End Sub

    <HttpPost>
    <Route("api/admin/migrar-passwords")>
    Public Function Migrar() As IHttpActionResult
        Dim habilitada As Boolean
        If Not Boolean.TryParse(ConfigurationManager.AppSettings("MigracionPasswordsHabilitada"), habilitada) OrElse Not habilitada Then
            Return NotFound()
        End If

        If Not servicio.ProviderEsHashed() Then
            Return Content(HttpStatusCode.Conflict, New With {.mensaje = "El provider de Membership no está en passwordFormat=""Hashed"": configurarlo antes de migrar."})
        End If

        Return Ok(servicio.Migrar())
    End Function
End Class
