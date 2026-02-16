using CounterStrikeSharp.API.Core;
using CS2_Poor_MapDecals.Config;
using CS2_Poor_MapDecals.Managers;
using CS2_Poor_MapDecals.Menu;
using CS2_Poor_MapDecals.Utils;
using Microsoft.Extensions.Logging;

namespace CS2_Poor_MapDecals;
public class CS2_Poor_MapDecals : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "CS2_Poor_MapAdvertisements";

    public override string ModuleVersion => "1.0";

    public override string ModuleAuthor => "Letaryat | github.com/letaryat";

    public override string ModuleDescription => "Creates map advertisements.";

    public required PluginConfig Config { get; set; }

    public static CS2_Poor_MapDecals? Instance { get; private set; }

    public EventManager? EventManager { get; private set; }
    public PropManager? PropManager { get; private set; }

    public PluginUtils? PluginUtils { get; private set; }
    public CommandsManager? CommandsManager { get; private set; }

    public PluginMenu? MenuManager {get; private set;}
    public override void Load(bool hotReload)
    {
        Console.WriteLine("Loaded CS2_Poor_MapAdvertisements");
        Instance = this;

        EventManager = new EventManager(this);
        PluginUtils = new PluginUtils(this);
        CommandsManager = new CommandsManager(this);
        PropManager = new PropManager(this);
        MenuManager = new PluginMenu(this);

        EventManager.RegisterEvents();
        CommandsManager.RegisterCommands();

    }

    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;
    }
    public override void Unload(bool hotReload)
    {
        Console.WriteLine("Unloaded CS2_Poor_MapAdvertisements");
    }

    public void DebugMode(string message)
    {
        if (Config.Debug)
        {
            Logger.LogInformation(message);
        }
    }

}
