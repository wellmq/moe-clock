public class Program
{
    static async Task<int> Main(string[] args)
    {
        if (!ConsoleSupported())
        {
            Console.WriteLine("your console emulator not supported");
            return 1;
        }
        Console.TreatControlCAsInput = true;
        bool parsed = TryParse(args, out LaunchParameters lParams, out string errorMessage);
        if (parsed)
        {
            AssetsLoader aLoader = new AssetsLoader();
            if (lParams.ListAllThemes)
            {
                string[] themes = aLoader.GetListOfThemes();
                foreach (string theme in themes)
                {
                    Console.WriteLine(theme);
                }
                return 0;
            }
            if (lParams.ShowPreview)
            {
                string[] base64Bytes = new string[10];
                try
                {
                    base64Bytes = aLoader.GetBytesOfTheme(lParams.Theme);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return 1;
                }
                bool big = lParams.Theme.Contains("Big");
                ClockRenderer cRenderer = new ClockRenderer(lParams.Center, big, lParams.Seconds);
                OpenAlternateScreenBuffer();
                cRenderer.RenderPreview(base64Bytes);
                AwaitCtrlC();
                CloseAlternateScreenBuffer();
                return 0;
            }
            if (lParams.ShowHelp)
            {
                Console.WriteLine("""
                    moe-clock is a terminal clock with anime number sprites
                    
                    Usage:
                      moe-clock [options]
                    Options:
                      -l, --list-all-themes     List all available themes
                      -p, --preview             Show a preview of all digits for selected theme
                      -t, --theme <title>       Theme to display (default: "gelbooru")
                      -s, --seconds             Display seconds
                      -c, --center              Center the clock in the terminal window
                    """);
                return 0;
            }
            else //Standart render
            {
                string[] base64Bytes = new string[10];
                try
                {
                    base64Bytes = aLoader.GetBytesOfTheme(lParams.Theme);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return 1;
                }
                bool big = lParams.Theme.Contains("Big");
                OpenAlternateScreenBuffer();
                ClockRenderer cRenderer = new ClockRenderer(lParams.Center, big, lParams.Seconds);
                CancellationTokenSource cSource = new CancellationTokenSource();
                _ = cRenderer.RenderClock(base64Bytes, cSource.Token);
                AwaitCtrlC();
                cSource.Cancel();
                CloseAlternateScreenBuffer();
                return 0;
            }
        }
        Console.WriteLine(errorMessage);
        return 1;

    }
    static void OpenAlternateScreenBuffer()
    {
        Console.CursorVisible = false;
        Console.Write("\e[?1049h\e[2J");
    }
    static void CloseAlternateScreenBuffer()
    {
        Console.CursorVisible = true;
        Console.Write("\e[2J\e[?1049l");
    }
    static void AwaitCtrlC()
    {
        bool awaiting = true;
        while (awaiting)
        {
            ConsoleKeyInfo cKeyInfo = Console.ReadKey();
            if (cKeyInfo.Key == ConsoleKey.C && cKeyInfo.Modifiers == ConsoleModifiers.Control) awaiting = false;
        }
    }
    static bool ConsoleSupported()
    {
        SupportChecker sChecker = new SupportChecker();
        return sChecker.HasKittyGraphicsProtocolSupport();
    }
    static bool TryParse(string[] args, out LaunchParameters lParams, out string errorMessage)
    {
        lParams = new LaunchParameters();
        errorMessage = "";
        if (args.Length < 1) return true;
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-l":
                case "--list-all-themes":
                    lParams.ShowPreview = false;
                    lParams.ListAllThemes = true;
                    break;
                case "-p":
                case "--preview":
                    lParams.ListAllThemes = false;
                    lParams.ShowPreview = true;
                    break;
                case "-h":
                case "--help":
                    lParams.ListAllThemes = false;
                    lParams.ShowPreview = false;
                    lParams.ShowHelp = true;
                    break;
                case "-t":
                case "--theme":
                    if (args.Length > i + 1)
                    {
                        lParams.Theme = args[i + 1];
                        i++;
                    }
                    else
                    {
                        errorMessage = "theme not specified";
                    }
                    break;
                case "-s":
                case "--seconds":
                    lParams.Seconds = true;
                    break;
                case "-c":
                case "--center":
                    lParams.Center = true;
                    break;
            }
        }
        if (!string.IsNullOrWhiteSpace(errorMessage)) return false;
        return true;
    }
}
