Namespace Interfaces

    Public Interface IPlugin
        ReadOnly Property Name As String
        ReadOnly Property Version As String

        Sub Init()
        Sub [Exit]()

        Public Interface IPluginClient
            Sub PreLoad()
            Sub PostLoad()
            Sub [Exit]()
        End Interface
    End Interface
End Namespace