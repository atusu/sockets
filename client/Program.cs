// See https://aka.ms/new-console-template for more information
using System;
using System.Net.Sockets;
using System.Text;

class Client
{
    static void Main()
    {
        string ipAddress = "127.0.0.1";
        int port = 8080;

        TcpClient client = new TcpClient();
        client.Connect(ipAddress, port);
        Console.WriteLine("Connected to server.");

        NetworkStream stream = client.GetStream();

        while (true)
        {
            // Read input from the user
            Console.Write("Enter a message: ");
            string message = Console.ReadLine();

            // Convert the message to bytes
            byte[] data = Encoding.ASCII.GetBytes(message);

            // Send the message to the server
            stream.Write(data, 0, data.Length);
            Console.WriteLine("Message sent: " + message);
        }

        client.Close();
    }
}