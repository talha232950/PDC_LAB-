// 232950
// Muhammad Talha 
// BSCSev - 7 - C 


// TASK 01

using System;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--child")
            RunAsChild();
        else
            RunAsParent();
    }

    static void RunAsChild()
    {
        Console.WriteLine($"[Child] PID = {Environment.ProcessId}");
        int counter = 100;
        counter += 50;
        Console.WriteLine($"[Child] final counter = {counter}");
    }

    static void RunAsParent()
    {
        Console.WriteLine($"[Parent] PID = {Environment.ProcessId}");
        int counter = 100;
        counter += 1;

        string exe = Environment.ProcessPath!;
        var psi = new ProcessStartInfo(exe);
        psi.ArgumentList.Add("--child");
        psi.UseShellExecute = false;

        using Process? child = Process.Start(psi);
        child!.WaitForExit();

        Console.WriteLine($"[Parent] final counter = {counter}");
        Console.WriteLine("[Parent] Parent and child counters were modified independently.");
    }
}


// TASK 02

using System;
using System.Threading;

class Program
{
    static long[] data = new long[10_000_000];
    static long[] partialSums = null!;
    static int numWorkers;

    static void SumSlice(object? arg)
    {
        int idx = (int)arg!;
        int sliceSize = data.Length / numWorkers;
        int start = idx * sliceSize;
        int end = (idx == numWorkers - 1) ? data.Length : start + sliceSize;

        long sum = 0;
        for (int i = start; i < end; i++)
            sum += data[i];

        partialSums[idx] = sum;
    }

    static void Main()
    {
        for (int i = 0; i < data.Length; i++)
            data[i] = i + 1;

        numWorkers = Environment.ProcessorCount;
        partialSums = new long[numWorkers];

        Thread[] threads = new Thread[numWorkers];

        for (int i = 0; i < numWorkers; i++)
        {
            int idx = i;
            threads[i] = new Thread(() => SumSlice(idx));
            threads[i].Start();
        }

        for (int i = 0; i < numWorkers; i++)
            threads[i].Join();

        long threadedTotal = 0;
        foreach (long partial in partialSums)
            threadedTotal += partial;

        long sequentialTotal = 0;
        foreach (long value in data)
            sequentialTotal += value;

        Console.WriteLine($"Array size: {data.Length:N0}");
        Console.WriteLine($"Worker threads: {numWorkers}");
        Console.WriteLine($"Threaded total: {threadedTotal:N0}");
        Console.WriteLine($"Sequential total: {sequentialTotal:N0}");
        Console.WriteLine($"Match: {threadedTotal == sequentialTotal}");
    }
}



// TASK 03


using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    const int Iterations = 50;
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--child")
        {
            return;
        }
        string target = Environment.ProcessPath!;
        Stopwatch processStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            psi.ArgumentList.Add("--child");
            using Process p = Process.Start(psi)!;
            p.WaitForExit();
        }
        processStopwatch.Stop();
        Stopwatch threadStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            Thread t = new Thread(() =>
            {
                // Trivial work
            });
            t.Start();
            t.Join();
        }
        threadStopwatch.Stop();
        double avgProcessMs =
            processStopwatch.Elapsed.TotalMilliseconds / Iterations;
        double avgThreadMs =
            threadStopwatch.Elapsed.TotalMilliseconds / Iterations;
        double ratio =
            avgProcessMs / avgThreadMs;

        Console.WriteLine("==========================================");
        Console.WriteLine("       TASK 3 - CREATION OVERHEAD");
        Console.WriteLine("==========================================");
        Console.WriteLine($"Processes created: {Iterations}");
        Console.WriteLine($"Threads created:   {Iterations}");
        Console.WriteLine();
        Console.WriteLine(
            $"Average process creation time: {avgProcessMs:F3} ms");
        Console.WriteLine(
            $"Average thread creation time:  {avgThreadMs:F3} ms");
        Console.WriteLine(
            $"Process/thread ratio: {ratio:F1}x");
        Console.WriteLine("==========================================");
    }
}






// TASK 04
using System;
using System.Threading;

class Program
{
    static void Worker()
    {
        Thread.Sleep(200);
    }

    static void Main()
    {
        Thread t = new Thread(Worker);

        Console.WriteLine($"After creation: {t.ThreadState}");
        // Conceptually: New

        t.Start();
        Console.WriteLine($"Immediately after Start(): {t.ThreadState}");
        // Conceptually: Runnable/Ready or Running

        Thread.Sleep(50);
        Console.WriteLine($"While worker is sleeping: {t.ThreadState}");
        // Conceptually: Blocked/Waiting

        t.Join();
        Console.WriteLine($"After Join() completes: {t.ThreadState}");
        // Conceptually: Terminated
    }
}
