# moe-clock
A lightweight terminal clock featuring anime-style digit sprites (similar to those found on Rule34 or Gelbooru).

<img width="800" height="450" alt="demo" src="https://github.com/user-attachments/assets/8dedc574-46cb-466b-8a82-bb7e3459c7bd" />
Requires a terminal that supports the Kitty Graphics Protocol.

## Instalation
Installation and compilation require the .NET SDK (version 9 or higher). Once built, the binary runs independently.
```bash
git clone https://github.com/wellmq/moe-clock.git
cd moe-clock

#Compilation in NativeAOT binary
dotnet publish -c Release -r linux-x64

#Copy binary to local PATH
cp bin/Release/net*/linux-x64/publish/moe-clock ~/.local/bin/
```

## Execute
```bash
#Execute with default (gelbooru) theme
moe-clock 

#Get a list of possible arguments
moe-clock --help

#Execute with specified theme, centering, and displaying seconds
moe-clock -t gelbooruHBig -c -s
```

## License
This project is licensed under the [MIT License](LICENSE).
