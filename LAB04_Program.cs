// using System;
// using System.Threading;

// class Program
// {
//     const int NumThreads = 4;
//     const int IncrementsPerThread = 1_000_000;

//     static long counter = 0;

//     static void Worker()
//     {
//         for (int i = 0; i < IncrementsPerThread; i++)
//         {
//             counter++; // No synchronization
//         }
//     }

//     static void Main()
//     {
//         Thread[] threads = new Thread[NumThreads];

//         for (int i = 0; i < NumThreads; i++)
//         {
//             threads[i] = new Thread(Worker);
//             threads[i].Start();
//         }

//         for (int i = 0; i < NumThreads; i++)
//         {
//             threads[i].Join();
//         }

//         long expected = (long)NumThreads * IncrementsPerThread;

//         Console.WriteLine($"Expected value: {expected}");
//         Console.WriteLine($"Actual value:   {counter}");
//         Console.WriteLine($"Lost updates:   {expected - counter}");
//         Console.WriteLine($"Logical processors: {Environment.ProcessorCount}");
//     }
// }





//TASK - 02
    
// using System;
// using System.Threading;

// class Program
// {
//     const int NumThreads = 4;
//     const int IncrementsPerThread = 250_000;

//     static int counter = 0;
//     static readonly object gate = new object();
//     static bool useLock = false;

//     static int[][] seenLog = new int[NumThreads][];

//     static void Worker(int id)
//     {
//         int[] log = seenLog[id];

//         for (int i = 0; i < IncrementsPerThread; i++)
//         {
//             if (useLock)
//             {
//                 lock (gate)
//                 {
//                     int seen = counter;
//                     log[i] = seen;
//                     counter = seen + 1;
//                 }
//             }
//             else
//             {
//                 int seen = counter;
//                 log[i] = seen;
//                 counter = seen + 1;
//             }
//         }
//     }

//     static void Main(string[] args)
//     {
//         useLock = args.Length > 0 && args[0] == "--lock";

//         int total = NumThreads * IncrementsPerThread;
//         Thread[] threads = new Thread[NumThreads];

//         for (int t = 0; t < NumThreads; t++)
//         {
//             seenLog[t] = new int[IncrementsPerThread];

//             int id = t;
//             threads[t] = new Thread(() => Worker(id));
//             threads[t].Start();
//         }

//         foreach (Thread th in threads)
//         {
//             th.Join();
//         }

//         int[] readCount = new int[total + 1];

//         for (int t = 0; t < NumThreads; t++)
//         {
//             for (int i = 0; i < IncrementsPerThread; i++)
//             {
//                 readCount[seenLog[t][i]]++;
//             }
//         }

//         int collisions = 0;

//         for (int v = 0; v <= total; v++)
//         {
//             if (readCount[v] > 1)
//             {
//                 collisions += readCount[v] - 1;
//             }
//         }

//         string mode = useLock ? "with lock" : "no synchronization";

//         Console.WriteLine($"Mode: {mode}");
//         Console.WriteLine($"Total increments: {total}");
//         Console.WriteLine($"Final counter: {counter}");
//         Console.WriteLine($"Lost updates: {total - counter}");
//         Console.WriteLine($"Collisions: {collisions}");

//         Console.WriteLine("\nFirst five colliding values:");

//         int printed = 0;

//         for (int v = 0; v <= total && printed < 5; v++)
//         {
//             if (readCount[v] > 1)
//             {
//                 Console.WriteLine(
//                     $"\nValue {v} was loaded {readCount[v]} times:");

//                 for (int t = 0; t < NumThreads; t++)
//                 {
//                     for (int i = 0; i < IncrementsPerThread; i++)
//                     {
//                         if (seenLog[t][i] == v)
//                         {
//                             Console.WriteLine(
//                                 $"  Thread {t}, loop index {i}");
//                         }
//                     }
//                 }

//                 printed++;
//             }
//         }

//         if (printed == 0)
//         {
//             Console.WriteLine("No collisions were observed.");
//         }
//     }
// }






// Task - 03


// using System;
// using System.Threading;

// class Program
// {
//     const int NumThreads = 4;
//     const int IncrementsPerThread = 1_000_000;

//     static long counter = 0;
//     static readonly object counterLock = new object();

//     static void Worker()
//     {
//         for (int i = 0; i < IncrementsPerThread; i++)
//         {
//             lock (counterLock)
//             {
//                 counter++;
//             }
//         }
//     }

//     static void Main()
//     {
//         Thread[] threads = new Thread[NumThreads];

//         for (int i = 0; i < NumThreads; i++)
//         {
//             threads[i] = new Thread(Worker);
//             threads[i].Start();
//         }

//         foreach (Thread thread in threads)
//         {
//             thread.Join();
//         }

//         long expected = (long)NumThreads * IncrementsPerThread;

//         Console.WriteLine($"Expected value: {expected}");
//         Console.WriteLine($"Actual value:   {counter}");
//         Console.WriteLine($"Correct:        {counter == expected}");
//     }
// }




// Task - 04

using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    const int NumThreads = 4;
    const int IncrementsPerThread = 1_000_000;

    static long counter = 0;
    static readonly object counterLock = new object();

    static void LockWorker()
    {
        for (int i = 0; i < IncrementsPerThread; i++)
        {
            lock (counterLock)
            {
                counter++;
            }
        }
    }

    static void InterlockedWorker()
    {
        for (int i = 0; i < IncrementsPerThread; i++)
        {
            Interlocked.Increment(ref counter);
        }
    }

    static double RunVersion(string name, ThreadStart worker)
    {
        counter = 0;

        Thread[] threads = new Thread[NumThreads];

        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < NumThreads; i++)
        {
            threads[i] = new Thread(worker);
            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        sw.Stop();

        long expected = (long)NumThreads * IncrementsPerThread;
        double ms = sw.Elapsed.TotalMilliseconds;

        Console.WriteLine(
            $"[{name}] expected={expected} actual={counter} " +
            $"correct={counter == expected} time={ms:F2} ms");

        return ms;
    }

    static void Main()
    {
        // Warm-up runs: discard their timings.
        RunVersion("warm-up lock", LockWorker);
        RunVersion("warm-up Interlocked", InterlockedWorker);

        double lockTotal = 0;
        double interlockedTotal = 0;

        Console.WriteLine("\n--- Benchmark runs ---");

        for (int run = 1; run <= 5; run++)
        {
            Console.WriteLine($"\nRun {run}");

            lockTotal += RunVersion("lock", LockWorker);
            interlockedTotal += RunVersion(
                "Interlocked.Increment", InterlockedWorker);
        }

        double averageLock = lockTotal / 5;
        double averageInterlocked = interlockedTotal / 5;

        Console.WriteLine("\n--- Summary ---");
        Console.WriteLine($"Average lock time: {averageLock:F2} ms");
        Console.WriteLine(
            $"Average Interlocked time: {averageInterlocked:F2} ms");

        if (averageInterlocked > 0)
        {
            Console.WriteLine(
                $"Ratio (lock / Interlocked): " +
                $"{averageLock / averageInterlocked:F2}");
        }

        Console.WriteLine(
            $"Threads: {NumThreads}, increments per thread: " +
            $"{IncrementsPerThread}");
    }
}
