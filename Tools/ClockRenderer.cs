using System.Runtime.InteropServices;

public class ClockRenderer
{
    //Standart hex code for *Terminal IOCtl Get Window Size*
    private ulong TIOCGWINSZ = 0x5413;
    private int imageMargin = 2;
    private bool big;
    private bool center;
    private bool withSeconds;

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowSize
    {
        public ushort Rows;
        public ushort Columns;
        public ushort XPixels;
        public ushort YPixels;
    }

    [DllImport("libc")] // Import request ioctl method from libc
    private static extern int ioctl(int fileDescriptor, ulong requestCode, ref WindowSize windowSize);

    private (int, int) GetSymbolSizeFromLibc()
    {
        WindowSize ws = new WindowSize();
        ioctl(1, TIOCGWINSZ, ref ws);
        int symbX = ws.XPixels / ws.Columns; //Symbol width in pixels
        int symbY = ws.YPixels / ws.Rows; //Symbol height in pixels
        return (symbX, symbY);
    }

    private (int, int) GetImageSizeInConsole()
    {
        (int, int) symbolSize = GetSymbolSizeFromLibc();
        int columns, rows;
        //All png assets have fixed size (136x300 or 68x150)
        if (big)
        {
            columns = 136 / symbolSize.Item1;
            rows = 300 / symbolSize.Item2;
        }
        else
        {
            columns = 68 / symbolSize.Item1;
            rows = 150 / symbolSize.Item2;
        }
        return (columns, rows);
    }

    private void SetInitialCentralCursorPosition(ref int cursorLeft, ref int cursorTop, int length)
    {
        (int, int) imageSize = GetImageSizeInConsole();
        cursorLeft = (Console.BufferWidth / 2) - ((imageSize.Item1 * length + imageMargin * length) / 2);
        cursorTop = (Console.BufferHeight / 2) - (imageSize.Item2 / 2);
        if (cursorLeft < 0) cursorLeft = 0;
        if (cursorTop < 0) cursorTop = 0;
    }

    private void CleanScreen()
    {
        Console.Write("\e[2J");
    }

    public ClockRenderer(bool center, bool big, bool withSeconds)
    {
        this.center = center;
        this.big = big;
        this.withSeconds = withSeconds;
    }

    public void RenderPreview(string[] base64Bytes)
    {
        CleanScreen();
        (int, int) imageSize = GetImageSizeInConsole();
        int cursorLeft = Console.CursorLeft; int cursorTop = Console.CursorTop;
        //if (center) SetInitialCentralCursorPosition(ref cursorLeft, ref cursorTop, 10);
        for (int i = 0; i < base64Bytes.Length; i++)
        {
            Console.SetCursorPosition(cursorLeft, cursorTop);
            Console.Write($"\e_Ga=T,f=100,m=0;{base64Bytes[i]}\e\\");
            cursorLeft += (imageSize.Item1 + imageMargin);
        }
    }
    public async Task RenderClock(string[] base64Bytes, CancellationToken cToken)
    {
        DateTimeOffset lastRenderTime = DateTimeOffset.MinValue;
        (int, int) imageSize = GetImageSizeInConsole();
        while (!cToken.IsCancellationRequested)
        {
            if (DateTimeOffset.Now.ToUnixTimeSeconds() - lastRenderTime.ToUnixTimeSeconds() >= 1)
            {
                CleanScreen();
                int cursorLeft = 0; int cursorTop = 0;
                lastRenderTime = DateTimeOffset.Now;
                string timeToRender = $"{lastRenderTime.Hour:00}{lastRenderTime.Minute:00}";
                if (withSeconds) timeToRender += $"{lastRenderTime.Second:00}";
                if (center)
                    SetInitialCentralCursorPosition(ref cursorLeft, ref cursorTop, timeToRender.Length);
                for (int i = 0; i < timeToRender.Length; i++)
                {
                    Console.SetCursorPosition(cursorLeft, cursorTop);
                    int numberToRender = Convert.ToInt32(timeToRender[i].ToString());
                    Console.Write($"\e_Ga=T,f=100,m=0;{base64Bytes[numberToRender]}\e\\");
                    cursorLeft += (imageSize.Item1 + imageMargin);
                }
                await Task.Delay(100, cToken);
            }
        }
    }
}
