using Dalamud.Game.Command;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnemyListDebuffs.StatusNode;

namespace EnemyListDebuffs
{
    public class EnemyListDebuffsPlugin : IDalamudPlugin
    {
        public string Name => "EnemyListDebuffs";

        internal DalamudPluginInterface Interface;
        internal PluginAddressResolver Address;
        internal StatusNodeManager StatusNodeManager;
        internal AddonEnemyListHooks Hooks;
        internal EnemyListDebuffsPluginUI UI;
        internal EnemyListDebuffsPluginConfig Config;

        internal bool InPvp;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            Interface = pluginInterface ?? throw new ArgumentNullException(nameof(pluginInterface), "DalamudPluginInterface cannot be null");

            Config = pluginInterface.GetPluginConfig() as EnemyListDebuffsPluginConfig ?? new EnemyListDebuffsPluginConfig();
            Config.Initialize(pluginInterface);

            if (!FFXIVClientStructs.Resolver.Initialized) FFXIVClientStructs.Resolver.Initialize();

            Address = new PluginAddressResolver();
            Address.Setup(Interface.TargetModuleScanner);

            StatusNodeManager = new StatusNodeManager(this);

            Hooks = new AddonEnemyListHooks(this);
            Hooks.Initialize();

            UI = new EnemyListDebuffsPluginUI(this);

            Interface.ClientState.TerritoryChanged += OnTerritoryChange;

            Interface.CommandManager.AddHandler("/eldebuffs", new CommandInfo(this.ToggleConfig)
            {
                HelpMessage = "Toggles config window."
            });
        }
        public void Dispose()
        {
            Interface.ClientState.TerritoryChanged -= OnTerritoryChange;
            Interface.CommandManager.RemoveHandler("/eldebuffs");

            UI.Dispose();
            Hooks.Dispose();
            StatusNodeManager.Dispose();
        }

        private void OnTerritoryChange(object sender, ushort e)
        {
            try
            {
                var territory = this.Interface.Data.GetExcelSheet<TerritoryType>().GetRow(e);
                this.InPvp = territory.IsPvpZone;
            }
            catch (KeyNotFoundException)
            {
                PluginLog.Warning("Could not get territory for current zone");
            }
        }

        private void ToggleConfig(string command, string args)
        {
            UI.ToggleConfig();
        }
    }
}
