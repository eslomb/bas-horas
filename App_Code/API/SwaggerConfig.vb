Imports System.Web.Http
Imports Swashbuckle.Application

Public Module SwaggerConfig
    Public Sub Register()
        GlobalConfiguration.Configuration.
            EnableSwagger(Sub(c)
                              c.SingleApiVersion("v1", "BASHoras API")

                              ' Permite pegar el token JWT en la UI (botón "Authorize") como header Authorization.
                              ' Se obtiene con POST /api/auth/login. Pegar el valor como: Bearer {token}
                              c.ApiKey("Bearer").
                                  Description("Token JWT obtenido en POST /api/auth/login. Pegar como: Bearer {token}").
                                  Name("Authorization").
                                  In("header")
                          End Sub).
            EnableSwaggerUi(Sub(c) c.EnableApiKeySupport("Authorization", "header"))
    End Sub
End Module
