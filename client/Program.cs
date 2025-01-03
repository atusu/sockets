using System;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Text;

class Client
{
    static void Main(string[] args)
    {
        if(args.Length != 2)
            throw new Exception("Usage: dotnet run <IP> <PORT>");
        Console.WriteLine("To Enter the server, type /join.");
        string initialMessage = Console.ReadLine();
        TcpClient client = new TcpClient();

        try{
            client.Connect(args[0], int.Parse(args[1])); // Connect to the server
        }
        catch
        {
            Console.WriteLine("Error: Couldn't connect to the server. It probably isn't on. :)");
            return;
        }

        NetworkStream stream = client.GetStream();

        // Sending the initial message (join command or possibly something else and server will not accept us) to the server
        byte[] initialMessageBytes = Encoding.ASCII.GetBytes(initialMessage);
        stream.Write(initialMessageBytes, 0, initialMessageBytes.Length);

        // Receive acknowledgment from the server
        byte[] acknowledgmentBytes = new byte[1024];
        int bytesRead = stream.Read(acknowledgmentBytes, 0, acknowledgmentBytes.Length);
        string acknowledgment = Encoding.ASCII.GetString(acknowledgmentBytes, 0, bytesRead);
        Console.WriteLine($"Server response: {acknowledgment}");

        if (acknowledgment.Equals("OK", StringComparison.OrdinalIgnoreCase))
        {
            // Send the name to the server
            Console.WriteLine("Enter your name: ");
            string name = Console.ReadLine();

            // Sending the name to the server
            byte[] nameBytes = Encoding.ASCII.GetBytes(name);
            stream.Write(nameBytes, 0, nameBytes.Length);

            // Keep the connection open for further interaction
            while (true)
            {
                Console.WriteLine("Enter /get-list for name list or /leave to disconnect :)");
                string message = Console.ReadLine();

                // Sending the message to the server
                byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                stream.Write(messageBytes, 0, messageBytes.Length);

                if (message.StartsWith("/leave", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Left the server :)");
                    break;
                }
                else if (message.StartsWith("/get-list", StringComparison.OrdinalIgnoreCase))
                {
                    // Receive the list of connected clients from the server
                    byte[] responseBytes = new byte[1024];
                    int responseLength = stream.Read(responseBytes, 0, responseBytes.Length);
                    string clientList = Encoding.ASCII.GetString(responseBytes, 0, responseLength);
                    Console.WriteLine($"Connected clients: {clientList}");
                }
                else{
                    Console.WriteLine("Command not recognized. Please try again!");
                }

                // You can handle other server responses here if needed
            }
        }else{

        }
    }
}
