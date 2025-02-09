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
                if (clients[i].TcpClient.GetStream().DataAvailable)
                {
                    Console.WriteLine("Data Available for client: " + i);
                    HandleClient(clients[i]);
                }
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

    public static void HandleClient(ClientData client) {
        NetworkStream stream = client.TcpClient.GetStream();
        Console.WriteLine("Handling: " + client.Name);

        string message = Receive(stream);
        client.CommandHistory.Add(message);

        if(client.ClientState == ClientState.INIT) {
            SendToClient(stream, message == "/join" ? "OK" : "ERR");
            if (message == "/join") {
                client.ClientState = ClientState.JOINED;
            }
            return;
        }

        if(client.ClientState == ClientState.JOINED) {
            if (ClientAlreadyExists(clients, message)) {
                SendToClient(stream, "ERR: name is already on server");
            }
            else {
                Console.WriteLine($"Client {message} is now connected to the server.");
                client.Name = message;
                SendToClient(stream, "OK");
            }
            return;
        }

        client.ClientState = ClientState.CONNECTED;
        SendToClient(stream, "OK and you are connected and fine.");

        // Console.WriteLine($"Received message: {message}");
        // if (message == "gogu") {
        //     SendToClient(stream, "OK");
        // }
        // else{
        //     SendToClient(stream, "hello");
        // }

        // byte[] responseBytes;
        // if(IsNewConnection(clientData)){
        //     if(message != "/join"){
        //         Console.WriteLine($"Unrecognized command received before user authentication: {message}");
        //         string response = "Cannot join the server - please check command :)\n";
        //         responseBytes = Encoding.ASCII.GetBytes(response);
        //         stream.Write(responseBytes, 0, responseBytes.Length);
        //         return;
        //     }

        //     Console.WriteLine("Client wants to join our server - asking for name");
        //     responseBytes = Encoding.ASCII.GetBytes("OK\n");
        //     stream.Write(responseBytes, 0, responseBytes.Length);
        //     clientData.CommandHistory.Add(message);
        //     return;
        // }
        // else if(UserHasJoinedButHasNoName(clientData))
        // {
        //     if(ClientAlreadyExists(clients, clientData)){
        //         responseBytes = Encoding.ASCII.GetBytes("Client with this name already exists");
        //         stream.Write(responseBytes, 0, responseBytes.Length);
        //         Console.WriteLine("Client with this name already exists");
        //         return;
        //     }

        //     responseBytes = Encoding.ASCII.GetBytes("OK\n");
        //     stream.Write(responseBytes, 0, responseBytes.Length);

        //     Console.WriteLine($"Name received: {message}");

        //     clientData.Name = message;
        //     clientData.CommandHistory.Add("Name received: " + clientData.Name);
        //     return;
        // }
        // else
        // {
        //     if(message == "/leave"){
        //         clients.RemoveAll(c => c.Name == clientData.Name);
        //         tcpClient.Close();
        //         Console.WriteLine("Client disconnected: " + clientData.Name);
        //         return;
        //     }

        //     if(message == "/get-list"){
        //         string clientList = string.Join(", ", clients.Select(client => client.Name));
        //         responseBytes = Encoding.ASCII.GetBytes(clientList + "\n");
        //         stream.Write(responseBytes, 0, responseBytes.Length);
        //         Console.WriteLine("The list was sent to the client.");
        //         return;
        //     }

        //     //Respond with an error message to other commands
        //     responseBytes = Encoding.ASCII.GetBytes("Invalid command\n");
        //     stream.Write(responseBytes, 0, responseBytes.Length);
        //     Console.WriteLine("Client sent an invalid command");
        //     return;
        // }
    }

    public static bool ClientAlreadyExists(List<ClientData> clients, string name)
    {
        return clients.Select(c => c.Name == name).First();
    }
}
