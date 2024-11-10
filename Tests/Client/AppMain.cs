//
// (C) 2024 Takap.
//

namespace Takap.RpcIpc.Samples;

internal class AppMain
{
    static async Task Main(string[] _)
    {
        Console.WriteLine("[Start]");

        string pipeName = "pipe-name";

        using ClientSample css = new ClientSample(pipeName);
        Sample s = new Sample()
        {
            Code = 100,
            Message = "asdfasdf",
        };
        await css.Calc0(s);

        // 戻り値なし
        await css.Calc1(1);
        await css.Calc2(1, 2, 3);

        // 戻り値あり
        int ret = await css.Calc3(10);
        Console.WriteLine("ret=" + ret);

        Console.WriteLine("[END] Press any key.");
        Console.ReadKey();
    }
}
