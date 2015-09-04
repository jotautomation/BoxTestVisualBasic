Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Web.Helpers
Imports System.Web.Script.Serialization



Module Module1

    Sub Main()
        While True
            Dim jsonString As String = """{""cmd"": ""get_state""}"""
            Dim uri As String = "http://10.186.81.193:80/slide1/command"
            Dim data = Encoding.UTF8.GetBytes(jsonString)
            Dim result_post = SendRequest(New Uri(uri), data, "application/json", "POST")


            Dim BoxStateDynamic = System.Web.Helpers.Json.Decode(result_post)
            Console.WriteLine(BoxStateDynamic.State)


            Dim jss = New JavaScriptSerializer()
            Dim BoxStateObject = jss.Deserialize(Of BoxStateObject)(result_post)
            Console.WriteLine(BoxStateObject.state)


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

    Public Structure BoxStateObject
        Public state As String
        Public products As ProductInBoxInfo()
        Public error_message As String
        Public error_description As String
        Public message_timestamp As UInteger
        Public disabled As Boolean
        Public calibrated As Boolean
        Public calibration_requested As Boolean
    End Structure

    Public Structure ProductInBoxInfo
        Public Jlid As String
        Public TestResult As String
    End Structure

End Module
