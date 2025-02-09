using System.Net;
using System.Net.Sockets;
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


    public static void HandleClient(ClientData client) {
        NetworkStream stream = client.TcpClient.GetStream();

        Console.WriteLine("Handling: " + client.Name);

        byte[] buffer = new byte[1024];
        int bytesRead;

        bytesRead = stream.Read(buffer, 0, buffer.Length);
        string message = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();

        Console.WriteLine($"Received message: {message}");

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

    public static bool UserHasJoinedButHasNoName(ClientData clientData){
        if(clientData.CommandHistory.Count == 1)
            return true;

        return false;
    }

    public static bool ClientAlreadyExists(List<ClientData> clients, ClientData clientData){
        if(clients.Select(c => c.Name == clientData.Name).First())
            return true;

        return false;
    }
}
