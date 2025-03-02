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
}