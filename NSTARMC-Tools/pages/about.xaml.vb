Imports System.IO
Imports System.Net

Class about
    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        '发送get请求，获取xml
        Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/update.xml")
        request.Method = "GET"
        Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
        Dim upd As XElement = XElement.Parse(sr.ReadToEnd)
        uphist.Text = upd.<history>.<text>.Value & vbCrLf & "最后更新日期：" & upd.<history>.<lastdate>.Value
        upfu.Text = upd.<future>.<text>.Value & vbCrLf & "最后更新日期：" & upd.<future>.<lastdate>.Value
        Dim sGUID As String
        sGUID = System.Guid.NewGuid.ToString()
        '获取版本信息
        verinfo.Subtitle = "工具版本：" & My.Application.Info.Version.ToString & vbCrLf & "版本状态：" & My.Application.Info.Description & vbCrLf & "运行路径：" & My.Application.Info.DirectoryPath & vbCrLf & "GUID:" & sGUID
    End Sub
End Class
