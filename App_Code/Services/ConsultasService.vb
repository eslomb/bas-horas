Imports System.Linq
Imports DTOs.Consultas
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    Public Class ConsultasService
        Implements IConsultasService

        Private ReadOnly productoRepo As IProductoRepository
        Private ReadOnly actividadRepo As IActividadRepository
        Private ReadOnly centroRepo As ICentroRepository

        Public Sub New(productoRepo As IProductoRepository,
                        actividadRepo As IActividadRepository,
                        centroRepo As ICentroRepository)
            Me.productoRepo = productoRepo
            Me.actividadRepo = actividadRepo
            Me.centroRepo = centroRepo
        End Sub

        Public Function ListarProductos(soloActivos As Boolean) As List(Of ProductoDTO) Implements IConsultasService.ListarProductos
            Dim productos = If(soloActivos, productoRepo.ObtenerActivos(), productoRepo.ObtenerTodos())

            Return (From p In productos
                    Select New ProductoDTO With {
                        .IDPro = p.IDPro,
                        .Codigo = p.Codigo,
                        .Producto = p.Producto,
                        .Activo = p.Activo,
                        .FProyIni = p.FProyIni,
                        .FProyFin = p.FProyFin
                    }).OrderBy(Function(p) p.Producto).ToList()
        End Function

        Public Function ListarActividades(soloHabilitadas As Boolean) As List(Of ActividadDTO) Implements IConsultasService.ListarActividades
            Dim actividades = If(soloHabilitadas, actividadRepo.ObtenerHabilitadas(), actividadRepo.ObtenerTodas())

            Return (From a In actividades
                    Select New ActividadDTO With {
                        .IDAct = a.IDAct,
                        .Codigo = a.Codigo,
                        .Actividad = a.Actividad,
                        .Habilitada = a.Habilitada
                    }).OrderBy(Function(a) a.Actividad).ToList()
        End Function

        Public Function ListarCentros() As List(Of CentroDTO) Implements IConsultasService.ListarCentros
            Return (From c In centroRepo.ObtenerTodos()
                    Select New CentroDTO With {
                        .CodCen = c.CodCen,
                        .Nombre = c.Nombre
                    }).OrderBy(Function(c) c.Nombre).ToList()
        End Function
    End Class
End Namespace
