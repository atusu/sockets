using server;
using Server.Tests;
using MockServer = server.Server;

public class ServerTests
{
    [Fact]
    public void EmptyServer()
    {
        var server = new MockServer(8080);
        Assert.True(server.clients.Count() == 0);
    }

    [Fact]
    public void TestAddOneClient()
    {
        var server = new MockServer(8080);
        var client1 = new MockClientConnection();
        server.clients.Add(client1);
        Assert.True(server.clients.Count == 1);
        Assert.True(server.clients[0].ClientState == ClientState.INIT);
        client1.SetInput("lala");
        server.HandleClient(client1);
        Assert.Equal("ERR: you cannot join server using this command.", client1.GetResponse());
        Assert.True(server.clients[0].ClientState == ClientState.INIT);
        client1.SetInput("/join");
        server.HandleClient(client1);
        Assert.Equal("OK", client1.GetResponse());
        Assert.True(server.clients[0].ClientState == ClientState.JOINED);
        client1.SetInput("Marinela");
        server.HandleClient(client1);
        Assert.Equal("OK", client1.GetResponse());
        Assert.True(server.clients[0].ClientState == ClientState.CONNECTED);
    }
}