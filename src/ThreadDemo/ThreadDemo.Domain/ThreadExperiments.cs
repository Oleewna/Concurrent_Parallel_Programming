using System.Diagnostics.Metrics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ThreadDemo.Domain;

public class ThreadExperiments
{
    private static int counter = 0;
    private static object gate = new();

    public void RunSequential()
    {
        Work("A");
        Work("B");
    }

    public void RunThreads()
    {
        Thread t1 = new(() => Work("A"))
        {
            Name = "Worker-A",
            IsBackground = true
        };

        Thread t2 = new(() => Work("B"))
        {
            Name = "Worker-B"
        };

        Console.WriteLine("Avvio dei thread...");

        t1.Start();
        Thread.Sleep(2000);
        t2.Start();

        Console.WriteLine("Il thread principale continua.");

        t1.Join();
        t2.Join();

        Console.WriteLine("Entrambi i thread sono terminati.");
    }

    public void ShowThreadProperties()
    {
        Thread current = Thread.CurrentThread;

        Console.WriteLine($"ID: {current.ManagedThreadId}");
        Console.WriteLine($"Nome: {current.Name ?? "(non assegnato)"}");
        Console.WriteLine($"Stato: {current.ThreadState}");
        Console.WriteLine($"Vivo: {current.IsAlive}");
        Console.WriteLine($"Background: {current.IsBackground}");
        Console.WriteLine($"Priorità: {current.Priority}");
    }

    public (int Expected, int Actual) RunCounter(bool useLock, int iterations = 100_000)
    {
        counter = 0;

        Thread t1 = new Thread(() => Increment(iterations, useLock));
        Thread t2 = new Thread(() => Increment(iterations, useLock));

        t1.Start();
        t2.Start();

        t1.Join();
        t2.Join();

        return (iterations * 2, counter);
    }

    private static void Increment(int iterations, bool useLock)
    {
        for (int i = 0; i < iterations; i++)
        {
            if (useLock)
            {
                lock (gate)
                {
                    counter++;
                }
            }
            else
            {
                counter++;
            }
        }
    }

    private static void Work(string name)
    {
        for (int i = 1; i <= 5; i++)
        {
            Console.WriteLine($"[{name}] Thread {Thread.CurrentThread.ManagedThreadId}: passo {i}");

            Thread.Sleep(100);
        }
    }
}