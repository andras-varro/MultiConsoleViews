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
                RequestedTop = 0,
                RequestedLeft = 0,
                RequestedWidth = 1000,
                RequestedHeight = 1000,
                Title = "Short Title",
                BorderSize = 2,
                Padding = 1
            };
            window.SetBorder(new ThickLineBorder());
            window.ForegroundColor = ConsoleColor.Black;
            window.BackgroundColor = ConsoleColor.Blue;
            window.ActiveBorderForegroundColor = ConsoleColor.Black;
            window.ActiveBorderBackgroundColor = ConsoleColor.Blue;
            window.InactiveBorderBackgroundColor = ConsoleColor.Blue;
            window.PaddingBackgroundColor = ConsoleColor.DarkGreen;
            ConsoleView child = new ConsoleView { RequestedTop = 4, RequestedLeft = 4, RequestedWidth = 20, RequestedHeight = 10, Title = "Very long title for this little window!" };
            window.AddChildWindow(child);
            child = new ConsoleView { RequestedTop = 15, RequestedLeft = 10, RequestedWidth = 16, RequestedHeight = 8, Title = "Title" };
            window.AddChildWindow(child);
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
            RootWindow.DrawWindow();
            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                RootWindow.OnKeyAvailableRootWindow(cki);
            }
        }
    }
}
