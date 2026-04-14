using Level_Plugin__Agiota_.Models;
using Level_Plugin__Agiota_.Models.Configuration;
using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Extensions;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Level_Plugin__Agiota_.Commands
{
    public class RedeemCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "Redeem";

        public string Help => "";

        public string Syntax => "/Redeem <Level/All>";

        public List<string> Aliases => new List<string> { "Regastar", "Reward", "Rewards", "LevelReward", "RewardLevel", "Recompensas" };

        public List<string> Permissions => new List<string> { "LevelPlugin.RedeemCommand" };

        public List<string> AllAliases => new List<string> { "all", "todos", "tudo", "100%" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            Player Caller = PlayerTool.getPlayer(caller.DisplayName);
            PlayerLevel PlayerData = Main.Instance.PlayerDatabase.First(X => X.PlayerID == Caller.channel.owner.playerID.steamID.m_SteamID);
            if (!PlayerData.IsActive)
                PlayerData.SendUI();
            if (command.Length < 1)
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("WrongUsage", "RedeemCommand", Syntax), Color.white, null, Caller.channel.owner, EChatMode.SAY, null, true);
                return;
            }

            if (AllAliases.Contains(command[0].ToLower()))
            {
                List<int> RedeemedLevels = new List<int>();
                int XpAmount = 0;
                int ItemAmount = 0;
                int VehicleAmount = 0;
                int PermissionAmount = 0;
                foreach (LevelConfig Config in Main.Instance.Configuration.Instance.Levels)
                {
                    if (Config.Level > PlayerData.CurrentLevel || PlayerData.Redeemed[Config.Level - 1] || Config.Reward == null || (Config.Reward.ItemsReward.Count(X => X != 0) == 0 && Config.Reward.VehiclesReward.Count(X => X != 0) == 0 && Config.Reward.ExperienceReward == 0 && Config.Reward.PermissionsReward.Count(X => X != null && X.Replace(" ", "").Length > 0) == 0))
                        continue;

                    PlayerData.Redeemed[Config.Level - 1] = true;
                    Config.Reward.RedeemReward(Caller);
                    RedeemedLevels.Add(Config.Level);

                    XpAmount += Config.Reward.ExperienceReward;
                    ItemAmount += Config.Reward.ItemsReward.Count(X => X != 0);
                    VehicleAmount += Config.Reward.VehiclesReward.Count(X => X != 0);
                    PermissionAmount += Config.Reward.PermissionsReward.Count(X => X != null && X.Replace(" ", "").Length > 0);
                }

                if (RedeemedLevels.Count == 0)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("NoLevels"), Color.white, null, Caller.channel.owner, EChatMode.SAY, null, true);
                    return;
                }

                string LevelText = "";
                foreach (int Level in RedeemedLevels)
                    LevelText += Level + "<color=white>,</color> ";
                LevelText = LevelText.Remove(LevelText.Length - 2, 2);
                ChatManager.serverSendMessage(Main.Instance.Translate("Redeemeds", LevelText, ItemAmount, VehicleAmount, XpAmount, PermissionAmount), Color.white, null, Caller.channel.owner, EChatMode.SAY, null, true);
            }
            else if (int.TryParse(command[0], out int TargetLevel))
            {
                if (TargetLevel > Main.Instance.Configuration.Instance.Levels.Count || TargetLevel < 1)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("InvalidRangeRegistered", Main.Instance.Configuration.Instance.Levels.Count), Color.white, null, Caller.channel.owner, EChatMode.SAY, null, true);
                    return;
                }
                if (TargetLevel > PlayerData.CurrentLevel)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("InvalidRangeLeveled", PlayerData.CurrentLevel), Color.white, null, Caller.channel.owner, EChatMode.SAY, null, true);
                    return;
                }
                if (PlayerData.Redeemed[TargetLevel - 1])
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("AlreadyClaimed", TargetLevel), Color.white, null, Caller.channel.owner, EChatMode.SAY, null, true);
                    return;
                }

                LevelReward Reward = Main.Instance.Configuration.Instance.Levels[TargetLevel - 1].Reward;
                if (Reward == null || (Reward.ItemsReward.Count(X => X != 0) == 0 && Reward.VehiclesReward.Count(X => X != 0) == 0 && Reward.ExperienceReward == 0 && Reward.PermissionsReward.Count(X => X != null && X.Replace(" ", "").Length > 0) == 0))
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("NoReward", TargetLevel), Color.white, null, Caller.channel.owner, EChatMode.SAY, null, true);
                    return;
                }

                PlayerData.Redeemed[TargetLevel - 1] = true;
                Reward.RedeemReward(Caller);
                ChatManager.serverSendMessage(Main.Instance.Translate("Redeemed", TargetLevel, Reward.ItemsReward.Count(X => X != 0), Reward.VehiclesReward.Count(X => X != 0), Reward.ExperienceReward, Reward.PermissionsReward.Count(X => X != null && X.Replace(" ", "").Length > 0)), Color.white, null, Caller.channel.owner, EChatMode.SAY, null, true);
            }
            else
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("WrongUsage", "RedeemCommand", Syntax), Color.white, null, Caller.channel.owner, EChatMode.SAY, null, true);
                return;
            }
        }
    }
}
