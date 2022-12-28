Imports System.Text.RegularExpressions
Imports IniParser
Imports IniParser.Model
Imports ModernWpf
Imports ModernWpf.Controls
Imports WPFUI
Imports System.IO
Imports System.Reflection
Imports GuerrillaNtp

Public Class beta_verify
    Private Sub Window_Initialized(sender As Object, e As EventArgs)
        MainWindow.shouquan = False


        '配置文件前置
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim parser = New FileIniDataParser()
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")
        If data("UI")("AutoTheme") = "False" Then
            If data("UI")("Theme") = "Dark" Then
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark
                Theme.Manager.Switch(Theme.Style.Dark)
            Else
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light
                Theme.Manager.Switch(Theme.Style.Light)

            End If
        Else
            If Theme.Manager.GetSystemTheme = Theme.Style.Dark Then
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark
                Theme.Manager.Switch(Theme.Style.Dark)
            Else
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light
                Theme.Manager.Switch(Theme.Style.Light)
            End If

        End If

        '加密模块-获取设备ID
        Dim currentUser As System.Security.Principal.WindowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent()
        Dim sid As String = currentUser.User.ToString() & "NSTARMCToolsSID"
        Dim dataToHash As Byte() = (New System.Text.ASCIIEncoding).GetBytes(sid)
        Dim hashvalue As Byte() = CType(System.Security.Cryptography.CryptoConfig.CreateFromName("MD5"), System.Security.Cryptography.HashAlgorithm).ComputeHash(dataToHash)
        Dim ATR As String = ""
        Dim i As Integer
        '选择32位字符的加密结果   
        For i = 0 To 15
            ATR &= Hex(hashvalue(i)).PadLeft(2, "0").ToLower
        Next
        sb_id.Text = ATR.ToUpper
        'Dim isbeta As Boolean = True
        Dim isbeta As Boolean = False
        If isbeta = True Then

            'Beta版本，启动校验
            If data("Keys")("key") = "" Then
            Else

                Dim spinfo As Sponsor_info
                spinfo = Sponsor_proof(data("Keys")("key"))
                If sb_id.Text = spinfo.attachid Then

                    '校验是否过期
                    Dim client As NtpClient = New NtpClient("ntp.aliyun.com")
                    Dim clock As NtpClock = client.Query()
                    Dim utc As DateTime = clock.UtcNow.UtcDateTime
                    If spinfo.outdate > utc Then
                        '通过
                        MainWindow.shouquan = True

                        Dim msw As MainWindow = New MainWindow()
                        msw.Show()
                        Me.Hide()
                    Else
                        '过期
                    End If
                Else
                    '不通过
                End If
            End If
        Else
            If data("Keys")("key") = "" Then
            Else
                Dim spinfo As Sponsor_info
                spinfo = Sponsor_proof(data("Keys")("key"))
                If sb_id.Text = spinfo.attachid Then

                    '校验是否过期
                    Dim client As NtpClient = New NtpClient("ntp.aliyun.com")
                    Dim clock As NtpClock = client.Query()
                    Dim utc As DateTime = clock.UtcNow.UtcDateTime
                    If spinfo.outdate > utc Then
                        '通过
                        MainWindow.shouquan = True
                    Else
                        '过期
                    End If
                Else
                    '不通过
                End If

            End If
            Dim msw As MainWindow = New MainWindow()
            msw.Show()
            Me.Hide()
        End If

    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)

        Clipboard.Clear() ' 清除剪贴板
        Clipboard.SetText(sb_id.Text) ' 拷贝数据到粘贴板
        Dim dialog As ContentDialog = New ContentDialog() With {
                                           .Title = "复制成功",
                                           .CloseButtonText = "我知道了",
                                           .IsPrimaryButtonEnabled = False,
                                           .DefaultButton = ContentDialogButton.Close,
                                           .Content = "您的设备ID已成功复制！"
                                       }
        dialog.ShowAsync()
    End Sub

    Private Async Sub Button_Click_1(sender As Object, e As RoutedEventArgs)

        '配置文件前置
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim parser = New FileIniDataParser()
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")
        Dim spinfo As Sponsor_info
        spinfo = Sponsor_proof(key_input.Text)

        '写入
        data("Keys")("key") = key_input.Text
        data("Keys")("Year") = spinfo.outdate.Year
        data("Keys")("Month") = spinfo.outdate.Month
        data("Keys")("Day") = spinfo.outdate.Day
        parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
        MsgBox(spinfo.success)
        If spinfo.attachid = sb_id.Text Then
            '校验是否过期
            Dim client As NtpClient = New NtpClient("ntp.aliyun.com")
            Dim clock As NtpClock = client.Query()
            Dim utc As DateTime = clock.UtcNow.UtcDateTime
            'Dim getWebDatetime As Date
            'Dim XmlHttp As Object
            'XmlHttp = CreateObject("Microsoft.XMLHTTP")
            'XmlHttp.open("POST", "https://www.baidu.com/", False)
            'XmlHttp.send()
            'getWebDatetime = (CDate(Date.FromOADate(1 / 3 + CDbl(CDate(Mid$(XmlHttp.getResponseHeader("Date"), 5, 21)).ToOADate()))))
            If spinfo.outdate.Date > utc Then
                '通过
                MainWindow.shouquan = True

                Dim dialog As ContentDialog = New ContentDialog() With {
                               .Title = "授权码检验成功！",
                               .CloseButtonText = "我知道了",
                               .IsPrimaryButtonEnabled = False,
                               .DefaultButton = ContentDialogButton.Close,
                               .Content = "授权码检验成功！" & vbCrLf & "你可以使用NSTARMC-Tools的Beta版本啦！"
                           }
                Await dialog.ShowAsync()

                Dim msw As MainWindow = New MainWindow()
                msw.Show()
                Me.Hide()
            Else
                '不通过
                Dim dialog As ContentDialog = New ContentDialog() With {
.Title = "授权码检验失败！",
.CloseButtonText = "我知道了",
.IsPrimaryButtonEnabled = False,
.DefaultButton = ContentDialogButton.Close,
.Content = "授权码检验失败！" & vbCrLf & "该授权码已经到期！"
}
                Await dialog.ShowAsync()
            End If



        Else

            Dim dialog As ContentDialog = New ContentDialog() With {
                                   .Title = "授权码检验失败！",
                                   .CloseButtonText = "我知道了",
                                   .IsPrimaryButtonEnabled = False,
                                   .DefaultButton = ContentDialogButton.Close,
                                   .Content = "授权码检验失败！" & vbCrLf & "请检查授权码是否与收到的一致！" & vbCrLf & "授权码建议复制粘贴！"
                               }
            Await dialog.ShowAsync()
        End If
    End Sub

    Private Sub key_day_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles key_day.PreviewTextInput
        Dim re As Regex = New Regex("[^0-9]+")
        e.Handled = re.IsMatch(e.Text)
    End Sub

    Private Sub key_day_TextChanged(sender As Object, e As TextChangedEventArgs) Handles key_day.TextChanged
        If key_day.Text = "" Then
            key_day.Text = "1"
        End If
    End Sub
End Class
