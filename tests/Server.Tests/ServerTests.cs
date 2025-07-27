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
        var client = new MockClientConnection();
        server.clients.Add(client);
        Assert.True(server.clients.Count == 1);
        Assert.True(server.clients[0].ClientState == ClientState.INIT);
        client.SetInput("lala");
        server.HandleClient(client);
        Assert.Equal("ERR: you cannot join server using this command.", client.GetResponse());
        Assert.True(server.clients[0].ClientState == ClientState.INIT);
        client.SetInput("/join");
        server.HandleClient(client);
        Assert.Equal("OK", client.GetResponse());
        Assert.True(server.clients[0].ClientState == ClientState.JOINED);
        client.SetInput("Marinela");
        server.HandleClient(client);
        Assert.Equal("OK", client.GetResponse());
        Assert.True(server.clients[0].ClientState == ClientState.CONNECTED);
    }
}