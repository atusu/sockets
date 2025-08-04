using server;
using Server.Tests;

public class ServerTests
{
    [Fact]
    public void EmptyServer()
    {
        var server = new server.Server(8080);
        Assert.True(server.clients.Count() == 0);
    }

    [Fact]
    public void TestAddOneClient()
    {
        var server = new server.Server(8080);
        var client = new MockClientConnection();
        server.clients.Add(client);
        Assert.True(server.clients.Count == 1);
        Assert.True(server.clients[0].ClientState == ClientState.INIT);
        Assert.Equal("ERR: you cannot join server using this command.", HandleClientCommand("lala", server, client));
        Assert.True(server.clients[0].ClientState == ClientState.INIT);
        Assert.Equal("OK", HandleClientCommand("/join", server, client));
        Assert.True(server.clients[0].ClientState == ClientState.JOINED);
        Assert.Equal("OK", HandleClientCommand("Marinela", server, client));
        Assert.True(server.clients[0].ClientState == ClientState.CONNECTED);
    }

    [Fact]
    public void TestTheEntireFlow()
    {
        var server = new server.Server(8080);
        
        var client1 = new MockClientConnection();
        var client2 = new MockClientConnection();
        
        server.clients.Add(client1);
        server.clients.Add(client2);
        
        Assert.True(server.clients.Count == 2);
        
        Assert.True(server.clients[0].ClientState == ClientState.INIT);
        Assert.Equal("OK", HandleClientCommand("/join", server, client1));
        
        Assert.True(server.clients[1].ClientState == ClientState.INIT);
        Assert.Equal("OK", HandleClientCommand("/join", server, client2));
        
        Assert.True(server.clients[0].ClientState == ClientState.JOINED);
        Assert.Equal("OK", HandleClientCommand("Marinela", server, client1));
        
        Assert.True(server.clients[1].ClientState == ClientState.JOINED);
        Assert.Equal("OK", HandleClientCommand("Gigel", server, client2));
        
        Assert.True(server.clients[0].ClientState == ClientState.CONNECTED);        
        Assert.True(server.clients[1].ClientState == ClientState.CONNECTED);

        Assert.Equal("Marinela, Gigel", HandleClientCommand("/get-list", server, client1));
        Assert.Equal("Marinela, Gigel", HandleClientCommand("/get-list", server, client2));

        HandleClientCommand("/leave", server, client1);
        Assert.Single(server.clients);
        
        Assert.Equal("Gigel", HandleClientCommand("/get-list", server, client2));
        
        HandleClientCommand("/leave", server, client2);
        Assert.True(!server.clients.Any());
    }
    public string HandleClientCommand(string input, server.Server server, MockClientConnection client)
    {
        client.SetInput(input);
        server.HandleClient(client);
        return client.GetResponse();
    }
}