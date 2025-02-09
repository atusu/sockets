using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using server;

public class Server
{
    private static List<ClientData> clients = new List<ClientData>();

    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 8080);
        server.Start();

        Console.WriteLine("Server started.");

        while (true)
        {
            DateTime start = DateTime.Now;
            Console.WriteLine("Waiting for clients...");
            while ((DateTime.Now - start).TotalMilliseconds < 1000)
            {
                if (server.Pending()) {
                    clients.Add(new ClientData{
                        TcpClient = server.AcceptTcpClient(),
                    });
                    Console.WriteLine("Added new client :)");
                }
            }

            Console.WriteLine($"Checking existing clients... (Total: {clients.Count()})");
            for(var i=0 ; i < clients.Count(); i++)
            {
                if(!clients[i].IsConnected()) {
                    clients.RemoveAt(i);
                    i--;
                    continue;
                }
                HandleClient(clients[i]);
            }
        }
    }

    private static void SendToClient(NetworkStream stream, string message) {
        var responseBytes = Encoding.ASCII.GetBytes(message);
        stream.Write(responseBytes, 0, responseBytes.Length);
    }

    private static string Receive(NetworkStream stream) {
        byte[] acknowledgmentBytes = new byte[1024];
        int bytesRead = stream.Read(acknowledgmentBytes, 0, acknowledgmentBytes.Length);
        string acknowledgment = Encoding.ASCII.GetString(acknowledgmentBytes, 0, bytesRead);
        return acknowledgment;
    }

    public static void HandleClient(ClientData client)
    {
        if (!client.TcpClient.GetStream().DataAvailable) {
            return;
        }

        NetworkStream stream = client.TcpClient.GetStream();
        Console.WriteLine("Handling: " + client.Name);

        string message = Receive(stream);
        client.CommandHistory.Add(message);

        if(client.ClientState == ClientState.INIT) {
            SendToClient(stream, message == "/join" ? "OK" : "ERR: you cannot join server using this command.");
            if (message == "/join") {
                client.ClientState = ClientState.JOINED;
            }
            return;
        }

        if(client.ClientState == ClientState.JOINED) {
            if (ClientAlreadyExists(clients, message)) {
                SendToClient(stream, "ERR: name is already on server");
                return;
            }

            Console.WriteLine($"Client {message} is now connected to the server.");
            client.Name = message;
            client.ClientState = ClientState.CONNECTED;
            SendToClient(stream, "OK");
            return;
        }

        if(client.ClientState == ClientState.CONNECTED){
            if (message == "/leave") {
                client.TcpClient.Close();
                Console.WriteLine("Client disconnected: " + client.Name);
                return;
            }

            if(message == "/get-list") {
                string clientList = string.Join(", ", clients.Select(client => client.Name)) + "\n";
                SendToClient(stream, clientList);
                Console.WriteLine("The list was sent to the client.");
                return;
            }

            SendToClient(stream, "Invalid command, please check spelling!");
            return;
        }
    }

    public static bool ClientAlreadyExists(List<ClientData> clients, string name)
    {
        return clients.Select(c => c.Name == name).First();
    }
}
