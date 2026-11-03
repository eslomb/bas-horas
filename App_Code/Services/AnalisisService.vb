Imports System.IO
Imports System.Linq
Imports System.Text.RegularExpressions
Imports ClosedXML.Excel
Imports DTOs.Analisis
Imports Repositories.Interfaces
Imports Services.Interfaces

Namespace Services
    Public Class AnalisisService
        Implements IAnalisisService

        ' Valores que acepta el parámetro @concepto de spAnalisisActividades.
        Private Shared ReadOnly Conceptos As String() = {"producto", "pais", "usuario"}

        ' Encabezado de la primera columna del Excel y parte del nombre del archivo, por concepto.
        Private Shared ReadOnly Titulos As New Dictionary(Of String, String) From {
            {"producto", "Producto"}, {"pais", "País"}, {"usuario", "Usuario"}
        }
        Private Shared ReadOnly Plurales As New Dictionary(Of String, String) From {
            {"producto", "Productos"}, {"pais", "Paises"}, {"usuario", "Usuarios"}
        }

        ' El SP devuelve el tiempo formateado por dbo.MinutosHHMM: "12h 05m".
        Private Shared ReadOnly FormatoRecuento As New Regex("^\s*(\d+)h\s*(\d+)m\s*$")

        ' Las horas van como tiempo de Excel (fracción de día) con este formato: se ven como hh:mm y se pueden sumar.
        Private Const FormatoHoras As String = "[h]:mm"
        Private Const MinutosPorDia As Double = 1440

        Private ReadOnly repo As IAnalisisRepository

        Public Sub New(repo As IAnalisisRepository)
            Me.repo = repo
        End Sub

        Public Function AnalizarActividades(concepto As String, desde As Date?, hasta As Date?) As ResultadoAnalisis Implements IAnalisisService.AnalizarActividades
            Dim c = NormalizarConcepto(concepto)
            Dim mensaje = Validar(c, desde, hasta)
            If mensaje IsNot Nothing Then Return New ResultadoAnalisis With {.Estado = ResultadoOperacionAnalisis.Invalido, .Mensaje = mensaje}

            Return New ResultadoAnalisis With {.Estado = ResultadoOperacionAnalisis.Ok, .Items = Consultar(c, desde, hasta)}
        End Function

        ' Tabla cruzada de los conceptos elegidos: una fila por concepto, una columna por actividad, con totales.
        Public Function ExportarActividades(concepto As String, desde As Date?, hasta As Date?, ids As String()) As ResultadoArchivoAnalisis Implements IAnalisisService.ExportarActividades
            Dim c = NormalizarConcepto(concepto)
            Dim mensaje = Validar(c, desde, hasta)

            Dim elegidos As New HashSet(Of String)(
                If(ids, New String() {}).Where(Function(i) Not String.IsNullOrWhiteSpace(i)).Select(Function(i) i.Trim()),
                StringComparer.OrdinalIgnoreCase)
            If mensaje Is Nothing AndAlso elegidos.Count = 0 Then mensaje = "Debe elegir al menos un " & Titulos(c).ToLowerInvariant() & "."

            If mensaje IsNot Nothing Then
                Return New ResultadoArchivoAnalisis With {.Estado = ResultadoOperacionAnalisis.Invalido, .Mensaje = mensaje}
            End If

            Dim items = Consultar(c, desde, hasta).Where(Function(i) elegidos.Contains(i.Id)).ToList()

            Return New ResultadoArchivoAnalisis With {
                .Estado = ResultadoOperacionAnalisis.Ok,
                .Contenido = GenerarExcel(items, c, desde, hasta),
                .NombreArchivo = "Analisis_" & Plurales(c) &
                                 If(desde.HasValue, "_" & desde.Value.ToString("yyyy-MM-dd"), "") &
                                 If(hasta.HasValue, "_" & hasta.Value.ToString("yyyy-MM-dd"), "") & ".xlsx"
            }
        End Function

        Private Shared Function NormalizarConcepto(concepto As String) As String
            Return If(concepto, "").Trim().ToLowerInvariant()
        End Function

        Private Shared Function Validar(concepto As String, desde As Date?, hasta As Date?) As String
            If Not Conceptos.Contains(concepto) Then
                Return "El concepto debe ser uno de: " & String.Join(", ", Conceptos) & "."
            End If
            If desde.HasValue AndAlso hasta.HasValue AndAlso desde.Value > hasta.Value Then
                Return "La fecha desde no puede ser posterior a la fecha hasta."
            End If
            Return Nothing
        End Function

        ' Las filas sin concepto o sin actividad (left joins del SP) no se pueden graficar.
        Private Function Consultar(concepto As String, desde As Date?, hasta As Date?) As List(Of AnalisisActividadDTO)
            Return repo.EjecutarAnalisisActividades(concepto, desde, hasta).
                Where(Function(r) Not String.IsNullOrWhiteSpace(r.ID) AndAlso Not String.IsNullOrWhiteSpace(r.CodActiv)).
                Select(Function(r) MapearDTO(r)).
                Where(Function(d) d IsNot Nothing).
                ToList()
        End Function

        Private Shared Function GenerarExcel(items As List(Of AnalisisActividadDTO), concepto As String, desde As Date?, hasta As Date?) As Byte()
            Dim actividades = items.
                Where(Function(i) i.Horas > 0 OrElse i.Minutos > 0).
                Select(Function(i) i.CodActividad).
                Distinct(StringComparer.OrdinalIgnoreCase).
                OrderBy(Function(a) a, StringComparer.OrdinalIgnoreCase).
                ToList()

            Dim filas = items.
                GroupBy(Function(i) i.Id, StringComparer.OrdinalIgnoreCase).
                Select(Function(g) New With {.Concepto = g.First().Concepto, .Items = g.ToList()}).
                OrderBy(Function(f) f.Concepto, StringComparer.CurrentCultureIgnoreCase).
                ToList()

            Using wb As New XLWorkbook()
                Dim ws = wb.Worksheets.Add("Analisis")

                ws.Cell(1, 1).Value = "Actividades por " & Titulos(concepto).ToLowerInvariant() & " " & DescribirPeriodo(desde, hasta)
                ws.Cell(1, 1).Style.Font.Bold = True

                ' tabla desde la fila 3; columna 1 = concepto, luego una por actividad y el total
                Const filaEncabezado As Integer = 3
                Dim colTotal = actividades.Count + 2
                Dim filaTotal = filaEncabezado + filas.Count + 1

                ws.Cell(filaEncabezado, 1).Value = Titulos(concepto)
                For j = 0 To actividades.Count - 1
                    ws.Cell(filaEncabezado, j + 2).Value = actividades(j)
                Next
                ws.Cell(filaEncabezado, colTotal).Value = "Total"

                For i = 0 To filas.Count - 1
                    Dim fila = filaEncabezado + i + 1
                    ws.Cell(fila, 1).Value = filas(i).Concepto
                    For Each item In filas(i).Items
                        Dim j = actividades.FindIndex(Function(a) a.Equals(item.CodActividad, StringComparison.OrdinalIgnoreCase))
                        If j >= 0 Then ws.Cell(fila, j + 2).Value = (item.Horas * 60 + item.Minutos) / MinutosPorDia
                    Next
                    ws.Cell(fila, colTotal).FormulaA1 = Suma(ws.Cell(fila, 2), ws.Cell(fila, colTotal - 1))
                Next

                ws.Cell(filaTotal, 1).Value = "Total"
                For col = 2 To colTotal
                    ws.Cell(filaTotal, col).FormulaA1 = Suma(ws.Cell(filaEncabezado + 1, col), ws.Cell(filaTotal - 1, col))
                Next

                ws.Range(filaEncabezado + 1, 2, filaTotal, colTotal).Style.NumberFormat.Format = FormatoHoras
                ws.Row(filaEncabezado).Style.Font.Bold = True
                ws.Row(filaTotal).Style.Font.Bold = True
                ws.Column(colTotal).Style.Font.Bold = True
                ws.SheetView.Freeze(filaEncabezado, 1)
                ws.Columns().AdjustToContents(filaEncabezado, filaTotal)

                Using ms As New MemoryStream()
                    wb.SaveAs(ms)
                    Return ms.ToArray()
                End Using
            End Using
        End Function

        ' IXLAddress oculta Object.ToString() con su sobrecarga: hay que pasar el estilo explícito.
        Private Shared Function Suma(desde As IXLCell, hasta As IXLCell) As String
            Return "SUM(" & desde.Address.ToString(XLReferenceStyle.A1) & ":" & hasta.Address.ToString(XLReferenceStyle.A1) & ")"
        End Function

        Private Shared Function DescribirPeriodo(desde As Date?, hasta As Date?) As String
            If desde.HasValue AndAlso hasta.HasValue Then Return "del " & desde.Value.ToString("dd/MM/yyyy") & " al " & hasta.Value.ToString("dd/MM/yyyy")
            If desde.HasValue Then Return "desde el " & desde.Value.ToString("dd/MM/yyyy")
            If hasta.HasValue Then Return "hasta el " & hasta.Value.ToString("dd/MM/yyyy")
            Return "(todo el período)"
        End Function

        Private Shared Function MapearDTO(r As spAnalisisActividades_Result) As AnalisisActividadDTO
            Dim m = FormatoRecuento.Match(If(r.Recuento, ""))
            If Not m.Success Then Return Nothing

            Return New AnalisisActividadDTO With {
                .Id = r.ID.Trim(),
                .Concepto = r.Concepto,
                .CodActividad = r.CodActiv.Trim(),
                .Horas = Integer.Parse(m.Groups(1).Value),
                .Minutos = Integer.Parse(m.Groups(2).Value)
            }
        End Function
    End Class
End Namespace
