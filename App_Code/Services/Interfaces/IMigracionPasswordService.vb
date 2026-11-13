Imports DTOs.Usuarios

Namespace Services.Interfaces
    Public Interface IMigracionPasswordService
        Function ProviderEsHashed() As Boolean
        Function Migrar() As ResultadoMigracionPasswordsDTO
    End Interface
End Namespace
