using System;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Text;

class Client
{
    private static void ReadAndSend(NetworkStream stream) {
        string message = Console.ReadLine();
        byte[] messageBytes = Encoding.ASCII.GetBytes(message);
        stream.Write(messageBytes, 0, messageBytes.Length);
    }

    private static string Receive(NetworkStream stream) {
        byte[] acknowledgmentBytes = new byte[1024];
        int bytesRead = stream.Read(acknowledgmentBytes, 0, acknowledgmentBytes.Length);
        string acknowledgment = Encoding.ASCII.GetString(acknowledgmentBytes, 0, bytesRead);
        return acknowledgment;
    }

    private static string SendAndReceive(NetworkStream stream) {
        ReadAndSend(stream);
        return Receive(stream);
    }

    static void Main(string[] args)
    {
        if(args.Length != 2)
            throw new Exception("Usage: dotnet run <IP> <PORT>");
        TcpClient client = new TcpClient();

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
        string received;

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
            Console.WriteLine($"Received {received}");
        }

        // if we arive here, we are in and the server knows our name

        // // Sending the initial message (join command or possibly something else and server will not accept us) to the server
        // Console.WriteLine("To Enter the server, type /join.");
        // string initialMessage = Console.ReadLine();
        // byte[] initialMessageBytes = Encoding.ASCII.GetBytes(initialMessage);
        // stream.Write(initialMessageBytes, 0, initialMessageBytes.Length);

        // // Receive acknowledgment from the server
        // byte[] acknowledgmentBytes = new byte[1024];
        // int bytesRead = stream.Read(acknowledgmentBytes, 0, acknowledgmentBytes.Length);
        // string acknowledgment = Encoding.ASCII.GetString(acknowledgmentBytes, 0, bytesRead);
        // Console.WriteLine($"Server response: {acknowledgment}");

        // if (acknowledgment.Equals("OK", StringComparison.OrdinalIgnoreCase))
        // {
        //     // Send the name to the server
        //     Console.WriteLine("Enter your name: ");
        //     string name = Console.ReadLine();

        //     // Sending the name to the server
        //     byte[] nameBytes = Encoding.ASCII.GetBytes(name);
        //     stream.Write(nameBytes, 0, nameBytes.Length);

        //     // Keep the connection open for further interaction
        //     while (true)
        //     {
        //         Console.WriteLine("Enter /get-list for name list or /leave to disconnect :)");
        //         string message = Console.ReadLine();

        //         // Sending the message to the server
        //         byte[] messageBytes = Encoding.ASCII.GetBytes(message);
        //         stream.Write(messageBytes, 0, messageBytes.Length);

        //         if (message.StartsWith("/leave", StringComparison.OrdinalIgnoreCase))
        //         {
        //             Console.WriteLine("Left the server :)");
        //             break;
        //         }
        //         else if (message.StartsWith("/get-list", StringComparison.OrdinalIgnoreCase))
        //         {
        //             // Receive the list of connected clients from the server
        //             byte[] responseBytes = new byte[1024];
        //             int responseLength = stream.Read(responseBytes, 0, responseBytes.Length);
        //             string clientList = Encoding.ASCII.GetString(responseBytes, 0, responseLength);
        //             Console.WriteLine($"Connected clients: {clientList}");
        //         }
        //         else{
        //             Console.WriteLine("Command not recognized. Please try again!");
        //         }

        //         // You can handle other server responses here if needed
        //     }
        // }else{

        // }
    }
}
