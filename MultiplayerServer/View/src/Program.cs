using System;

namespace ServerView.src
{
    internal class Program
    {
        static void Main(string[] args)
        {
            View view = new View();

            view.Run(args);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
