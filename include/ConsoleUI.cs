using System;

namespace DiscordTools
{
    public static class ConsoleUI
    {
        public static void ShowBanner()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"                   _____________                          _________   ________           ______       ");
            Console.WriteLine(@"                   ___  __ \__(_)_______________________________  /   ___  __/______________  /_______");
            Console.WriteLine(@"                   __  / / /_  /__  ___/  ___/  __ \_  ___/  __  /    __  /  _  __ \  __ \_  /__  ___/");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@"                   _  /_/ /_  / _(__  )/ /__ / /_/ /  /   / /_/ /     _  /   / /_/ / /_/ /  / _(__  ) ");
            Console.WriteLine(@"                   /_____/ /_/  /____/ \___/ \____//_/    \__,_/      /_/    \____/\____//_/  /____/  ");
            Console.WriteLine("\n\n                  |------------------------------------------------------------------------------------|");
       
        }

        public static void ShowMenu()
        {         
            Console.WriteLine("\n\n\n                  |----------- V2.1.1 -------------------- Tools Menu ---------------------------------|   ");
            Console.WriteLine("\n\n\t\t\t                        1. Send Webhook Message");
            Console.WriteLine("\n\t\t\t                        2. Get Server Info");
            Console.WriteLine("\n\t\t\t                        3. Server Monitoring");
            Console.WriteLine("\n\t\t\t                        4. Get Members Info");
            Console.WriteLine("\n\t\t\t                        5. Auto Add Role");
            Console.WriteLine("\n\t\t\t                        0. Exit");
            Console.WriteLine("\n\n                  |--------------------------------------------------------------- @wsnrfidev ---------|");
            Console.Write("\n                  Press the number to choose your tool: ");
            Console.ResetColor();
        }
    }
}
