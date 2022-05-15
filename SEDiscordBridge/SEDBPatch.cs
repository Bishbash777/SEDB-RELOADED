using System.Reflection;
using Torch.API.Session;
using Torch.Commands;
using Torch.Managers.PatchManager;
using Torch.Server;

namespace SEDiscordBridge
{
    [PatchShim]
    public static class SEDBPatch
    {
        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(TorchCommands).GetMethod(nameof(TorchCommands.Restart))).Prefixes.Add(typeof(SEDBPatch).GetMethod(nameof(SEDBPatch.OnRestarting)));
            ctx.GetPattern(typeof(TorchCommands).GetMethod(nameof(TorchCommands.CancelRestart))).Prefixes.Add(typeof(SEDBPatch).GetMethod(nameof(SEDBPatch.OnRestartCancel)));
            ctx.GetPattern(typeof(Initializer).GetMethod("HandleException", BindingFlags.Instance | BindingFlags.NonPublic)).Prefixes.Add(typeof(SEDBPatch).GetMethod(nameof(SEDBPatch.OnCrash)));
        }

        public static void OnRestarting()
        {
            SEDiscordBridgePlugin.Static.IsRestart = true;
        }

        public static void OnRestartCancel()
        {
            SEDiscordBridgePlugin.Static.IsRestart = false;
        }

        public static void OnCrash()
        {
            if (SEDiscordBridgePlugin.Static.DDBridge == null) return;
            SEDiscordBridgePlugin.Static.SessionChanged(null, TorchSessionState.Unloading);
            SEDiscordBridgePlugin.Static.DDBridge.StopDiscord();
        }
    }
}
