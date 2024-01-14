using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{
    private static List<TcpClient> clients = new List<TcpClient>();
    private static Dictionary<TcpClient, string> clientNames = new Dictionary<TcpClient, string>();

    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 8080);
        server.Start();

        Console.WriteLine("Server started. Waiting for clients...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            clients.Add(client);

            Console.WriteLine("Client connected.");

            // Handle the client in a separate thread or using async/await
            HandleClient(client);
        }
    }

    static void HandleClient(object clientObj)
    {
        TcpClient tcpClient = (TcpClient)clientObj;
        NetworkStream stream = tcpClient.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead;

        bool isRequestReceived = false;
        try
        {
            // Wait for the initial message from the client
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            string initialMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"{initialMessage} (Sent from Client)");

            // Parse the initial message for IP and port (you can adjust this based on your actual protocol)
            string[] addressParts = initialMessage.Split(' ');

            if (addressParts.Length == 3 && addressParts[0].Equals("/join", StringComparison.OrdinalIgnoreCase))
            {
                string joinIp = addressParts[1];
                int joinPort;

                if (int.TryParse(addressParts[2], out joinPort))
                {
                    Console.WriteLine($"Client wants to join our server");

                    // Respond with acknowledgment (you can add more logic here)
                    string response = "OK";
                    byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);

                    isRequestReceived = true;
                }
                else
                {
                    Console.WriteLine("Invalid port number.");
                }
            }
            else
            {
                Console.WriteLine("Invalid initial message format.");
            }

            Array.Clear(buffer, 0, buffer.Length);

            // Continue handling the client as before once they are considered connected
            while (isRequestReceived && (bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                if (string.IsNullOrWhiteSpace(message))
                {
                    continue; // Skip empty messages
                }

                Console.WriteLine($"{message} (received from Client)");

                if (message.StartsWith("/leave", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle leave command
                    break;
                }
                else if (message.StartsWith("/get-list", StringComparison.OrdinalIgnoreCase))
                {
                    // Respond with the list of connected client names
                    string clientList = string.Join(", ", clientNames.Values);
                    byte[] responseBytes = Encoding.ASCII.GetBytes(clientList);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                    Console.WriteLine("The list was sent to the client.");
                }
                else
                {
                    // Assuming the message contains the client's name
                    // Add the client's name to the dictionary
                    clientNames[tcpClient] = message;

                    // Handle other commands or regular messages for the connected client
                    // You can extend this part based on your application logic
                }

                // Clear buffer for the next iteration
                Array.Clear(buffer, 0, buffer.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            // Remove the client from the list when the connection is closed
            clients.Remove(tcpClient);
            Console.WriteLine("Client disconnected.");
        }
    }
}
