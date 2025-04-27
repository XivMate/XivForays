# XivMate Data Gathering Forays Dalamud

XivMate Data Gathering Forays Dalamud is a plugin for Dalamud that crowdsources data for Adventuring Forays in Final Fantasy XIV, specifically for Eureka, Zadnor, and Bozjan Southern Front. This plugin collects and uploads data about FATEs (Full Active Time Events) in these areas to a central server for analysis and sharing.

## Features

- **FATE Tracking**: Automatically tracks active FATEs in specified Foray territories.
- **Data Upload**: Uploads FATE data to a specified API endpoint.
- **Configuration**: Provides a configuration window for setting API URL and API Key.
- **UI Integration**: Includes a main window and configuration window for user interaction.

## Prerequisites

- XIVLauncher, FINAL FANTASY XIV, and Dalamud must be installed and the game must have been run with Dalamud at least once.
- A .NET Core 8 SDK must be installed and configured.

## Getting Started

### Cloning the Repository

To begin, clone this repository to your local machine:

```sh
git clone https://github.com/yourusername/XivMate.DataGathering.Forays.Dalamud.git
```

### Building the Project

1. Open `XivMate.DataGathering.Forays.Dalamud.sln` in your C# editor of choice (e.g., Visual Studio 2022 or JetBrains Rider).
2. Build the solution. By default, this will build a `Debug` build, but you can switch to `Release` in your IDE.
3. The resulting plugin can be found at `XivMate.DataGathering.Forays.Dalamud/bin/x64/Debug/XivMate.DataGathering.Forays.Dalamud.dll` (or `Release` if appropriate).

### Activating the Plugin In-Game

1. Launch the game and use `/xlsettings` in chat or `xlsettings` in the Dalamud Console to open the Dalamud settings.
2. Go to `Experimental`, and add the full path to the `XivMate.DataGathering.Forays.Dalamud.dll` to the list of Dev Plugin Locations.
3. Use `/xlplugins` (chat) or `xlplugins` (console) to open the Plugin Installer.
4. Go to `Dev Tools > Installed Dev Plugins`, and enable `XivMate.DataGathering.Forays.Dalamud`.

### Configuration

1. Use `/xivmate` in chat to open the main window.
2. Click on "Show Settings" to open the configuration window.
3. Set the API URL and API Key in the System tab.

## Project Structure

- **Configuration**: Contains configuration classes for the plugin.
- **Extensions**: Contains extension methods used throughout the plugin.
- **Gathering**: Contains modules for gathering data, including the `FateModule`.
- **Models**: Contains data models used by the plugin.
- **Services**: Contains services for interacting with external APIs and game data.
- **Windows**: Contains UI windows for the plugin.

## License

This project is licensed under the GNU Affero General Public License v3.0. See the [LICENSE.md](LICENSE.md) file for details.

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request with your changes.

## Contact

For questions or support, please join the [Dalamud Discord](https://discord.gg/holdshift).

---

This project is a template for creating Dalamud plugins. For more detailed questions, come ask in the Discord.
