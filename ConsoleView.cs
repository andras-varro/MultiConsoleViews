using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiConsoleViews
{
    public class ConsoleView
    {        
        public char LeftBorder { get; set; } = '║';
        public char RightBorder { get; set; } = '║';
        public char TopBorder { get; set; } = '═';
        public char BottomBorder { get; set; } = '═';
        public char TopLeftCorner { get; set; } = '╔';
        public char TopRightCorner { get; set; } = '╗';
        public char BottomLeftCorner { get; set; } = '╚';
        public char BottomRightCorner { get; set; } = '╝';
        public ConsoleColor ActiveBorderColor = ConsoleColor.Yellow;
        public ConsoleColor InactiveBorderColor = ConsoleColor.Gray;
        public ConsoleColor ForegroundColor = ConsoleColor.Gray;
        public ConsoleColor BackgroundColor = ConsoleColor.Black;
        public string Title { get; set; }
        public int BorderSize { get; set; } = 1;
        public int Padding { get; set; } = 0;
        public int ClientAreaFrameWidth { get => BorderSize + Padding; }

        public int MaxWidth
        {
            get
            {
                if (Parent is null) return Console.WindowWidth - Left;

                return Parent.Width - Left;
            }
        }
        public int RequestedWidth { get; set; }
        public int Width { get => Math.Min(RequestedWidth, MaxWidth); }
        public int ClientAreaWidth { get => Width - 2 * ClientAreaFrameWidth; }

        public int MaxHeight
        {
            get
            {
                if (Parent is null) return Console.WindowHeight - Top;

                return Parent.Height - Top;
            }
        }
        public int RequestedHeight { get; set; }
        public int Height { get => Math.Min(RequestedHeight, MaxHeight); }
        public int ClientAreaHeight { get => Height - 2 * ClientAreaFrameWidth; }

        public int RequestedTop { get; set; }
        public int Top { get => Math.Max(RequestedTop, 0); }
        public int TopOnScreen 
        {
            get
            {
                if (Parent is null) return Top;

                return Parent.ClientAreaTopOnScreen + Top;
            }
        }        
        public int ClientAreaTop { get => Top + ClientAreaFrameWidth; }
        public int ClientAreaTopOnScreen { get => TopOnScreen + ClientAreaFrameWidth; }
       
        public int RequestedLeft { get; set; }
        public int Left { get => Math.Max(RequestedLeft, 0); }
        public int LeftOnScreen 
        {
            get
            {
                if (Parent is null) return Left;

                return Parent.ClientAreaLeftOnScreen + Left;
            }
        }
        public int ClientAreaLeft { get => Left + ClientAreaFrameWidth; }
        public int ClientAreaLeftOnScreen { get => LeftOnScreen + ClientAreaFrameWidth; }

        public int Right { get => Left + Width - 1; }
        public int RightOnScreen { get => LeftOnScreen + Width - 1; }
        public int ClientAreaRight { get => ClientAreaLeft + ClientAreaWidth - 1; }
        public int ClientAreaRightOnScreen { get => ClientAreaLeftOnScreen + ClientAreaWidth - 1; }

        public int Bottom { get => Top + Height - 1; }
        public int BottomOnScreen { get => TopOnScreen + Height - 1; }
        public int ClientAreaBottom { get => ClientAreaTop + ClientAreaHeight - 1; }
        public int ClientAreaBottomOnScreen { get => ClientAreaTopOnScreen + ClientAreaHeight - 1; }

        public ConsoleView Parent { get; private set; }
        public IEnumerable<ConsoleView> Children { get => children.AsEnumerable(); }
        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                DrawBorder();
            }
        }
        
        private ConsoleView activeChildWindow;
        private List<ConsoleView> children = new List<ConsoleView>();
        private int cursorLeftOnScreen;
        private int CursorLeftOnScreen
        {
            get => cursorLeftOnScreen;
            set
            {
                if (value < ClientAreaLeftOnScreen)
                {
                    if (scrollPosition == 0 && CursorTopOnScreen <= ClientAreaTopOnScreen)
                    {
                        cursorLeftOnScreen = ClientAreaLeftOnScreen;
                        Console.SetCursorPosition(CursorLeftOnScreen, CursorTopOnScreen);
                        return;
                    }

                    cursorLeftOnScreen = ClientAreaRightOnScreen - 1;
                    CursorTopOnScreen--;
                    return;
                }

                if (value > ClientAreaRightOnScreen)
                {
                    cursorLeftOnScreen = ClientAreaLeftOnScreen;
                    CursorTopOnScreen++;
                    return;
                }

                cursorLeftOnScreen = value;
                Console.SetCursorPosition(CursorLeftOnScreen, CursorTopOnScreen);
            }
        }
        private int cursorTopOnScreen;
        private int CursorTopOnScreen
        {
            get => cursorTopOnScreen;
            set
            {
                if (value < ClientAreaTopOnScreen)
                {
                    if (scrollPosition == 0)
                    {
                        cursorTopOnScreen = ClientAreaTopOnScreen;
                        Console.SetCursorPosition(CursorLeftOnScreen, CursorTopOnScreen);
                        return;
                    }

                    OnScrollDown();
                    return;
                }

                cursorTopOnScreen = value;
                if (CursorTopOnScreen > ClientAreaBottomOnScreen)
                {
                    OnScrollUp();
                    return;
                }


                Console.SetCursorPosition(CursorLeftOnScreen, CursorTopOnScreen);
            }
        }
        private bool autoScroll = true;
        private int scrollPosition;
        private bool isActive;
        private List<List<char>> Buffer = new List<List<char>>();

        // private (int row, int pos) GetCursorBufferPosition()
        // {
        //     //get => (0, cursorLeftOnScreen - ClientAreaLeftOnScreen + (CursorTopOnScreen - ClientAreaTopOnScreen) * ClientAreaWidth + scrollPosition * ClientAreaWidth - 1 ); 
        //     int linesInWindowLinesCount = 0, row = -1, pos, lastLineInWindowLinesCount = 0;
        //     while (linesInWindowLinesCount < scrollPosition + (CursorTopOnScreen - ClientAreaTopOnScreen) + 1 && row + 1 < Buffer.Count)
        //     {
        //         lastLineInWindowLinesCount = BufferLineToScreenLinesCount(++row);
        //         linesInWindowLinesCount += lastLineInWindowLinesCount;
        //     }

        //     pos = (lastLineInWindowLinesCount - (linesInWindowLinesCount - (scrollPosition + (CursorTopOnScreen - ClientAreaTopOnScreen)))) * ClientAreaWidth + cursorLeftOnScreen - ClientAreaLeftOnScreen;
        //     return (Math.Max(0, row), Math.Max(0, pos));
        // }
        public ConsoleView()
        {
            isActive = true;
        }

        public void SetBorder(BorderSet borderSet)
        {
            if (borderSet is null) throw new ArgumentNullException(nameof(borderSet));

            LeftBorder = borderSet.LeftBorder;
            RightBorder = borderSet.RightBorder;
            TopBorder = borderSet.TopBorder;
            BottomBorder = borderSet.BottomBorder;
            TopLeftCorner = borderSet.TopLeftCorner;
            TopRightCorner = borderSet.TopRightCorner;
            BottomLeftCorner = borderSet.BottomLeftCorner;
            BottomRightCorner = borderSet.BottomRightCorner;
        }

        public void FillClientArea(char c)
        {
            string fillString = new string(c, ClientAreaWidth);
            for (int i = ClientAreaTopOnScreen; i <= ClientAreaBottomOnScreen; i++)
            {
                Console.SetCursorPosition(ClientAreaLeftOnScreen, i);
                ConsoleColor originalForeColor = Console.ForegroundColor;
                ConsoleColor originalBackColor = Console.BackgroundColor;
                Console.ForegroundColor = ForegroundColor;
                Console.BackgroundColor = BackgroundColor;
                Console.Write(fillString);
                Console.ForegroundColor = originalForeColor;
                Console.BackgroundColor = originalBackColor;
            }
        }

        public void ClearClientArea()
        {
            FillClientArea(' ');            
            CursorTopOnScreen = ClientAreaTopOnScreen;
            CursorLeftOnScreen = ClientAreaLeftOnScreen;
        }
        
        public bool ActivateWindow(ConsoleView window)
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

        // public void DrawClientAreaBorder()
        // {
        //     Console.SetCursorPosition(ClientAreaLeftOnScreen, ClientAreaTopOnScreen);
        //     Console.Write(TopLeftCorner);
        //     Console.SetCursorPosition(ClientAreaRightOnScreen, ClientAreaTopOnScreen);
        //     Console.Write(TopRightCorner);
        //     Console.SetCursorPosition(ClientAreaLeftOnScreen, ClientAreaBottomOnScreen);
        //     Console.Write(BottomLeftCorner);
        //     Console.SetCursorPosition(ClientAreaRightOnScreen, ClientAreaBottomOnScreen);
        //     Console.Write(BottomRightCorner);
        // }

        public bool AddChildWindow(ConsoleView window)
        {
            if (children.Contains(window)) return false;

            window.Parent = this;
            children.Add(window);
            IsActive = false;
            ActivateWindow(window);
            return true;
        }

        public virtual void OnScrollDown()
        {
            if (scrollPosition == 0) return;
            scrollPosition--;
            WriteBufferToWindowOnScroll();
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

        public virtual void OnScrollUp()
        {
            if (!autoScroll) return;
            if (scrollPosition >= BufferLinesToScreenLinesTotalCount()) return;

            scrollPosition++;
            WriteBufferToWindowOnScroll();
        }

        public void Write(char c)
        {            
            List<char> currentLine = Buffer.LastOrDefault();

            if (currentLine is null) 
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

        internal void DrawBorder()
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            ConsoleColor originalBackColor = Console.BackgroundColor;
            if (IsActive) Console.ForegroundColor = ActiveBorderColor;
            else Console.ForegroundColor = InactiveBorderColor;
            Console.BackgroundColor = BackgroundColor;

            for (int j = 0; j < BorderSize; j++)
            {
                int localTop = Top + j, localLeft = Left + j, localBottom = Top + Height - 1 - j, localRight = Left + Width - 1 - j, localBorderWidth = Width - 2 - 2 * j, localBorderHeight = Height - 2 - 2 * j;
                Console.SetCursorPosition(localLeft, localTop);
                Console.Write($"{TopLeftCorner}{new string(TopBorder, localBorderWidth)}{TopRightCorner}");
                DrawTitle();
                for (int i = 1; i <= localBorderHeight; i++)
                {
                    Console.SetCursorPosition(localLeft, localTop + i);
                    Console.Write(RightBorder);
                    Console.SetCursorPosition(localRight, localTop + i);
                    Console.Write(LeftBorder);
                }

                Console.SetCursorPosition(localLeft, localBottom);
                Console.Write($"{BottomLeftCorner}{new string(BottomBorder, localBorderWidth)}{BottomRightCorner}");
            }
            
            Console.ForegroundColor = originalColor;
            Console.BackgroundColor = originalBackColor;
            ClearClientArea();
            //DrawClientAreaBorder();
            foreach (var child in Children) child.DrawBorder();
        }

        internal void OnScrollUpInternal()
        {
            if (activeChildWindow is null) OnScrollUp();
            else activeChildWindow.OnScrollUpInternal();
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

        private void WriteBufferToWindowOnScroll()
        {
            bool originalAutoScrollSetting = autoScroll;
            autoScroll = false;
            WriteBufferToWindow();
            autoScroll = originalAutoScrollSetting;
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
            ConsoleColor originalForeColor = Console.ForegroundColor;
            ConsoleColor originalBackColor = Console.BackgroundColor;
            Console.ForegroundColor = ForegroundColor;
            Console.BackgroundColor = BackgroundColor;
            Console.SetCursorPosition(ClientAreaLeftOnScreen, CursorTopOnScreen);
            Console.Write(new string(' ', ClientAreaWidth));
            Console.ForegroundColor = originalForeColor;
            Console.BackgroundColor = originalBackColor;
        }

        private void DrawTitle()
        {
            if (Title.Length > ClientAreaWidth)
            {
                Console.SetCursorPosition(ClientAreaLeftOnScreen, Top);
                Console.Write($"{Title.Substring(0, ClientAreaWidth / 2 - 1)}..{Title.Substring(Title.Length - (int)Math.Ceiling(ClientAreaWidth / 2.0) + 1)}");
            }
            else
            {
                Console.SetCursorPosition(ClientAreaLeftOnScreen + (ClientAreaWidth - Title.Length) / 2, Top);
                Console.Write(Title);
            }
        }

        private void WriteClientArea(char c)
        {
            if (CursorTopOnScreen < ClientAreaTopOnScreen) CursorTopOnScreen = ClientAreaTopOnScreen;
            if (CursorLeftOnScreen < ClientAreaLeftOnScreen) CursorLeftOnScreen = ClientAreaLeftOnScreen;
            if (CursorTopOnScreen > ClientAreaBottomOnScreen) return;
            if (c == '\r')
            {
                CursorLeftOnScreen = ClientAreaLeftOnScreen;
                CursorTopOnScreen++;
                return;
            }

            ConsoleColor originalForeColor = Console.ForegroundColor;
            ConsoleColor originalBackColor = Console.BackgroundColor;
            Console.ForegroundColor = ForegroundColor;
            Console.BackgroundColor = BackgroundColor;
            Console.Write(c);
            MoveCursorToRight();
            Console.ForegroundColor = originalForeColor;
            Console.BackgroundColor = originalBackColor;
        }

        private void MoveCursorToRight()
        {
            CursorLeftOnScreen++;
        }

        private void MoveCursorToLeft()
        {
            CursorLeftOnScreen--;
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
}
