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
            Window window = new Window { RequestedTop = 2, RequestedLeft = 2, RequestedWidth = 70, RequestedHeight = 25, Title = "Short Title" };
            Window child = new Window { RequestedTop = 4, RequestedLeft = 4, RequestedWidth = 20, RequestedHeight = 10, Title = "Very long title for this little window!" };
            window.AddChildWindow(child);
            child = new Window { RequestedTop = 15, RequestedLeft = 10, RequestedWidth = 16, RequestedHeight = 8, Title = "Title" };
            window.AddChildWindow(child);
            child = new Window { RequestedTop = 5, RequestedLeft = 25, RequestedWidth = 16, RequestedHeight = 4, Title = "Small" };
            window.AddChildWindow(child);
            new UpdateQueue().Run(window);
        }
    }

    class Window
    {
        private List<List<char>> Buffer = new List<List<char>>();
        public char LeftBorder { get; set; } = '║';
        public char RightBorder { get; set; } = '║';
        public char TopBorder { get; set; } = '═';
        public char BottomBorder { get; set; } = '═';
        public char TopLeftCorner { get; set; } = '╔';
        public char TopRightCorner { get; set; } = '╗';
        public char BottomLeftCorner { get; set; } = '╚';
        public char BottomRightCorner { get; set; } = '╝';
        public string Title { get; set; }
        public int BorderSize { get; set; } = 1;
        public int Padding { get; set; } = 0;
        public int RequestedWidth { get; set; }
        public int Width { get => Math.Min(RequestedWidth, Console.WindowWidth - Left); }
        public int ClientAreaWidth { get => Width - 2 * BorderSize - 2 * Padding; }
        public int RequestedHeight { get; set; }
        public int Height { get => Math.Min(RequestedHeight, Console.WindowHeight - Top); }
        public int ClientAreaHeight { get => Height - 2 * BorderSize - 2 * Padding; }
        public int RequestedTop { get; set; }
        public int Top { get => RequestedTop; }
        public int ClientAreaTop { get => Top + BorderSize + Padding; }
        public int RequestedLeft { get; set; }
        public int Left { get => RequestedLeft; }
        public int ClientAreaLeft { get => Left + BorderSize + Padding; }
        public int ClientAreaRight { get => ClientAreaLeft + ClientAreaWidth - 1; }
        public int ClientAreaBottom { get => ClientAreaTop + ClientAreaHeight - 1; }
        public Window Parent { get; private set; }
        public IEnumerable<Window> Children { get => children.AsEnumerable(); }
        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                DrawBorder();
            }
        }
        private Window activeChildWindow;
        private List<Window> children = new List<Window>();
        private int cursorLeft;
        private int CursorLeft
        {
            get => cursorLeft;
            set
            {
                if (value < ClientAreaLeft)
                {
                    if (scrollPosition == 0 && CursorTop <= ClientAreaTop)
                    {
                        cursorLeft = ClientAreaLeft;
                        Console.SetCursorPosition(cursorLeft, cursorTop);
                        return;
                    }

                    cursorLeft = ClientAreaRight - 1;
                    CursorTop--;
                    return;
                }

                if (value > ClientAreaRight)
                {
                    cursorLeft = ClientAreaLeft;
                    CursorTop++;
                    return;
                }

                cursorLeft = value;
                Console.SetCursorPosition(cursorLeft, cursorTop);
            }
        }
        private int cursorTop;
        private int CursorTop
        {
            get => cursorTop;
            set
            {
                if (value < ClientAreaTop)
                {
                    if (scrollPosition == 0)
                    {
                        cursorTop = ClientAreaTop;
                        Console.SetCursorPosition(cursorLeft, cursorTop);
                        return;
                    }

                    OnScrollDown();
                    return;
                }

                cursorTop = value;
                if (cursorTop > ClientAreaBottom)
                {
                    OnScrollUp();
                    return;
                }


                Console.SetCursorPosition(cursorLeft, cursorTop);
            }
        }
        private bool autoScroll = true;
        private int scrollPosition;
        private bool isActive;
        private (int row, int pos) GetCursorBufferPosition()
        {
            //get => (0, cursorLeft - ClientAreaLeft + (cursorTop - ClientAreaTop) * ClientAreaWidth + scrollPosition * ClientAreaWidth - 1 ); 
            int linesInWindowLinesCount = 0, row = -1, pos, lastLineInWindowLinesCount = 0;
            while (linesInWindowLinesCount < scrollPosition + (CursorTop - ClientAreaTop) + 1 && row + 1 < Buffer.Count)
            {
                lastLineInWindowLinesCount = BufferLineToScreenLinesCount(++row);
                linesInWindowLinesCount += lastLineInWindowLinesCount;
            }

            pos = (lastLineInWindowLinesCount - (linesInWindowLinesCount - (scrollPosition + (CursorTop - ClientAreaTop)))) * ClientAreaWidth + cursorLeft - ClientAreaLeft;
            return (Math.Max(0, row), Math.Max(0, pos));
        }

        public void FillClientArea(char c)
        {
            string fillString = new string(c, ClientAreaWidth);
            for (int i = ClientAreaTop; i <= ClientAreaBottom; i++)
            {
                Console.SetCursorPosition(ClientAreaLeft, i);
                Console.Write(fillString);
            }
        }

        public void ClearClientArea()
        {
            FillClientArea(' ');            
            CursorTop = ClientAreaTop;
            CursorLeft = ClientAreaLeft;
        }

        private int BufferLineToScreenLinesCount(int lineNumberInBuffer)
        {
            if (lineNumberInBuffer < 0 || lineNumberInBuffer >= Buffer.Count) throw new InvalidOperationException($"{lineNumberInBuffer} is an invalid indexer for Buffer. Buffer has {Buffer.Count} elements.");
            return LineToScreenLineCount(Buffer[lineNumberInBuffer]);
        }

        private int LineToScreenLineCount(List<char> line)
        {
            return (int)Math.Ceiling(line.Count / (double)ClientAreaWidth);
        }

        private int BufferLinesToScreenLinesTotalCount()
        {
            int sum = 0;
            for (int i = 0; i < Buffer.Count;i++) sum += BufferLineToScreenLinesCount(i);
            return sum;
        }

        private List<string> BufferToScreenLines(int startLine = 0, int length = 0)
        {
            List<string> retVal = new List<string>();
            if (length == 0) length = ClientAreaHeight;

            int screenLinesUpToNow = -1;
            foreach (List<char> row in Buffer)
            {
                int screenLinesInThisLine = LineToScreenLineCount(row);
                if (screenLinesUpToNow + screenLinesInThisLine < startLine)
                {
                    screenLinesUpToNow += screenLinesInThisLine;
                    continue;
                }

                int startChar = 0;
                while (startChar + ClientAreaWidth < row.Count && screenLinesUpToNow < startLine+length)
                {
                    screenLinesUpToNow++;
                    if (screenLinesUpToNow < startLine)
                    {
                        startChar += ClientAreaWidth;
                        continue;
                    }

                    retVal.Add(new string(row.GetRange(startChar, ClientAreaWidth).ToArray()));
                    startChar += ClientAreaWidth;
                }

                if (startChar < row.Count && startLine <= screenLinesUpToNow + 1 && screenLinesUpToNow < startLine+length)
                {
                    retVal.Add(new string(row.GetRange(startChar, row.Count - startChar).ToArray()));
                }
            }

            return retVal;
        }

        public bool ActivateWindow(Window window)
        {
            if (window is null) return false;
            if (activeChildWindow == window) return true;

            if (Children.Contains(window))
            {
                if (activeChildWindow is object) activeChildWindow.IsActive = false;
                activeChildWindow = window;
                window.IsActive = true;
                return true;
            }

            return false;
        }

        public void DrawClientAreaBorder()
        {
            Console.SetCursorPosition(ClientAreaLeft, ClientAreaTop);
            Console.Write(TopLeftCorner);
            Console.SetCursorPosition(ClientAreaRight, ClientAreaTop);
            Console.Write(TopRightCorner);
            Console.SetCursorPosition(ClientAreaLeft, ClientAreaBottom);
            Console.Write(BottomLeftCorner);
            Console.SetCursorPosition(ClientAreaRight, ClientAreaBottom);
            Console.Write(BottomRightCorner);
        }

        public bool AddChildWindow(Window window)
        {
            if (children.Contains(window)) return false;

            window.Parent = this;
            children.Add(window);
            ActivateWindow(window);
            return true;
        }

        internal void OnKeyAvailableRootWindow(ConsoleKeyInfo consoleKeyInfo)
        {
            if (consoleKeyInfo.Modifiers == ConsoleModifiers.Control &&
                    consoleKeyInfo.Key == ConsoleKey.N &&
                    children.Count > 1)
            {
                if (activeChildWindow is null) ActivateWindow(children[0]);

                int activeChildWindowIndex = children.IndexOf(activeChildWindow);
                if (activeChildWindowIndex == children.Count - 1) ActivateWindow(children[0]);
                else ActivateWindow(children[++activeChildWindowIndex]);
                return;
            }

            if (consoleKeyInfo.Key == ConsoleKey.UpArrow)
            {
                OnScrollUpInternal();
                return;
            }

            if (consoleKeyInfo.Key == ConsoleKey.DownArrow)
            {
                OnScrollDownInternal();
                return;
            }

            OnKeyAvailableInternal(consoleKeyInfo);
        }

        internal void OnScrollDownInternal()
        {
            if (activeChildWindow is null) OnScrollDown();
            else activeChildWindow.OnScrollDownInternal();
        }

        public virtual void OnScrollDown()
        {
            if (scrollPosition == 0) return;
            scrollPosition--;
            WriteBufferToWindowOnScroll();
        }

        private void WriteBufferToWindowOnScroll()
        {
            bool originalAutoScrollSetting = autoScroll;
            autoScroll = false;
            WriteBufferToWindow();
            autoScroll = originalAutoScrollSetting;
        }

        internal void OnScrollUpInternal()
        {
            if (activeChildWindow is null) OnScrollUp();
            else activeChildWindow.OnScrollUpInternal();
        }

        public virtual void OnScrollUp()
        {
            if (!autoScroll) return;
            if (scrollPosition >= BufferLinesToScreenLinesTotalCount()) return;

            scrollPosition++;
            WriteBufferToWindowOnScroll();
        }

        private void WriteBufferToWindow()
        {            
            ClearClientArea();
            List<string> screenLines = BufferToScreenLines(scrollPosition);
            foreach (var line in screenLines)
            {
                foreach (char c in line) WriteClientArea(c);
            }
        }

        private void EmptyCurrentLine()
        {
            Console.SetCursorPosition(ClientAreaLeft, cursorTop);
            Console.Write(new string(' ', ClientAreaWidth));
        }

        internal void OnKeyAvailableInternal(ConsoleKeyInfo consoleKeyInfo)
        {
            if (activeChildWindow is null) OnKeyAvailable(consoleKeyInfo);
            else activeChildWindow.OnKeyAvailableInternal(consoleKeyInfo);
        }

        public virtual void OnKeyAvailable(ConsoleKeyInfo consoleKeyInfo)
        {
            // Ignore not printable chars.
            if (consoleKeyInfo.KeyChar == '\u0000') return;

            if (consoleKeyInfo.Key == ConsoleKey.Backspace)
            {
                Backspace();
                return;
            }

            Write(consoleKeyInfo.KeyChar);
        }

        internal void DrawBorder()
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            if (IsActive) Console.ForegroundColor = ConsoleColor.Yellow;
            else Console.ForegroundColor = ConsoleColor.Gray;
            Console.SetCursorPosition(Left, Top);
            Console.Write($"{TopLeftCorner}{new string(TopBorder, Width - 2)}{TopRightCorner}");
            DrawTitle();
            for (int i = 1; i < Height - 1; i++)
            {
                Console.SetCursorPosition(Left, Top + i);
                Console.Write(RightBorder);
                Console.SetCursorPosition(Left + Width - 1, Top + i);
                Console.Write(LeftBorder);
            }

            Console.SetCursorPosition(Left, Top + Height - 1);
            Console.Write($"{BottomLeftCorner}{new string(TopBorder, Width - 2)}{BottomRightCorner}");
            Console.ForegroundColor = originalColor;
            //DrawClientAreaBorder();
            foreach (var child in Children) child.DrawBorder();
        }

        private void DrawTitle()
        {
            if (Title.Length > ClientAreaWidth)
            {
                Console.SetCursorPosition(ClientAreaLeft, Top);
                Console.Write($"{Title.Substring(0, ClientAreaWidth / 2 - 1)}..{Title.Substring(Title.Length - (int)Math.Ceiling(ClientAreaWidth / 2.0) + 1)}");
            }
            else
            {
                Console.SetCursorPosition(ClientAreaLeft + (ClientAreaWidth - Title.Length) / 2, Top);
                Console.Write(Title);
            }
        }

        public void Write(char c)
        {
            var bufferPosition = GetCursorBufferPosition();
            List<char> currentLine;
            if (Buffer.Count > bufferPosition.row) currentLine = Buffer[bufferPosition.row];
            else
            {
                currentLine = new List<char>();
                Buffer.Add(currentLine);
            }

            currentLine.Add(c);
            if (c == '\r')
            {
                Buffer.Add(new List<char>());
            }

            WriteClientArea(c);
        }

        public void Write(string s)
        {
            foreach(char c in s) Write(c);
        }

        public void WriteLine(char c)
        {
            Write(c);
            Write('\n');
        }

        public void WriteLine(string s)
        {
            Write(s);
            Write('\n');
        }

        private void WriteClientArea(char c)
        {
            if (CursorTop < ClientAreaTop) CursorTop = ClientAreaTop;
            if (CursorLeft < ClientAreaLeft) CursorLeft = ClientAreaLeft;
            if (CursorTop > ClientAreaBottom) return;
            if (c == '\r')
            {
                CursorLeft = ClientAreaLeft;
                CursorTop++;
                return;
            }

            Console.Write(c);
            MoveCursorToRight();
        }

        private void MoveCursorToRight()
        {
            CursorLeft++;
        }

        private void MoveCursorToLeft()
        {
            CursorLeft--;
        }

        private void Backspace()
        {
            // if (CursorBufferPosition == -1) return;
            // // Buffer is zero based
            // Buffer.RemoveAt(CursorBufferPosition);
            // EmptyCurrentLine();
            // MoveCursorToLeft();
            // WriteBufferToWindow();
        }

        private List<List<char>> GetLines(int startLine = 0, int numberOfLines = 0)
        {

            if (startLine < 0) startLine = 0;
            if (numberOfLines <= 0) numberOfLines = Math.Min(ClientAreaHeight, Buffer.Count-startLine);

            return Buffer.GetRange(startLine, numberOfLines);
        }
    }

    class UpdateQueue
    {
        Window RootWindow { get; set; }
        public void Run(Window rootWindow)
        {
            if (rootWindow is null) throw new ArgumentNullException(nameof(rootWindow));

            RootWindow = rootWindow;
            Console.Clear();
            //Console.CursorVisible = false;
            RootWindow.DrawBorder();
            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                RootWindow.OnKeyAvailableRootWindow(cki);
            }
        }
    }
}
