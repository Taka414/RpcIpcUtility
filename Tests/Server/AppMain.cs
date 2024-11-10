//
// (C) 2024 Takap.
//

using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Takap.RpcIpc.Samples;

internal class AppMain
{
    static async Task Main(string[] _)
    {
        Console.WriteLine("[Start] Server");

        string pipeName = "pipe-name";

        PipeSecurity pipeSecurity = new PipeSecurity();
        pipeSecurity.AddAccessRule(
            new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                               PipeAccessRights.FullControl,
                               AccessControlType.Allow));


        ServerSample s = new(pipeName, pipeSecurity);

        Console.WriteLine("Start");
        Console.ReadLine();

        Console.WriteLine("[END] Server");

        return;
    }
}
