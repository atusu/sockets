# SOCKETS

## Running the tests

### Bash end to end test
For Windows use Git Bash
```bash
cd [path]/Sockets/tests/end-to-end/bash
bash two_clients_join.sh
```

### .NET Tests (WIP)
```powershell
dotnet add package Microsoft.NET.Test.Sdk --version 17.13.0
dotnet add package xunit --version 2.9.3
dotnet add package xunit.runner.visualstudio --version 3.0.2

# in tests/server
dotnet new xunit -n Server.Tests
cd Server.Tests

dotnet add reference ../../server/server.csproj

```