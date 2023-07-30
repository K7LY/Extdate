Attribute VB_Name = "Module1"
Sub Extdate(Subject As String)
    Dim Ptn As String, Ptn2 As String
    Dim re As Object, re_date As Object, mc As Object
    Dim DateStr As String, Months() As Integer, Days() As Integer, DateNum() As Integer
    
    Let Ptn = "([1‚P][0-2‚O-‚Q]|[0‚O]?[1-9‚P-‚X])[\/\.][0-3‚O-‚R]?[0-9‚O-‚X]"
    Set re = CreateObject("VBScript.RegExp")
    With re
        .Pattern = Ptn
        .Global = True
    End With 're
    
    Let Ptn2 = "[0-9]+"
    Set re_date = CreateObject("VBScript.RegExp")
    With re_date
        .Pattern = Ptn2
        .Global = True
    End With 're_date
    
    Set mc = re.Execute(Subject)
    If mc.Count Then
        For Each d In mc
            DateStr = d.Value
            DateStr = StrConv(DateStr, vbNarrow)
            MsgBox DateStr
        Next
    End If
End Sub

Private Sub test()
    Call Extdate("2/9,9/14")
End Sub
