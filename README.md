[![trophy](https://github-profile-trophy.vercel.app/?username=zhivotnoya&no-bg=true&no-frame=true&theme=matrix&row=1&column=5&rank=-SECRET,-SSS,-SS,-S)](https://github.com/ryo-ma/github-profile-trophy)

I'm a part-time coder that makes things. My most popular repository is my XION-ChaseCam. I see it used all the time on streams and videos, and even other folks have used it in software. It's public domain and I enjoy the fact others have used it.  If you'd like your name included in the readme for that repository, submit a pull-request on that repository and I'll add it in. Be sure to include a link to the social platform (or github) you wish to be contacted on.

Otherwise, everyone have fun and be excellent to each other!

## CarHUD Plugin

This repository also contains the source code for **CarHUD**, a lightweight
vehicle HUD plugin for **RAGEPluginHook** / **LSPDFR**.

### Features

- Digital speedometer with optional metric or imperial units.
- Color–coded vehicle health (and optional fuel level).
- HUD position can be configured to one of the following:
  - `BottomCenter`
  - `BottomRight`
  - `AboveMinimap`
  - `RightOfMinimap`

Configuration values are stored in `Plugins/CarHUD.ini` and can be tweaked to
suit your preferences.

### Building

To build the plugin you will need the latest **RAGEPluginHook** SDK. Open the
`CarHUD` project in Visual Studio, reference `RagePluginHook.dll`, compile, and
place the resulting DLL along with `CarHUD.ini` in your game's `Plugins`
folder. The project targets **.NET Framework 4.8** as recommended for modern
versions of LSPDFR. If `Plugins/CarHUD.ini` is missing, the plugin will create
a new file with default settings on first launch.
