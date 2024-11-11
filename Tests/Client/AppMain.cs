//
// (C) 2024 Takap.
//

using System.Diagnostics;

namespace Takap.RpcIpc.Samples;

internal class AppMain
{
    static async Task Main(string[] _)
    {
        Console.WriteLine("[Start]");

        string pipeName = "pipe-name";

        using ClientSample client = new ClientSample(pipeName);

        for (int i = 0; i < 10; i++)
        {
            //Stopwatch sw = Stopwatch.StartNew();
            await client.Notify1(); // 高速で呼び出すと3回目以降メッセージが伝わらなくなる問題が発生中
            //sw.Stop();
            //Console.WriteLine("Notify1 exec time=" + sw.Elapsed.TotalMilliseconds);
            // 少し停止すると通信できる
            //Thread.Sleep(1);
        }
        for (int i = 0; i < 10; i++)
        {
            await client.Notify2(i); // 一度不通になると二度と通信できない
        }

        Sample s = new Sample()
        {
            Code = 100,
            Message = "asdfasdf",
        };

        for (int i = 0; i < 10; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            await client.Calc0(s);
            sw.Stop();
            Console.WriteLine("Calc0 exec time=" + sw.Elapsed.TotalMilliseconds);
            // 初回だけ動作速度が遅い
        }

        // 戻り値なし
        await client.Calc1(1);
        await client.Calc2(1, 2, 3);

        // 戻り値あり
        int ret = await client.Calc3(10);
        Console.WriteLine("ret=" + ret);

        Console.WriteLine("[END] Press any key.");
        Console.ReadKey();
    }
}
