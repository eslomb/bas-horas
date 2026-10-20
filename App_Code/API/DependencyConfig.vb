Imports System.Web.Http
Imports Autofac
Imports Autofac.Integration.WebApi
Imports Repositories.EF
Imports Repositories.Email
Imports Repositories.Interfaces
Imports Repositories.Membership
Imports Services
Imports Services.Interfaces

Public Module DependencyConfig
    Public Sub Register(config As HttpConfiguration)
        Dim builder As New ContainerBuilder()

        builder.RegisterApiControllers(GetType(ConsultasController).Assembly)

        builder.RegisterType(Of BASHorasEntities)().InstancePerRequest()

        builder.RegisterType(Of ProductoRepository)().As(Of IProductoRepository)().InstancePerRequest()
        builder.RegisterType(Of ActividadRepository)().As(Of IActividadRepository)().InstancePerRequest()
        builder.RegisterType(Of PaisRepository)().As(Of IPaisRepository)().InstancePerRequest()
        builder.RegisterType(Of CentroRepository)().As(Of ICentroRepository)().InstancePerRequest()
        builder.RegisterType(Of RegistroRepository)().As(Of IRegistroRepository)().InstancePerRequest()
        builder.RegisterType(Of ReporteGeneralRepository)().As(Of IReporteGeneralRepository)().InstancePerRequest()
        builder.RegisterType(Of ReporteGuardadoRepository)().As(Of IReporteGuardadoRepository)().InstancePerRequest()
        builder.RegisterType(Of RemuneracionRepository)().As(Of IRemuneracionRepository)().InstancePerRequest()
        builder.RegisterType(Of UsuarioRepository)().As(Of IUsuarioRepository)().InstancePerRequest()
        builder.RegisterType(Of NotificadorUsuarios)().As(Of INotificadorUsuarios)().InstancePerRequest()

        builder.RegisterType(Of ConsultasService)().As(Of IConsultasService)().InstancePerRequest()
        builder.RegisterType(Of PaisService)().As(Of IPaisService)().InstancePerRequest()
        builder.RegisterType(Of RegistroService)().As(Of IRegistroService)().InstancePerRequest()
        builder.RegisterType(Of ReporteGeneralService)().As(Of IReporteGeneralService)().InstancePerRequest()
        builder.RegisterType(Of ReporteExportacionService)().As(Of IReporteExportacionService)().InstancePerRequest()
        builder.RegisterType(Of ReporteGuardadoService)().As(Of IReporteGuardadoService)().InstancePerRequest()
        builder.RegisterType(Of UsuarioService)().As(Of IUsuarioService)().InstancePerRequest()

        Dim container = builder.Build()
        config.DependencyResolver = New AutofacWebApiDependencyResolver(container)
    End Sub
End Module
