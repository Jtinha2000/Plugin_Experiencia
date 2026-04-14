using Level_Plugin__Agiota_.Models;
using Rocket.API;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Level_Plugin__Agiota_.Commands
{
    public class XPCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "GiveXP";

        public string Help => "";

        public string Syntax => "/GiveXP <Player> <Amount>";

        public List<string> Aliases => new List<string> { "XpGive", "XP", "AddXp", "XpAdd" };

        public List<string> Permissions => new List<string> { "LevelPlugin.GiveXP" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            Player Caller = PlayerTool.getPlayer(caller.DisplayName);
            if(command.Length < 2)
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("WrongUsage", "XpCommand", Syntax), Color.white, null, Caller.channel.owner, EChatMode.SAY, null, true);
                return;
            }

            Player Target = PlayerTool.getPlayer(command[0]);
            if (Target == null)
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("PlayerNotFinded", command[0]), Color.white, null, Caller.channel.owner, EChatMode.SAY, null, true);
                return;
            }
            if (!uint.TryParse(command[1], out uint XpAmount))
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("NotValidNumber", command[1]), Color.white, null, Caller.channel.owner, EChatMode.SAY, null, true);
                return;
            }

            Main.Instance.PlayerDatabase.First(X => X.PlayerID == Target.channel.owner.playerID.steamID.m_SteamID).AddXP(XpAmount);
            if(Target != Caller)
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("XpSended", Target.channel.owner.playerID.characterName, XpAmount), Color.white, null, Caller.channel.owner, EChatMode.SAY, null, true);
                ChatManager.serverSendMessage(Main.Instance.Translate("XpReceived", Caller.channel.owner.playerID.characterName, XpAmount), Color.white, null, Target.channel.owner, EChatMode.SAY, null, true);
            }
            else
                ChatManager.serverSendMessage(Main.Instance.Translate("XpReflexive", XpAmount), Color.white, null, Caller.channel.owner, EChatMode.SAY, null, true);
        }
    }
}
