using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace MultiConsoleViews
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleView window = new ConsoleView
            {
                RequestedTop = 2,
                RequestedLeft = 2,
                RequestedWidth = 70,
                RequestedHeight = 25,
                Title = "Short Title",
                BorderSize = 4,
                Padding = 4
            };
            window.SetBorder(new DualLineBorder());
            window.ForegroundColor = ConsoleColor.Black;
            window.BackgroundColor = ConsoleColor.Blue;
            window.ActiveBorderColor = ConsoleColor.Black;
            // Window child = new Window { RequestedTop = 4, RequestedLeft = 4, RequestedWidth = 20, RequestedHeight = 10, Title = "Very long title for this little window!" };
            // window.AddChildWindow(child);
            // child = new Window { RequestedTop = 15, RequestedLeft = 10, RequestedWidth = 16, RequestedHeight = 8, Title = "Title" };
            // window.AddChildWindow(child);
            // child = new Window { RequestedTop = 5, RequestedLeft = 25, RequestedWidth = 16, RequestedHeight = 4, Title = "Small" };
            // window.AddChildWindow(child);
            new UpdateQueue().Run(window);
        }
    }

    class UpdateQueue
    {
        ConsoleView RootWindow { get; set; }
        public void Run(ConsoleView rootWindow)
        {
            if (rootWindow is null) throw new ArgumentNullException(nameof(rootWindow));

            RootWindow = rootWindow;
            Console.Clear();
            Console.CursorVisible = false;
            RootWindow.DrawBorder();
            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                RootWindow.OnKeyAvailableRootWindow(cki);
            }
        }
    }
}
