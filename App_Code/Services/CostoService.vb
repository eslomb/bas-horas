Imports System.Linq
Imports DTOs.Costos
Imports Repositories.Interfaces
Imports Services.Interfaces
Imports Utils

Namespace Services
    ' Costo de las horas cargadas por actividad (código de tarea) o por centro, de todos los usuarios, en un período.
    ' Cada registro se valoriza igual que la columna "Costo Tarea" de los reportes (Utils.CostoTarea).
    ' Acceso: solo RemuneracionesAuthorize (lo aplica CostosController).
    Public Class CostoService
        Implements ICostoService

        ' Tope del período: se trae cada registro y se lee el Profile de cada usuario del período.
        Private Const MaxDiasPeriodo As Integer = 366

        Private ReadOnly registros As IRegistroRepository
        Private ReadOnly actividades As IActividadRepository
        Private ReadOnly centros As ICentroRepository
        Private ReadOnly usuarios As IUsuarioRepository
        Private ReadOnly remuneraciones As IRemuneracionRepository
        Private ReadOnly diasHabiles As IDiaHabilRepository

        ' Lo mínimo de cada registro para valorizarlo (no se usa spReporteGeneral: trae los textos de cada fila,
        ' une con REMUNERACIONES y ordena, nada de lo cual hace falta acá).
        Private Class FilaCosto
            Public Property Usuario As String
            Public Property Fecha As Date
            Public Property IdActividad As Integer
            Public Property Horas As Integer
            Public Property Minutos As Integer
        End Class

        ' Lo que se usa del Profile de cada usuario.
        Private Class DatosPerfil
            Public Jornada As Decimal
            Public Centro As String
        End Class

        Public Sub New(registros As IRegistroRepository,
                       actividades As IActividadRepository,
                       centros As ICentroRepository,
                       usuarios As IUsuarioRepository,
                       remuneraciones As IRemuneracionRepository,
                       diasHabiles As IDiaHabilRepository)
            Me.registros = registros
            Me.actividades = actividades
            Me.centros = centros
            Me.usuarios = usuarios
            Me.remuneraciones = remuneraciones
            Me.diasHabiles = diasHabiles
        End Sub

        Public Function Costos(desde As Date?, hasta As Date?, agrupacion As String) As ResultadoCostos Implements ICostoService.Costos
            Dim agrupar As AgrupacionCostos
            Select Case If(agrupacion, "").Trim().ToLowerInvariant()
                Case "", "actividad" : agrupar = AgrupacionCostos.Actividad
                Case "centro" : agrupar = AgrupacionCostos.Centro
                Case Else : Return Invalido("La agrupación debe ser ""actividad"" o ""centro"".")
            End Select

            If Not desde.HasValue OrElse Not hasta.HasValue Then Return Invalido("Debe indicar las fechas desde y hasta.")
            Dim d = desde.Value.Date
            Dim h = hasta.Value.Date
            If d > h Then Return Invalido("La fecha desde no puede ser posterior a la fecha hasta.")
            If (h - d).TotalDays + 1 > MaxDiasPeriodo Then Return Invalido("El período no puede superar los " & MaxDiasPeriodo & " días.")

            ' Todos los usuarios, productos, actividades y países; la proyección hace que SQL devuelva solo estas columnas.
            Dim filas = registros.ObtenerTodosEnRango(d, h).
                Select(Function(r) New FilaCosto With {
                    .Usuario = r.Usuario, .Fecha = r.Fecha, .IdActividad = r.idActividad, .Horas = r.Horas, .Minutos = r.Minutos
                }).
                ToList()
            For Each f In filas
                If f.Usuario IsNot Nothing Then f.Usuario = f.Usuario.Trim()
            Next

            Dim dias = CargarDiasHabiles(d, h)
            Dim remuneracionesPorMes = CargarRemuneraciones(d, h)
            Dim perfiles = CargarPerfiles(filas)

            ' Código y nombre del grupo de cada registro.
            Dim grupoDe As Func(Of FilaCosto, Tuple(Of String, String))
            Dim sinGrupo As String
            If agrupar = AgrupacionCostos.Centro Then
                sinGrupo = "(sin centro)"
                Dim nombres = centros.ObtenerTodos().
                    Select(Function(c) New With {c.CodCen, c.Nombre}).
                    ToList().
                    GroupBy(Function(c) If(c.CodCen, "").Trim(), StringComparer.OrdinalIgnoreCase).
                    ToDictionary(Function(g) g.Key, Function(g) g.First().Nombre, StringComparer.OrdinalIgnoreCase)
                grupoDe = Function(f)
                              Dim p As DatosPerfil = Nothing
                              Dim codigo = If(f.Usuario IsNot Nothing AndAlso perfiles.TryGetValue(f.Usuario, p), p.Centro, "")
                              Dim nombre As String = Nothing
                              nombres.TryGetValue(codigo, nombre)
                              Return Tuple.Create(codigo, nombre)
                          End Function
            Else
                sinGrupo = "(sin actividad)"
                Dim catalogo = actividades.ObtenerTodas().
                    Select(Function(a) New With {a.IDAct, a.Codigo, a.Actividad}).
                    ToList().
                    ToDictionary(Function(a) a.IDAct)
                grupoDe = Function(f)
                              If Not catalogo.ContainsKey(f.IdActividad) Then Return Tuple.Create("", CStr(Nothing))
                              Dim act = catalogo(f.IdActividad)
                              Return Tuple.Create(If(act.Codigo, "").Trim(), act.Actividad)
                          End Function
            End If

            Dim sinCosto As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim porGrupo As New Dictionary(Of String, CostoItemDTO)(StringComparer.OrdinalIgnoreCase)

            For Each f In filas
                Dim grupo = grupoDe(f)
                Dim codigo = grupo.Item1
                Dim item As CostoItemDTO = Nothing
                If Not porGrupo.TryGetValue(codigo, item) Then
                    item = New CostoItemDTO With {
                        .Codigo = If(codigo = "", sinGrupo, codigo),
                        .Nombre = grupo.Item2
                    }
                    porGrupo(codigo) = item
                End If

                Dim fecha = f.Fecha
                Dim diasMes As Integer? = Nothing
                Dim cant As Integer
                If dias.TryGetValue(ClaveMes(fecha.Year, fecha.Month), cant) Then diasMes = cant

                Dim remuneracion As Decimal? = Nothing
                Dim monto As Decimal
                If f.Usuario IsNot Nothing AndAlso remuneracionesPorMes.TryGetValue(ClaveRemuneracion(f.Usuario, fecha.Year, fecha.Month), monto) Then
                    remuneracion = monto
                End If

                Dim jornada As Decimal? = Nothing
                Dim perfil As DatosPerfil = Nothing
                If f.Usuario IsNot Nothing AndAlso perfiles.TryGetValue(f.Usuario, perfil) Then jornada = perfil.Jornada

                Dim costo = CostoTarea.Calcular(remuneracion, diasMes, jornada, CostoTarea.Duracion(f.Horas, f.Minutos))

                item.Registros += 1
                item.Minutos += f.Horas * 60 + f.Minutos
                If costo.HasValue Then
                    item.Costo += costo.Value
                Else
                    item.RegistrosSinCosto += 1
                    sinCosto.Add(If(f.Usuario, "(sin usuario)"))
                End If
            Next

            ' Minutos acumulados -> horas y minutos.
            For Each item In porGrupo.Values
                item.Horas = item.Minutos \ 60
                item.Minutos = item.Minutos Mod 60
            Next

            Dim items = porGrupo.Values.
                OrderByDescending(Function(i) i.Costo).
                ThenBy(Function(i) i.Codigo, StringComparer.CurrentCultureIgnoreCase).
                ToList()

            Return New ResultadoCostos With {
                .Estado = ResultadoOperacionCostos.Ok,
                .Costos = New CostosDTO With {
                    .Agrupacion = agrupar.ToString().ToLowerInvariant(),
                    .Desde = d,
                    .Hasta = h,
                    .Items = items,
                    .Total = items.Sum(Function(i) i.Costo),
                    .RegistrosSinCosto = items.Sum(Function(i) i.RegistrosSinCosto),
                    .UsuariosSinCosto = sinCosto.OrderBy(Function(u) u, StringComparer.CurrentCultureIgnoreCase).ToList()
                }
            }
        End Function

        Private Shared Function Invalido(mensaje As String) As ResultadoCostos
            Return New ResultadoCostos With {.Estado = ResultadoOperacionCostos.Invalido, .Mensaje = mensaje}
        End Function

        Private Function CargarDiasHabiles(desde As Date, hasta As Date) As Dictionary(Of Integer, Integer)
            Dim anioDesde = desde.Year
            Dim anioHasta = hasta.Year
            Dim resultado As New Dictionary(Of Integer, Integer)
            For Each x In diasHabiles.ObtenerTodos().Where(Function(r) r.Anio >= anioDesde AndAlso r.Anio <= anioHasta).ToList()
                resultado(ClaveMes(x.Anio, x.Mes)) = x.CantDias
            Next
            Return resultado
        End Function

        Private Function CargarRemuneraciones(desde As Date, hasta As Date) As Dictionary(Of String, Decimal)
            Dim anioDesde = desde.Year
            Dim anioHasta = hasta.Year
            Dim resultado As New Dictionary(Of String, Decimal)(StringComparer.OrdinalIgnoreCase)
            For Each r In remuneraciones.ListarRemuneraciones().Where(Function(x) x.Anio >= anioDesde AndAlso x.Anio <= anioHasta).ToList()
                resultado(ClaveRemuneracion(r.Usuario, r.Anio, r.Mes)) = r.Remuneracion
            Next
            Return resultado
        End Function

        ' Jornada (Profile.Modalidad) y centro de cada usuario del período, una lectura de Profile por usuario.
        ' El centro es el actual del perfil: si un usuario cambió de centro, todas sus horas van al nuevo.
        Private Function CargarPerfiles(filas As List(Of FilaCosto)) As Dictionary(Of String, DatosPerfil)
            Dim resultado As New Dictionary(Of String, DatosPerfil)(StringComparer.OrdinalIgnoreCase)
            For Each u In filas.Select(Function(f) f.Usuario).
                                Where(Function(x) Not String.IsNullOrWhiteSpace(x)).
                                Distinct(StringComparer.OrdinalIgnoreCase)
                Dim perfil = usuarios.ObtenerPerfil(u)
                If perfil IsNot Nothing Then
                    resultado(u) = New DatosPerfil With {.Jornada = perfil.Modalidad, .Centro = If(perfil.Centro, "").Trim()}
                End If
            Next
            Return resultado
        End Function

        Private Shared Function ClaveMes(anio As Integer, mes As Integer) As Integer
            Return anio * 100 + mes
        End Function

        Private Shared Function ClaveRemuneracion(usuario As String, anio As Integer, mes As Integer) As String
            Return usuario.Trim() & "|" & anio & "|" & mes
        End Function
    End Class
End Namespace
