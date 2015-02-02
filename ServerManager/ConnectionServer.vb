﻿Imports System.Net.Sockets
Imports System.Threading
Imports System.Net
Imports EncryptionClassLibrary.Encryption.Asymmetric

Public Class ConnectionServer
    Public Event ClientConnected(sender As Object, ByRef e As ConnectionServerEventArgs)
    Private Listener As TcpListener
    Private Cancel As Boolean
    Private DecryptKey As PrivateKey
    Private EncryptKey As PublicKey
    ''' <summary>
    ''' Listens for incoming connections until an event handler cancels it.
    ''' Runs synchronously.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Listen()
        Cancel = False
        Listener.Start()
        While Not Cancel
            Dim client As TcpClient = Listener.AcceptTcpClient
            Dim s = client.GetStream
            Dim bytes As New List(Of Byte)
            Dim buffer(256) As Byte
            Dim i = s.Read(buffer, 0, buffer.Length - 1)
            For count As Integer = 0 To i - 1
                bytes.Add(buffer(count))
            Next
            Dim packet = RequestPacket.DecryptPacket(bytes.ToArray, DecryptKey)
            If Security.IsValidUser(packet.Username, packet.Password) Then
                Dim args = New ConnectionServerEventArgs(packet, client, EncryptKey)
                RaiseEvent ClientConnected(Me, args)
                Cancel = args.Cancel
            End If
            client.Close()
        End While
    End Sub
    Public Sub StopListening()
        Listener.Stop()
    End Sub
    Public Sub New(IP As IPAddress, Port As Integer, DecryptKey As PrivateKey, EncryptKey As PublicKey)
        Listener = New TcpListener(IP, Port)
        Me.DecryptKey = DecryptKey
        Me.EncryptKey = EncryptKey
    End Sub
End Class
Public Class ConnectionServerEventArgs
    Inherits EventArgs
    Public Property Client As TcpClient
    Private EncryptKey As PublicKey
    Public Property Request As RequestPacket
    Public Property Cancel As Boolean
    Public Sub SendResponse(Response As ResponsePacket)
        Dim e = Response.EncryptPacket(EncryptKey)
        _client.GetStream.Write(e, 0, e.Length)
    End Sub
    Public Sub New(Request As RequestPacket, ByRef Client As TcpClient, EncryptKey As PublicKey)
        Me.Request = Request
        Me.Client = Client
        Me.Cancel = False
    End Sub
End Class