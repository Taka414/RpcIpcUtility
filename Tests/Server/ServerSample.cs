//
// (C) 2024 Takap.
//

using System.IO.Pipes;

namespace Takap.RpcIpc.Samples;

/// <summary>
/// サーバー側のサンプル実装
/// </summary>
public class ServerSample(string pipeName, PipeSecurity pipeSecurity = null) : IpcRpcServer(pipeName, pipeSecurity)
{
    protected override void RegisterMethods()
    {
        Register<Sample>(0, Calc0);
        Register<int>(1, Calc1);
        Register<int, int, int>(2, Calc2);
        Register<int, int>(100, Calc3);
    }

    public async ValueTask Calc0(Sample s)
    {
        Console.WriteLine("Calc0");
    }
    public async ValueTask Calc1(int a)
    {
        Console.WriteLine("Calc1");
    }
    public async ValueTask Calc2(int a, int b, int c)
    {
        Console.WriteLine("Calc2");
    }
    public async ValueTask<int> Calc3(int a)
    {
        Console.WriteLine("Calc3");
        return a;

    }
}
