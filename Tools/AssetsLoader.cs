using System.Reflection;


public class AssetsLoader
{
    public string[] GetListOfThemes()
    {
        Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        string[] resourceNames = assembly.GetManifestResourceNames();
        return resourceNames.Select(name => name.Split(".")[2]).ToHashSet().ToArray();
    }
    public string[] GetBytesOfTheme(string name)
    {
        if (!GetListOfThemes().Contains(name)) throw new Exception("theme not exists");
        string baseName = $"moe-clock.Assets.{name}";
        Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        byte[][] bytes = new byte[10][];
        for (int i = 0; i < 10; i++)
        {
            using Stream stream = assembly.GetManifestResourceStream($"{baseName}.{i}.png");
            bytes[i] = new byte[stream.Length];
            stream.ReadExactly(bytes[i]);
        }
        string[] base64Bytes = new string[10];
        for (int i = 0; i < 10; i++)
        {
            byte[] blob = bytes[i];
            base64Bytes[i] = Convert.ToBase64String(blob);
        }
        return base64Bytes;
    }
}
