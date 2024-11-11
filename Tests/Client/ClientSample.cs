//
// (C) 2024 Takap.
//

namespace Takap.RpcIpc.Samples;

/// <summary>
/// クライアントの実装例
/// </summary>
public class ClientSample(string pipeName) : IpcRpcClient(pipeName)
{
    public ValueTask Notify1()
    {
        return PublishOne(200);
    }
    public ValueTask Notify2(int value)
    {
        return PublishOne(201, value);
    }

    // 戻り値なし
    public async ValueTask Calc0(Sample s, CancellationToken ct = default)
    {
        await Publish(0, s, 3.0, ct);
    }
    public async ValueTask Calc1(int a, CancellationToken ct = default)
    {
        await Publish(1, a, 3.0, ct);
    }
    public async ValueTask Calc2(int a, int b, int c, CancellationToken ct = default)
    {
        await Publish(2, a, b, c, 3.0, ct);
    }

    // 戻り値あり
    public async Task<int> Calc3(int a, CancellationToken ct = default)
    {
        return await Publish<int, int>(100, a, 999.0, ct);
    }
}
