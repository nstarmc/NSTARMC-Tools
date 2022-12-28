Imports System.Threading

Public Class Form1
    Dim mods, sharders, res

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim thr As Thread = New Thread(AddressOf Thread2)
        thr.Start()
    End Sub
    Private Function Thread2(ByVal objParamReport As Object) As String
        For Each re In ListBox1.Items
            Dim newname = "[##模组##]" & Replace(re, mods, "")
            FileSystem.Rename(re, mods & newname)
        Next
        For Each re In ListBox2.Items
            Dim newname = "[##光影##]" & Replace(re, sharders, "")
            FileSystem.Rename(re, sharders & newname)
        Next
        For Each re In ListBox3.Items
            Dim newname = "[##资源包##]" & Replace(re, res, "")
            FileSystem.Rename(re, res & newname)
        Next
        MsgBox("success", 64, "success")
        Dim thr As Thread = New Thread(AddressOf Thread1)
        thr.Start()
    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckForIllegalCrossThreadCalls = False
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        Try

            mods = TextBox1.Text & "\.minecraft\mods\"
            sharders = TextBox1.Text & "\.minecraft\shaderpacks\"
            res = TextBox1.Text & "\.minecraft\resourcepacks\"
            Label2.Text = "模组路径：" & mods & vbCrLf & "光影：" & sharders & vbCrLf & "资源路径：" & res
            Dim thr As Thread = New Thread(AddressOf Thread1)
            thr.Start()
        Catch ex As Exception

        End Try
    End Sub

    Private Function Thread1(ByVal objParamReport As Object) As String
        ListBox1.Items.Clear()
        ListBox2.Items.Clear()
        ListBox3.Items.Clear()

        If IO.Directory.Exists(mods) Then
            For Each file1 In System.IO.Directory.GetFiles(mods)
                ListBox1.Items.Add(file1)
            Next
        End If
        If IO.Directory.Exists(sharders) Then
            For Each file1 In System.IO.Directory.GetFiles(sharders)
                ListBox2.Items.Add(file1)
            Next
        End If
        If IO.Directory.Exists(res) Then
            For Each file1 In System.IO.Directory.GetFiles(res)
                ListBox3.Items.Add(file1)
            Next
        End If
    End Function
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            FolderBrowserDialog1.ShowDialog()
            TextBox1.Text = FolderBrowserDialog1.SelectedPath
            mods = TextBox1.Text & "\.minecraft\mods\"
            sharders = TextBox1.Text & "\.minecraft\shaderpacks\"
            res = TextBox1.Text & "\.minecraft\resourcepacks\"
            Label2.Text = "模组路径：" & mods & vbCrLf & "光影：" & sharders & vbCrLf & "资源路径：" & res
            Dim thr As Thread = New Thread(AddressOf Thread1)
            thr.Start()
        Catch ex As Exception

        End Try

    End Sub
End Class
