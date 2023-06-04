using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    static void Main()
    {
        // Set the IP address and port for the server
        string ipAddress = "127.0.0.1";
        int port = 8080;

        // Create a TCP listener
        TcpListener listener = new TcpListener(IPAddress.Parse(ipAddress), port);
        listener.Start();
        Console.WriteLine("Server started. Waiting for connections...");

        while (true)
        {
            // Accept client connections
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected.");

            // Start a new thread to handle the client
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }

        // Close the listener
        listener.Stop();
    }

    static void HandleClient(TcpClient client)
    {
        try
        {
            // Get the network stream from the client
            NetworkStream stream = client.GetStream();

            // Read incoming messages
            byte[] buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Message received: {message}");
            }
        }
        catch (IOException e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            // Close the client connection
            client.Close();
        }
    }
}
