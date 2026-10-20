Imports System.Web.Http
Imports System.Web.Http.Cors

Public Module WebApiConfig
    Public Sub Register(config As HttpConfiguration)
        ' CORS: debe ir ANTES de MapHttpAttributeRoutes para que la política global
        ' combine bien con las rutas por atributo.
        Dim corsPolicy = New EnableCorsAttribute("http://localhost:3000", "*", "*")
        ' Content-Disposition lo necesita el frontend para leer el nombre del .xlsx descargado.
        corsPolicy.ExposedHeaders.Add("Content-Disposition")
        config.EnableCors(corsPolicy)

        ' Habilita el enrutamiento por atributos (ej: <Route("api/informes/mensual")>)
        config.MapHttpAttributeRoutes()

        ' Define la convención de rutas clásica: dominio.com/api/Controlador/Id
        config.Routes.MapHttpRoute(
            name:="DefaultApi",
            routeTemplate:="api/{controller}/{id}",
            defaults:=New With {.id = RouteParameter.Optional}
        )

        ' TRUCO PARA VUE.JS: Web API suele devolver XML por defecto en algunos navegadores.
        ' Esta línea obliga a que siempre devuelva JSON, que es lo que Axios necesita.
        config.Formatters.Remove(config.Formatters.XmlFormatter)

        ' Inyección de dependencias (repositorios/servicios) para los controladores
        DependencyConfig.Register(config)

        ' Documentación Swagger/OpenAPI (UI en /swagger)
        SwaggerConfig.Register()
    End Sub
End Module