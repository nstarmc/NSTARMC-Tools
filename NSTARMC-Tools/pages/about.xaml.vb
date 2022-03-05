Imports System.IO
Imports System.Net
Imports System.Threading
Imports ModernWpf.Controls

Class about
    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        Dim pagestart As Thread = New Thread(AddressOf pagestartthread)
        pagestart.Start()

    End Sub

    Private Sub pagestartthread()
        '发送get请求，获取xml
        Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/update.xml")
        request.Method = "GET"
        Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
        Dim upd As XElement = XElement.Parse(sr.ReadToEnd)
        verinfo.Dispatcher.Invoke(New Action(Sub()
                                                 uphist.Text = upd.<history>.<text>.Value & vbCrLf & "最后更新日期：" & upd.<history>.<lastdate>.Value
                                                 upfu.Text = upd.<future>.<text>.Value & vbCrLf & "最后更新日期：" & upd.<future>.<lastdate>.Value
                                             End Sub))

        If My.Settings.sguid = "nothing" Then
            Dim sGUID As String
            sGUID = System.Guid.NewGuid.ToString()
            My.Settings.sguid = System.Guid.NewGuid.ToString()
            My.Settings.Save()
        End If
        verinfo.Dispatcher.Invoke(New Action(Sub()
                                                 '获取版本信息
                                                 verinfo.Subtitle = "工具版本：" & My.Application.Info.Version.ToString & vbCrLf & "版本状态：" & My.Application.Info.Description & vbCrLf & "运行路径：" & My.Application.Info.DirectoryPath & vbCrLf & "GUID:" & My.Settings.sguid
                                             End Sub))

    End Sub

    Private Sub bt_copy_Click(sender As Object, e As RoutedEventArgs) Handles bt_copy.Click
        Clipboard.Clear() ' 清除剪贴板
        Clipboard.SetText("工具版本：" & My.Application.Info.Version.ToString & vbCrLf & "版本状态：" & My.Application.Info.Description & vbCrLf & "运行路径：" & My.Application.Info.DirectoryPath & vbCrLf & "GUID:" & My.Settings.sguid) ' 拷贝数据到粘贴板
        Dim dialog As ContentDialog = New ContentDialog() With {
                                           .Title = "复制成功",
                                           .CloseButtonText = "我知道了",
                                           .IsPrimaryButtonEnabled = False,
                                           .DefaultButton = ContentDialogButton.Close,
                                           .Content = "工具信息复制成功！"
                                       }
        dialog.ShowAsync()
    End Sub
End Class
