using HarmonyLib;
using Rocket.Unturned.Chat;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Level_Plugin__Agiota_.Patches
{
    [HarmonyPatch(typeof(UseableFisher), "ReceiveCatch")]
    public static class FishingPatch
    {
        [HarmonyPostfix]
        public static void FishingMetheod(UseableFisher __instance)
        {
            Player Instigator = __instance.player;
            if (Instigator == null)
                return;

            Main.Instance.PlayerDatabase.FirstOrDefault(X => X.PlayerID == Instigator.channel.owner.playerID.steamID.m_SteamID).AddXP(Main.Instance.Configuration.Instance.FishingXP, "Fishing");
        }
    }
}
