Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Web.Script.Serialization
Imports System.Threading



Module Module1

    Sub Main()

        Dim bstop As Boolean = False
        Dim boxip As String = "10.186.81.194"


        Dim t As New Thread(Sub()

                                While (Not bstop)

                                    Dim result_post = HandleBoxCmd("get_state", False, boxip, 1)

                                    Dim jss = New JavaScriptSerializer()
                                    Dim BoxStateObject = jss.Deserialize(Of BoxStateObject)(result_post)
                                    Console.WriteLine(BoxStateObject.state)

                                    Dim BoxState = jss.Deserialize(Of Object)(result_post)
                                    Console.WriteLine(BoxState("state"))

                                    Console.WriteLine(result_post)

                                    Threading.Thread.Sleep(1000)
                                End While

                            End Sub)
        t.Start()


        While True
            Dim result_post As String = ""
            Dim input = Console.ReadLine()
            If input.Equals("!") Then
                bstop = True
                t.Join()
                Exit Sub
            End If

            If input.Equals("st") Then
                result_post = HandleBoxCmd("get_state", False, boxip, 1)
            End If

            If input.Equals("in") Then
                result_post = HandleBoxCmd("initialize", False, boxip, 1)
            End If

            If input.Equals("o") Then
                result_post = HandleBoxCmd("open", False, boxip, 1)
            End If

            If input.Equals("cn") Then
                result_post = HandleBoxCmd("close", False, boxip, 1)
            End If

            If input.Equals("cp") Then
                result_post = HandleBoxCmd("close", True, boxip, 1)
            End If

            If input.Equals("en") Then
                result_post = HandleBoxCmd("engage", False, boxip, 1)
            End If

            If input.Equals("res") Then
                result_post = HandleBoxCmd("reset", False, boxip, 1)
            End If

            If input.Equals("dis") Then
                result_post = HandleBoxSetting("disabled", True, boxip, 1)
            End If

            If input.Equals("ena") Then
                result_post = HandleBoxSetting("disabled", False, boxip, 1)
            End If

            If input.Equals("cali") Then
                result_post = HandleBoxSetting("calibrated", True, boxip, 1)
            End If

            If input.Equals("uncali") Then
                result_post = HandleBoxSetting("calibrated", False, boxip, 1)
            End If

            If input.Equals("calreq") Then
                result_post = HandleBoxSetting("calibration_requested", True, boxip, 1)
            End If

            If input.Equals("uncalreq") Then
                result_post = HandleBoxSetting("calibration_requested", False, boxip, 1)
            End If

            If input.Equals("re") Then
                result_post = HandleBoxCmd("get_state", False, boxip, 1)
                Dim jss = New JavaScriptSerializer()
                Dim BoxStateObject = jss.Deserialize(Of BoxStateObject)(result_post)
                result_post = HandleBoxCmd("release", True, BoxStateObject.products(0).jlid, boxip, 1)
            End If

            Console.WriteLine(result_post)

        End While

    End Sub

    Public Function HandleBoxCmd(cmd As String, withproduct As Boolean, jlid As String, boxip As String, slidenumber As Integer) As String
        Dim commandToBeSent As BoxCommand
        Dim rnd As New Random()

        Dim thejlid As Object
        If (Not withproduct) Then
            thejlid = ""
        Else
            If (jlid.Equals("")) Then
                thejlid = rnd.[Next](1000, 100000).ToString()
            Else
                thejlid = jlid
            End If
        End If

        Dim theproduct As New ProductInBoxInfo() With { _
             .jlid = thejlid, _
             .test_result = "not_tested" _
        }
        commandToBeSent = New BoxCommand() With { _
            .cmd = cmd, _
            .products = New ProductInBoxInfo() {theproduct} _
        }
        Dim response As [String] = PostCmd(String.Format("http://{0}/slide{1}/command", boxip, slidenumber.ToString()), commandToBeSent)
        Return response

    End Function

    Public Function HandleBoxCmd(cmd As String, withproduct As Boolean, boxip As String, slidenumber As Integer) As String
        Return HandleBoxCmd(cmd, withproduct, "", boxip, slidenumber)
    End Function

    Public Function HandleBoxSetting(SettingName As String, value As Boolean, boxip As String, slidenumber As Integer) As String

        Dim commandToBeSent As Object = New With { _
            Key .cmd = "change_setting", _
            Key .name = SettingName, _
            Key .value = value _
        }


        Dim response As [String] = PostCmd(String.Format("http://{0}/slide{1}/command", boxip, slidenumber.ToString()), commandToBeSent)
        Return response

    End Function


    Public Function PostCmd(postDestination As String, objectToSend As Object) As String
        'Send the message
        Dim subrequest = DirectCast(WebRequest.Create(postDestination), HttpWebRequest)
        subrequest.Method = "POST"
        subrequest.Timeout = 4000

        Try
            Using requestStream = subrequest.GetRequestStream()
                Using requestStreamWriter = New System.IO.StreamWriter(requestStream)
                    Dim ser As New JavaScriptSerializer()
                    requestStreamWriter.Write(ser.Serialize(objectToSend))
                End Using
            End Using
            Using response = DirectCast(subrequest.GetResponse(), HttpWebResponse)
                ' todo: consider a more lenient check here?
                If response.StatusCode <> HttpStatusCode.OK Then
                    Thread.Sleep(1000)
                Else
                    Console.WriteLine("command sent ok")
                    Dim theresponse As String = ""
                    Using reader = New StreamReader(response.GetResponseStream())
                        theresponse = reader.ReadToEnd().ToString()
                    End Using

                    Return theresponse
                End If
            End Using

        Catch ex1 As System.Net.WebException
        Catch ex As Exception
        End Try
        Return Nothing
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
        Public jlid As String
        Public test_result As String
    End Structure

    Public Structure BoxCommand
        Public cmd As String
        Public products As ProductInBoxInfo()
        Public scanner_secret As String
    End Structure

End Module
