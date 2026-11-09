public class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.TreatControlCAsInput = true;
        bool parsed = TryParse(args, out LaunchParameters lParams, out string errorMessage);
        if (parsed)
        {
            if (lParams.ShowHelp)
            {
                Console.WriteLine("""
                    moe-clock is a terminal clock with anime number sprites
                    
                    Usage:
                      moe-clock [options]
                    Options:
                      -l, --list-all-themes         List all available themes
                      -p, --preview                 Show a preview of all digits for selected theme
                      -t, --theme <TITLE>           (built-in)Theme to display (default: "gelbooru")
                      -et, --external-theme <PATH>  Use external theme to display(specified directory must contains (0..9).png assets, all pngs also must be same size);
                      -s, --seconds                 Display seconds
                      -c, --center                  Center the clock in the terminal window
                      -i, --ignore-support-check    Ignore check for support the console
                    """);
                return 0;
            }

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

            string[] base64Bytes = new string[10];
            try
            {
                //Selected built-in theme to display
                if (!string.IsNullOrWhiteSpace(lParams.Theme) && string.IsNullOrWhiteSpace(lParams.ExternalTheme))
                {
                    base64Bytes = aLoader.GetBytesOfTheme(lParams.Theme);
                }
                //Selected external theme to display
                else if (string.IsNullOrWhiteSpace(lParams.Theme) && !string.IsNullOrWhiteSpace(lParams.ExternalTheme))
                {
                    base64Bytes = aLoader.GetBytesOfExternalTheme(lParams.ExternalTheme);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }

            if (!lParams.IgnoreConsoleSupportCheck && !CheckForConsoleSupport())
            {
                Console.WriteLine("your console emulator not supported");
                return 1;
            }

            ClockRenderer cRenderer = new ClockRenderer(lParams.Center, lParams.Seconds, base64Bytes);
            if (lParams.ShowPreview)
            {
                OpenAlternateScreenBuffer();
                cRenderer.RenderPreview();
                AwaitCtrlC();
                CloseAlternateScreenBuffer();
                return 0;
            }

            else //Standart render
            {
                OpenAlternateScreenBuffer();
                CancellationTokenSource cSource = new CancellationTokenSource();
                _ = cRenderer.RenderClock(cSource.Token);
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
    static bool CheckForConsoleSupport()
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
                        lParams.ExternalTheme = "";
                        lParams.Theme = args[i + 1];
                        i++;
                    }
                    else
                    {
                        errorMessage = "theme not specified";
                    }
                    break;
                case "-et":
                case "--external-theme":
                    if (args.Length > i + 1)
                    {
                        lParams.Theme = "";
                        lParams.ExternalTheme = args[i + 1];
                        i++;
                    }
                    else
                    {
                        errorMessage = "theme path not specified";
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
                case "-i":
                case "--ignore-support-check":
                    lParams.IgnoreConsoleSupportCheck = true;
                    break;
            }
        }
        if (!string.IsNullOrWhiteSpace(errorMessage)) return false;
        return true;
    }
}
