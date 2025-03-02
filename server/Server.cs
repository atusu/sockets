using System.Net;
using System.Net.Sockets;
using System.Text;
using server;

public class Server{
    public List<ClientData> _clients {get; set;}
    public int _port {get;set;}

    public Server(int port){
        _port = port;
        _clients = new List<ClientData>();
    }

    public void ServerInit(){
        TcpListener server = new TcpListener(IPAddress.Any, _port);
        server.Start();

        Console.WriteLine("Server started.");

        while (true)
        {
            DateTime start = DateTime.Now;
            Console.WriteLine("Waiting for clients...");
            while ((DateTime.Now - start).TotalMilliseconds < 1000)
            {
                if (server.Pending()) {
                    _clients.Add(new ClientData{
                        TcpClient = server.AcceptTcpClient(),
                    });
                    Console.WriteLine("Added new client :)");
                }
            }

            Console.WriteLine($"Checking existing clients... (Total: {_clients.Count()})");
            for(var i=0 ; i < _clients.Count(); i++)
            {
                if(!_clients[i].IsConnected()) {
                    _clients.RemoveAt(i);
                    i--;
                    continue;
                }
                HandleClient(_clients[i]);
            }
        }
    }
    private void SendToClient(NetworkStream stream, string message)
    {
        // the \n here is needed fot the nc test so we get the message on a new line always
        message = message[message.Count()-1].ToString() == "\n" ? message : message + "\n";
        var responseBytes = Encoding.ASCII.GetBytes(message);
        stream.Write(responseBytes, 0, responseBytes.Length);
    }

    private string Receive(NetworkStream stream)
    {
        byte[] acknowledgmentBytes = new byte[1024];
        int bytesRead = stream.Read(acknowledgmentBytes, 0, acknowledgmentBytes.Length);
        string acknowledgment = Encoding.ASCII.GetString(acknowledgmentBytes, 0, bytesRead);
        return acknowledgment;
    }

    public bool ClientAlreadyExists(List<ClientData> clients, string name)
    {
        foreach (var client in clients) {
            if (client.Name == name) {
                return true;
            }
        }
        return false;
    }

    public void HandleClient(ClientData client)
    {
        if (!client.TcpClient.GetStream().DataAvailable) {
            return;
        }

        NetworkStream stream = client.TcpClient.GetStream();

        string message = Receive(stream);
        // some clients end a \n as well, like for example the netcat client (integration test). We trim it.
        message = message[message.Count()-1].ToString() == "\n" ? message.Substring(0, message.Count()-1) : message;
        Console.WriteLine($"Handling client message: {message}");
        client.CommandHistory.Add(message);

        if(client.ClientState == ClientState.INIT) {
            SendToClient(stream, message == "/join" ? "OK" : "ERR: you cannot join server using this command.");
            if (message == "/join") {
                client.ClientState = ClientState.JOINED;
            }
            return;
        }

        if(client.ClientState == ClientState.JOINED) {
            if (ClientAlreadyExists(_clients, message)) {
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
                _clients.RemoveAll(c => c.Name == client.Name);
                Console.WriteLine("Client disconnected: " + client.Name);
                return;
            }

            if(message == "/get-list") {
                string clientList = string.Join(", ", _clients.Select(client => client.Name)) + "\n";
                SendToClient(stream, clientList);
                Console.WriteLine("The list was sent to the client.");
                return;
            }

            SendToClient(stream, "Invalid command, please check spelling!");
            return;
        }
    }
}