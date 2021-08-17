using System;

namespace ElasticSearchTest.Helper
{
    public static class ExceptionHelper
    {
        public static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Console.WriteLine($"HATA: {(args.ExceptionObject as Exception).Message}");
            Console.ReadKey();
            Environment.Exit(1);
        }
    }
}
