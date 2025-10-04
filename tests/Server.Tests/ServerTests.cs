using server;
using Server.Tests;
using File = server.File;

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
    public void TestJoinTheServerEntireFlow()
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

    [Fact]
    public void TestShareUnshareFilesFlow()
    {
        var server = new server.Server(8080);

        var client1 = new MockClientConnection();
        var client2 = new MockClientConnection();
        
        server.clients.Add(client1);
        server.clients.Add(client2);

        HandleClientCommand("/join", server, client1);
        HandleClientCommand("/join", server, client2);
        
        HandleClientCommand("Marinela", server, client1);
        HandleClientCommand("Gigel", server, client2);

        var file1 = new File { Name = "file1.txt", Size = 248, Hash = "a1b2c3d4e5f67890a1b2c3d4e5f67890" };
        var file2 = new File { Name = "file2.txt", Size = 124, Hash = "f0e1d2c3b4a59687f0e1d2c3b4a59687" };

        Assert.Equal("INFO: user Marinela shared no files", HandleClientCommand("/list-files Marinela", server, client2));
        Assert.Equal("OK", HandleClientCommand($"/share {file1.Name}", server, client1));

        Assert.True(server.clients[0].ClientState == ClientState.GET_FILE_DETAILS);  
        Assert.Equal("ERR: size must be a positive number", HandleClientCommand($"-875 {file1.Hash}", server, client1));
        Assert.Equal("ERR: invalid hash", HandleClientCommand($"{file1.Size} abc", server, client1));
        Assert.Equal("OK", HandleClientCommand($"{file1.Size} {file1.Hash}", server, client1));
        Assert.Equal($"({file1.Name}, {file1.Size}, {file1.Hash})", HandleClientCommand("/list-files Marinela", server, client2));
        
        Assert.Equal("ERR: file already shared", HandleClientCommand($"/share {file1.Name}", server, client1));
        Assert.Equal("ERR: no file provided", HandleClientCommand("/share ", server, client1));
        
        Assert.Equal("OK", HandleClientCommand($"/share {file2.Name}", server, client1));
        Assert.Equal("OK", HandleClientCommand($"{file2.Size} {file2.Hash}", server, client1));
        Assert.Equal($"({file1.Name}, {file1.Size}, {file1.Hash})\n({file2.Name}, {file2.Size}, {file2.Hash})", 
            HandleClientCommand("/list-files Marinela", server, client2));
        
        Assert.Equal("OK", HandleClientCommand("/unshare file1.txt", server, client1));
        Assert.Equal($"({file2.Name}, {file2.Size}, {file2.Hash})", HandleClientCommand("/list-files Marinela", server, client2));
        
        Assert.Equal("ERR: no such user on server", HandleClientCommand("/list-files InexistentUser", server, client2));
        
        HandleClientCommand("/leave", server, client1);
        Assert.Equal("ERR: no such user on server", HandleClientCommand("/list-files Marinela", server, client2));
    }
    
    public string HandleClientCommand(string input, server.Server server, MockClientConnection client)
    {
        client.SetInput(input);
        server.HandleClient(client);
        return client.GetResponse();
    }
}