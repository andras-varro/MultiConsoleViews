namespace MultiConsoleViews
{
    public class BorderSet
    {
        public char LeftBorder { get; set; }
        public char RightBorder { get; set; }
        public char TopBorder { get; set; }
        public char BottomBorder { get; set; }
        public char TopLeftCorner { get; set; }
        public char TopRightCorner { get; set; }
        public char BottomLeftCorner { get; set; }
        public char BottomRightCorner { get; set; }
    }

    public class DualLineBorder:BorderSet
    {
        public DualLineBorder()
        {
            LeftBorder = '║';
            RightBorder = '║';
            TopBorder = '═';
            BottomBorder = '═';
            TopLeftCorner = '╔';
            TopRightCorner = '╗';
            BottomLeftCorner = '╚';
            BottomRightCorner = '╝';
        }
    }    

    public class ThickLineBorder:BorderSet
    {
        public ThickLineBorder()
        {
            TopLeftCorner = '▄';
            TopRightCorner = '▄';
            TopBorder = '▄';
            LeftBorder = '▌';
            RightBorder = '▐';
            BottomLeftCorner = '▀';
            BottomRightCorner = '▀';
            BottomBorder = '▀';
        }
    }

    public class MonoBlockBorder:BorderSet
    {
        public MonoBlockBorder()
        {
            TopLeftCorner = '█';
            TopRightCorner = '█';
            TopBorder = '█';
            LeftBorder = '█';
            RightBorder = '█';
            BottomLeftCorner = '█';
            BottomRightCorner = '█';
            BottomBorder = '█';
        }
    }

    public class SingleLineBorder:BorderSet
    {
        public SingleLineBorder()
        {
            LeftBorder = '│';
            RightBorder = '│';
            TopBorder = '─';
            BottomBorder = '─';
            TopLeftCorner = '┌';
            TopRightCorner = '┐';
            BottomLeftCorner = '└';
            BottomRightCorner = '┘';
        }
    }    

    public class MixedSingleAndDoubleLineBorder:BorderSet
    {
        public MixedSingleAndDoubleLineBorder()
        {
            LeftBorder = '│';
            RightBorder = '│';
            TopBorder = '═';
            BottomBorder = '═';
            TopLeftCorner = '╒';
            TopRightCorner = '╕';
            BottomLeftCorner = '╘';
            BottomRightCorner = '╛';
        }
    }
    public class InverseMixedSingleAndDoubleLineBorder:BorderSet
    {
        public InverseMixedSingleAndDoubleLineBorder()
        {
            LeftBorder = '║';
            RightBorder = '║';
            TopBorder = '─';
            BottomBorder = '─';
            TopLeftCorner = '╓';
            TopRightCorner = '╖';
            BottomLeftCorner = '╙';
            BottomRightCorner = '╛';
        }
    }
    public class GrayBorder:BorderSet
    {
        public GrayBorder()
        {
            LeftBorder = '░';
            RightBorder = '░';
            TopBorder = '░';
            BottomBorder = '░';
            TopLeftCorner = '░';
            TopRightCorner = '░';
            BottomLeftCorner = '░';
            BottomRightCorner = '░';
        }
    }
}