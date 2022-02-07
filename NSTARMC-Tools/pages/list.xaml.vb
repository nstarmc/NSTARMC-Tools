Imports System.Threading
Imports ModernWpf.Controls
Imports System.IO

Class list
    Dim del_dir
    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        Dim homepagestart As Thread = New Thread(AddressOf Homepagestartthread)
        homepagestart.Start()
    End Sub
    Private Function Homepagestartthread(ByVal objParamReport As Object) As String
        mclist.Dispatcher.Invoke(New Action(Sub()
                                                mclist.Items.Clear()
                                            End Sub))


        For Each dirlist In System.IO.Directory.GetDirectories(My.Application.Info.DirectoryPath & "\file\")
            If My.Computer.FileSystem.FileExists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = My.Computer.FileSystem.ReadAllText(dirlist & "\info.xml")
                Dim info_xml As XElement = XElement.Parse(info_reader)
                mclist.Dispatcher.Invoke(New Action(Sub()
                                                        mclist.Items.Add(info_xml.<mcversion>.Value & " - " & info_xml.<modloader>.Value & "(" & info_xml.<sharder>.Value & ")")

                                                    End Sub))
            End If
        Next
        If mclist.Items.Count = 0 Then
            mclist.Dispatcher.Invoke(New Action(Sub()
                                                    mclist.IsEnabled = False
                                                    Dim dialog As ContentDialog = New ContentDialog() With {
                                                        .Title = "错误",
                                                        .CloseButtonText = "我知道了",
                                                        .IsPrimaryButtonEnabled = False,
                                                        .DefaultButton = ContentDialogButton.Close,
                                                        .Content = "无法在工具下的File检测到有效格式的整合包！" & vbCrLf & "请正确放置整合包文件后，重启工具再试！" & vbCrLf & "如还是无法检测，请在Q群求助！"
                                                    }
                                                    dialog.ShowAsync()
                                                End Sub))

        Else
            mclist.Dispatcher.Invoke(New Action(Sub()
                                                    mclist.SelectedIndex = 0
                                                End Sub))

        End If
    End Function

    Private Sub mclist_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles mclist.SelectionChanged
        Dim choose_dir
        For Each dirlist In System.IO.Directory.GetDirectories(My.Application.Info.DirectoryPath & "\file\")
            If My.Computer.FileSystem.FileExists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = My.Computer.FileSystem.ReadAllText(dirlist & "\info.xml")
                Dim info_xml As XElement = XElement.Parse(info_reader)
                If mclist.SelectedItem = info_xml.<mcversion>.Value & " - " & info_xml.<modloader>.Value & "(" & info_xml.<sharder>.Value & ")" Then
                    choose_dir = dirlist
                    mclist.Dispatcher.Invoke(New Action(Sub()
                                                            verinfolabel.Content = "版本号：" & info_xml.<mcversion>.Value & vbCrLf &
                                                            "模组加载器：" & info_xml.<modloader>.Value & vbCrLf &
                                                            "光影加载器：" & info_xml.<sharder>.Value & vbCrLf &
                                                            "打包日期：" & info_xml.<packdate>.<year>.Value & "年" & info_xml.<packdate>.<month>.Value & "月" & info_xml.<packdate>.<day>.Value & "日" & vbCrLf &
                                                            "整合包ID（日后检查更新用）：" & info_xml.<packid>.Value
                                                        End Sub))
                End If

            End If
        Next
    End Sub

    Private Sub bt_opdir_Click(sender As Object, e As RoutedEventArgs) Handles bt_opdir.Click
        For Each dirlist In System.IO.Directory.GetDirectories(My.Application.Info.DirectoryPath & "\file\")
            If My.Computer.FileSystem.FileExists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = My.Computer.FileSystem.ReadAllText(dirlist & "\info.xml")
                Dim info_xml As XElement = XElement.Parse(info_reader)
                If mclist.SelectedItem = info_xml.<mcversion>.Value & " - " & info_xml.<modloader>.Value & "(" & info_xml.<sharder>.Value & ")" Then
                    Dim p As New Process()
                    p.StartInfo.FileName = "cmd.exe"
                    p.StartInfo.UseShellExecute = False
                    p.StartInfo.RedirectStandardInput = True
                    p.StartInfo.RedirectStandardOutput = True
                    p.StartInfo.RedirectStandardError = True
                    p.StartInfo.CreateNoWindow = True
                    p.Start()
                    p.StandardInput.WriteLine("explorer " & dirlist) '这个Data就是cmd命令
                    p.StandardInput.WriteLine("exit") '这个Data就是cmd命令
                    p.WaitForExit()
                    Exit For
                End If

            End If
        Next
    End Sub

    Private Async Function bt_delpack_ClickAsync(sender As Object, e As RoutedEventArgs) As Task Handles bt_delpack.Click
        For Each dirlist In System.IO.Directory.GetDirectories(My.Application.Info.DirectoryPath & "\file\")
            If My.Computer.FileSystem.FileExists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = My.Computer.FileSystem.ReadAllText(dirlist & "\info.xml")
                Dim info_xml As XElement = XElement.Parse(info_reader)
                If mclist.SelectedItem = info_xml.<mcversion>.Value & " - " & info_xml.<modloader>.Value & "(" & info_xml.<sharder>.Value & ")" Then
                    Dim dialog As ContentDialog = New ContentDialog() With {
                                                        .Title = "确山删除整合包？",
                                                        .CloseButtonText = "取消",
                                                        .PrimaryButtonText = "确认删除",
                                                        .DefaultButton = ContentDialogButton.Primary,
                                                        .Content = "即将删除整合包：" & info_xml.<mcversion>.Value & " - " & info_xml.<modloader>.Value & "(" & info_xml.<sharder>.Value & ")" & vbCrLf &
                                                        "将删除整合包内所有数据，该操作不可逆1" & vbCrLf & "是否继续删除？"
                                                    }
                    Dim result As ContentDialogResult = Await dialog.ShowAsync()
                    If result = ContentDialogResult.Primary Then
                        Dim delpack As Thread = New Thread(AddressOf Delpackthread)
                        delpack.Start()
                        del_dir = dirlist
                    End If
                    Exit For
                End If

            End If
        Next
    End Function

    Private Function Delpackthread(ByVal objParamReport As Object) As String
        Try
            Directory.Delete(del_dir, True)
            mclist.Dispatcher.Invoke(New Action(Sub()


                                                    Dim dialog As ContentDialog = New ContentDialog() With {
                .Title = "成功",
                .CloseButtonText = "我知道了",
                .IsPrimaryButtonEnabled = False,
                .DefaultButton = ContentDialogButton.Close,
                .Content = "成功删除整合包！"
            }
                                                    dialog.ShowAsync()
                                                End Sub))
            '刷新列表
            Dim homepagestart As Thread = New Thread(AddressOf Homepagestartthread)
            homepagestart.Start()
        Catch ex As Exception
            mclist.Dispatcher.Invoke(New Action(Sub()
                                                    Dim dialog As ContentDialog = New ContentDialog() With {
               .Title = "失败",
               .CloseButtonText = "我知道了",
               .IsPrimaryButtonEnabled = False,
               .DefaultButton = ContentDialogButton.Close,
               .Content = "删除整合包不成功或不完全！" & vbCrLf & "请尝试手动删除！"
           }
                                                    dialog.ShowAsync()
                                                End Sub))
            '刷新列表
            Dim homepagestart As Thread = New Thread(AddressOf Homepagestartthread)
            homepagestart.Start()
        End Try

    End Function
End Class
