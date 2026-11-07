public class SupportChecker
{
    static string[] supportedTerminals = [
      "kitty", "ghostty", "wezterm", "konsole"
    ];
    public bool HasKittyGraphicsProtocolSupport()
    {
        //Get all system variables
        string[] variables = Environment.GetEnvironmentVariables().Keys.Cast<string>().ToArray();
        int specificVarCount = 0;
        foreach (string terminal in supportedTerminals)
        {
            //Check for specific envs of supported terminals
            if (variables.Any(var => var.ToLower().Contains(terminal))) specificVarCount++;
        }
        return (specificVarCount > 0);
    }
}
