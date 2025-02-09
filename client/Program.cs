using System;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Text;

class Client
{
    private static string ReadAndSend(NetworkStream stream) {
        string message = Console.ReadLine();
        byte[] messageBytes = Encoding.ASCII.GetBytes(message);
        stream.Write(messageBytes, 0, messageBytes.Length);
        return message;
    }

    private static string Receive(NetworkStream stream) {
        byte[] acknowledgmentBytes = new byte[1024];
        int bytesRead = stream.Read(acknowledgmentBytes, 0, acknowledgmentBytes.Length);
        string acknowledgment = Encoding.ASCII.GetString(acknowledgmentBytes, 0, bytesRead);
        return acknowledgment;
    }

    private static string SendAndReceive(NetworkStream stream) {
        var message = ReadAndSend(stream);
        var messageFromServer = Receive(stream);
        if(message == "/leave") {
            return message;
        }
        return messageFromServer;
    }

    static void Main(string[] args)
    {
        if(args.Length != 2)
            throw new Exception("Usage: dotnet run <IP> <PORT>");
        TcpClient client = new TcpClient();
        string received;

        try
        {
            client.Connect(args[0], int.Parse(args[1])); // Connect to the server
        }
        catch
        {
            Console.WriteLine("Error: Couldn't connect to the server. It probably isn't on. :)");
            return;
        }

        NetworkStream stream = client.GetStream();
        Console.WriteLine("To Enter the server, type /join.");

        // here we try to join the server until we are allowed in.
        while ((received = SendAndReceive(stream)) != "OK") {
            Console.WriteLine($"Server response: {received}");
        }

        Console.WriteLine("We have joined. We need to send the server our name.");
        received = SendAndReceive(stream);
        // todo, do a while true with /join and name in case the server doesn't allow us in because it's full or bad name.
        if (received != "OK") {
            Console.WriteLine("Server didn't allow us in.");
            return;
        }
        Console.WriteLine("We are connected.");

        while(true) {
            received = SendAndReceive(stream);
            if (received == "/leave") {
                client.Close();
                break;
            }
            Console.WriteLine($"Received {received}");
        }
    }
}
