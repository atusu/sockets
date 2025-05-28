using System.Net.Sockets;
using server;
using Xunit;

public class ServerTests
{
    [Fact]
    public void EmptyServer()
    {
        var server = new Server(8080);
        Assert.True(server.clients.Count() == 0);
    }

    [Fact]
    public void AddNewClient()
    {
        var server = new Server(8080);
        var client1 = new MockedClient { TcpClient = null };
        server.clients.Add(client1);
        Assert.True(server.clients.Count == 1);
        Assert.True(server.clients[0].ClientState == ClientState.INIT);
        var response = client1.Write(server, "lala");
        Assert.True(response == "ERR: you cannot join server using this command.");
        Assert.True(server.clients[0].ClientState == ClientState.INIT);
        response = client1.Write(server, "/join");
        Assert.True(response == "OK");
        Assert.True(server.clients[0].ClientState == ClientState.JOINED);
        response = client1.Write(server, "Marinela");
        Assert.True(response == "OK");
        Assert.True(server.clients[0].ClientState == ClientState.CONNECTED);
    }

    public class MockedClient : ClientData
    {
        public string Write(Server server, string message)
        {
            //TcpClient.WriteMessage(message);
            server.HandleClient(this);
            return "";
            //return TcpClient.ReadMessage();
        }
    }
}