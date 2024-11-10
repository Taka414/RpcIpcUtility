//
// (C) 2024 Takap.
//

using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using Takap.RpcIpc.Samples;

namespace Takap
{
    internal class Program
    {
        static void Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
            
            LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);

            PipeSecurity pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(
                new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                       PipeAccessRights.FullControl,
                       AccessControlType.Allow));

            ServerSample server = new("pipe-name", pipeSecurity);

            // サービスの登録
            builder.Services.AddHostedService<MyService>();

            // プロセス間通信用のサーバーオブジェクトを登録する
            builder.Services.AddSingleton(typeof(ServerSample), server);

            var host = builder.Build();
            host.Run();
        }
    }
}