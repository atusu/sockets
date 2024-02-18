using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

public class ClientData
{
    public TcpClient TcpClient { get; set; }
    public NetworkStream NetworkStream { get; set; }
    public string Name { get; set; }
    public List<string> CommandHistory {get; set;}
}

public class Server
{
    private static List<ClientData> clients = new List<ClientData>();

    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 8080);
        server.Start();

        Console.WriteLine("Server started. Waiting for clients...");

        while (true)
        {
            while (server.Pending())
            {
                TcpClient newClient = server.AcceptTcpClient();

                // Create a new instance of the ClientData class for the new client
                ClientData newClientData = new ClientData
                {
                    TcpClient = newClient,
                    NetworkStream = newClient.GetStream(),
                    Name = string.Empty, 
                    CommandHistory = new List<string>()
                };

                // Add the new client to the list
                clients.Add(newClientData);

                // Handle the new client in a separate thread or using async/await
                ThreadPool.QueueUserWorkItem(HandleClient, newClientData);
            }


            // Check for incoming messages from existing clients
            foreach (ClientData clientData in clients)
            {
                if (clientData.NetworkStream.DataAvailable)
                {
                    // Handle incoming messages for existing clients
                    ThreadPool.QueueUserWorkItem(HandleClient, clientData);
                }
                // TODO: check if clients disconnected and remove it from the list of clients.
            }

            // You can add a delay to reduce CPU usage
            Thread.Sleep(100);
        }

    }

    static void HandleClient(object state)
    {
        if (state is ClientData clientData)
        {
            TcpClient tcpClient = clientData.TcpClient;
            NetworkStream stream = clientData.NetworkStream;

            Console.WriteLine("Handling: " + clientData.Name);
            
            byte[] buffer = new byte[1024];
            int bytesRead;

            //bool isRequestReceived = false; 

            try
            {
                // Wait for the initial message from the client
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();

                //If empty command history, we check for valid join command
                if(clientData.CommandHistory.Count == 0)
                {
                    //Check command for new client
                    if(message == "/join")
                    {
                        Console.WriteLine("Client wants to join our server - asking for name");

                        // Respond with acknowledgment (you can add more logic here)
                        string response = "OK";
                        byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseBytes, 0, responseBytes.Length);
                        //isRequestReceived = true;
                        //Add command to client history
                        clientData.CommandHistory.Add(message);
                    }
                    else
                    {
                        Console.WriteLine($"Unrecognized command received: {message}");
                        string response = "Cannot join the server - please check command :)";
                        byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseBytes, 0, responseBytes.Length);
                    }                    
                }
                else if(clientData.CommandHistory.Count == 1)
                {
                    Console.WriteLine($"Name received: {message}");
                    //Save client name
                    clientData.Name = message;
                    //Save command in history
                    clientData.CommandHistory.Add("Name received");
                }
                else 
                {
                    if(message == "/leave")
                    {
                        //remove client from list according to command
                        clients.RemoveAll(c => c.Name == clientData.Name); // TODO: client is guaranteed to have unique name
                        Console.WriteLine("Client disconnected.");
                        foreach (var client in clients)
                            Console.WriteLine(client.Name);
                        tcpClient.Close();
                    }
                    else if(message == "/get-list")
                    {
                        //Respond with the list of connected client names
                        string clientList = string.Join(", ", clients.Select(client => client.Name));
                        byte[] responseBytes = Encoding.ASCII.GetBytes(clientList);
                        stream.Write(responseBytes, 0, responseBytes.Length);
                        Console.WriteLine("The list was sent to the client.");
                    }
                    else 
                    {
                        //Respond with an error message to other commands
                        byte[] responseBytes = Encoding.ASCII.GetBytes("Invalid command");
                        stream.Write(responseBytes, 0, responseBytes.Length);
                        Console.WriteLine("Client sent an invalid command");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {   

            }
        }    
    }
}
