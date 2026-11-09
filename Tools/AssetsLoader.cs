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
        bool isThemeExits = GetListOfThemes().Contains(name);
        if (!isThemeExits) throw new Exception("theme not exists");
        string baseName = $"moe-clock.Assets.{name}";
        Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        byte[][] bytes = new byte[10][];
        // Read bytes of images
        for (int i = 0; i < 10; i++)
        {
            using Stream stream = assembly.GetManifestResourceStream($"{baseName}.{i}.png");
            bytes[i] = new byte[stream.Length];
            stream.ReadExactly(bytes[i]);
        }
        string[] base64Bytes = ConvertBytesToBase64Strings(bytes);
        return base64Bytes;
    }
    public string[] GetBytesOfExternalTheme(string path)
    {
        bool isDirectoryExists = Directory.Exists(path);
        if (!isDirectoryExists) throw new Exception("specified directory not exists");
        byte[][] bytes = new byte[10][];
        // Read bytes of images
        for (int i = 0; i < 10; i++)
        {
            string filePath = Path.Combine(path, $"{i}.png");
            bool isImageExists = File.Exists(filePath);
            if (!isImageExists) throw new Exception("not all images (0..9).png exists");
            bytes[i] = File.ReadAllBytes(filePath);
        }
        string[] base64Bytes = ConvertBytesToBase64Strings(bytes);
        return base64Bytes;
    }
    public string[] ConvertBytesToBase64Strings(byte[][] bytes)
    {
        string[] base64Bytes = new string[bytes.Length];
        for (int i = 0; i < 10; i++)
        {
            base64Bytes[i] = Convert.ToBase64String(bytes[i]);
        }
        return base64Bytes;
    }
}
