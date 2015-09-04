Imports System.Net
Imports System.IO
Imports System.Text

Module Module1

    Sub Main()
        While True
            Dim jsonString As String = """{""cmd"": ""get_state""}"""
            Dim uri As String = "http://10.186.81.193:80/slide1/command"
            Dim data = Encoding.UTF8.GetBytes(jsonString)
            Dim result_post = SendRequest(New Uri(uri), data, "application/json", "POST")

            Console.WriteLine(result_post)

            Threading.Thread.Sleep(1000)

        End While

    End Sub

    Private Function SendRequest(uri As Uri, jsonDataBytes As Byte(), contentType As String, method As String) As String
        Dim req As WebRequest = WebRequest.Create(uri)
        req.ContentType = contentType
        req.Method = method
        req.ContentLength = jsonDataBytes.Length


        Dim stream = req.GetRequestStream()
        stream.Write(jsonDataBytes, 0, jsonDataBytes.Length)
        stream.Close()

        Dim response = req.GetResponse().GetResponseStream()

        Dim reader As New StreamReader(response)
        Dim res = reader.ReadToEnd()
        reader.Close()
        response.Close()

        Return res
    End Function

End Module
