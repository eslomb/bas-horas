Imports System.ComponentModel
Imports System.Data
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports ClosedXML.Excel

Namespace Utils
    ' Genera un .xlsx a partir de una lista de DTOs: una columna por propiedad pública de "tipo"
    ' (primero las de la clase base), con el <DisplayName> como encabezado si lo tiene.
    ' Se pasa el tipo explícito para que las columnas salgan aunque la lista esté vacía.
    Public NotInheritable Class ExportadorExcel
        Private Sub New()
        End Sub

        ' columnasPrimero: nombres de propiedad que van al principio, en ese orden (las que el tipo no tiene se ignoran);
        ' el resto sigue en el orden de la jerarquía.
        Public Shared Function Generar(items As IEnumerable, tipo As Type, nombreHoja As String, Optional columnasPrimero As String() = Nothing) As Byte()
            Dim propiedades = Reordenar(PropiedadesEnOrden(tipo), columnasPrimero)
            Dim tabla As New DataTable(nombreHoja)

            For Each p In propiedades
                tabla.Columns.Add(Encabezado(p), If(Nullable.GetUnderlyingType(p.PropertyType), p.PropertyType))
            Next

            For Each item In items
                Dim fila = tabla.NewRow()
                For i = 0 To propiedades.Count - 1
                    fila(i) = If(propiedades(i).GetValue(item), DBNull.Value)
                Next
                tabla.Rows.Add(fila)
            Next

            Using wb As New XLWorkbook()
                wb.Worksheets.Add(tabla).Columns().AdjustToContents()

                Using ms As New MemoryStream()
                    wb.SaveAs(ms)
                    Return ms.ToArray()
                End Using
            End Using
        End Function

        ' Recorre la jerarquía desde la clase base hacia la derivada para que las columnas comunes queden primero.
        Private Shared Function PropiedadesEnOrden(tipo As Type) As List(Of PropertyInfo)
            Dim jerarquia As New List(Of Type)
            Dim actual = tipo
            While actual IsNot Nothing AndAlso actual IsNot GetType(Object)
                jerarquia.Insert(0, actual)
                actual = actual.BaseType
            End While

            ' <Browsable(False)> marca propiedades que van en el JSON pero no en el Excel (ej. IDReg).
            Return jerarquia.
                SelectMany(Function(t) t.GetProperties(BindingFlags.Public Or BindingFlags.Instance Or BindingFlags.DeclaredOnly)).
                Where(Function(p) Not EsOculta(p)).
                ToList()
        End Function

        Private Shared Function Reordenar(propiedades As List(Of PropertyInfo), columnasPrimero As String()) As List(Of PropertyInfo)
            If columnasPrimero Is Nothing OrElse columnasPrimero.Length = 0 Then Return propiedades

            Dim primeras = columnasPrimero.
                Select(Function(nombre) propiedades.FirstOrDefault(Function(p) p.Name = nombre)).
                Where(Function(p) p IsNot Nothing).
                ToList()

            Return primeras.Concat(propiedades.Except(primeras)).ToList()
        End Function

        Private Shared Function EsOculta(p As PropertyInfo) As Boolean
            Dim atributo = p.GetCustomAttribute(Of BrowsableAttribute)()
            Return atributo IsNot Nothing AndAlso Not atributo.Browsable
        End Function

        Private Shared Function Encabezado(p As PropertyInfo) As String
            Dim atributo = p.GetCustomAttribute(Of DisplayNameAttribute)()
            Return If(atributo IsNot Nothing, atributo.DisplayName, p.Name)
        End Function
    End Class
End Namespace
