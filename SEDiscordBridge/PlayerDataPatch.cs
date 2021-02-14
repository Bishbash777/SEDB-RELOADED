
using System.Reflection;
using Torch.Managers.PatchManager;
using NLog;
using VRage.Network;
using Sandbox.Engine.Multiplayer;
using Torch.Managers.ChatManager;
using VRage.Game;
using Torch.Utils;
using Sandbox.Game.Gui;
using System;
using Sandbox.Game.World;

namespace SEDiscordBridge {
    [PatchShim]
    public static class PlayerdDataPatch {
        public static bool debug = false;
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static MethodInfo FindOverLoadMethod(MethodInfo[] methodInfo, string name, int parameterLenth) {
            MethodInfo method = null;
            foreach (var DecalredMethod in methodInfo) {
                if (debug)
                    Log.Info($"Method name: {DecalredMethod.Name}");
                if (DecalredMethod.GetParameters().Length == parameterLenth && DecalredMethod.Name == name) {
                    method = DecalredMethod;
                    break;
                }
            }
            return method;
        }

        public static void Patch(PatchContext ctx) {
            var target = FindOverLoadMethod(typeof(MyMultiplayerBase).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static), "OnChatMessageReceived_Server", 1);
            var patchMethod = typeof(PlayerdDataPatch).GetMethod(nameof(OnChatMessageReceived_Server), BindingFlags.Static | BindingFlags.NonPublic);
            ctx.GetPattern(target).Prefixes.Add(patchMethod);

            Log.Info("Patched OnChatMessageReceived_Server!");
        }

        private static bool OnChatMessageReceived_Server(ref ScriptedChatMsg msg) {
            return false;
        }

    }
}