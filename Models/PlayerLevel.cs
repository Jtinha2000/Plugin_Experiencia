using Level_Plugin__Agiota_.Models.Configuration;
using Mono.Cecil;
using Newtonsoft.Json;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace Level_Plugin__Agiota_.Models
{
    public class PlayerLevel
    {
        [JsonIgnore]
        public Coroutine TimingController { get; set; }
        [JsonIgnore]
        public Coroutine AnimController { get; set; }
        [JsonIgnore]
        public Coroutine DisappearController { get; set; }
        [JsonIgnore]
        public bool IsActive { get; set; }

        public bool[] Redeemed { get; set; }
        public ulong PlayerID { get; set; }
        public byte CurrentLevel { get; set; }
        public uint CurrentXP { get; set; }
        public PlayerLevel()
        {

        }
        public PlayerLevel(ulong playerID)
        {
            PlayerID = playerID;
            Redeemed = new bool[Main.Instance.Configuration.Instance.Levels.Count()];
            for (int i = 0; i < Redeemed.Length; i++)
                Redeemed[i] = false;
            CurrentLevel = 1;
            CurrentXP = 0;
        }
        public IEnumerator Counter()
        {
            while (true)
            {
                yield return new WaitForSeconds(Main.Instance.Configuration.Instance.OnlineTimeSeconds);
                AddXP(Main.Instance.Configuration.Instance.OnlineTimeXP, "Online");
            }
        }

        public void SendUI()
        {
            Player Owner = PlayerTool.getPlayer(new Steamworks.CSteamID(PlayerID));
            if (Owner == null)
                return;

            IsActive = true;
            if (DisappearController != null)
            {
                Main.Instance.StopCoroutine(DisappearController);
                DisappearController = null;
            }
            if (!Main.Instance.Configuration.Instance.AlwaysActiveUI)
                EffectManager.sendUIEffect(3289, 3289, Owner.channel.owner.transportConnection, true);

            float ProgressPorcentage = (CurrentXP * 100) / Main.Instance.Configuration.Instance.Levels.FirstOrDefault(X => X.Level == CurrentLevel).LevelUpXP;
            EffectManager.sendUIEffectText(3289, Owner.channel.owner.transportConnection, true, "Current", $"<size=18>Lv. {CurrentLevel}</size> <size=4>|</size> {CurrentXP}/{Main.Instance.Configuration.Instance.Levels.FirstOrDefault(X => X.Level == CurrentLevel).LevelUpXP}");
            EffectManager.sendUIEffectVisibility(3289, Owner.channel.owner.transportConnection, true, "Up", false);
            for (int CurrentUiBarIndex = 1; CurrentUiBarIndex <= 100; CurrentUiBarIndex++)
            {
                EffectManager.sendUIEffectVisibility(3289, Owner.channel.owner.transportConnection, true, $"Filler{CurrentUiBarIndex} Red", false);
                EffectManager.sendUIEffectVisibility(3289, Owner.channel.owner.transportConnection, true, $"Filler{CurrentUiBarIndex} Green", false);
                EffectManager.sendUIEffectVisibility(3289, Owner.channel.owner.transportConnection, true, $"Filler{CurrentUiBarIndex}", CurrentUiBarIndex <= ProgressPorcentage);
            }
            DisappearController = Main.Instance.StartCoroutine(RemoveUI(Owner));
        }
        public void AddXP(uint XPAmount, string Cause = null)
        {
            bool Upgraded = false;
            byte OldLevel = CurrentLevel;
            uint OldXP = CurrentXP;
            float OldPorcentage = (CurrentXP * 100) / Main.Instance.Configuration.Instance.Levels.FirstOrDefault(X => X.Level == CurrentLevel).LevelUpXP;
            CurrentXP += XPAmount;
            while (CurrentXP >= Main.Instance.Configuration.Instance.Levels.FirstOrDefault(X => X.Level == CurrentLevel).LevelUpXP)
            {
                Upgraded = true;
                CurrentXP -= Main.Instance.Configuration.Instance.Levels.FirstOrDefault(X => X.Level == CurrentLevel).LevelUpXP;
                CurrentLevel++;
            }

            Player Owner = PlayerTool.getPlayer(new Steamworks.CSteamID(PlayerID));
            if (Owner != null)
            {
                IsActive = true;
                if (Cause != null)
                    ChatManager.serverSendMessage(Main.Instance.Translate(Cause, XPAmount.ToString()), Color.white, null, Owner.channel.owner, EChatMode.SAY, null, true);
                if (DisappearController != null)
                {
                    Main.Instance.StopCoroutine(DisappearController);
                    DisappearController = null;
                }
                if (AnimController != null)
                    Main.Instance.StopCoroutine(AnimController);
                else if (!Main.Instance.Configuration.Instance.AlwaysActiveUI)
                    EffectManager.sendUIEffect(3289, 3289, Owner.channel.owner.transportConnection, true);
                EffectManager.sendUIEffectText(3289, Owner.channel.owner.transportConnection, true, "Current", $"<size=18>Lv. {CurrentLevel}</size> <size=4>|</size> {CurrentXP}/{Main.Instance.Configuration.Instance.Levels.FirstOrDefault(X => X.Level == CurrentLevel).LevelUpXP}"); 
                EffectManager.sendUIEffectVisibility(3289, Owner.channel.owner.transportConnection, true, "Up", Upgraded);
                for (int CurrentUiBarIndex = 1; CurrentUiBarIndex <= 100; CurrentUiBarIndex++)
                {
                    EffectManager.sendUIEffectVisibility(3289, Owner.channel.owner.transportConnection, true, $"Filler{CurrentUiBarIndex} Red", false);
                    EffectManager.sendUIEffectVisibility(3289, Owner.channel.owner.transportConnection, true, $"Filler{CurrentUiBarIndex} Green", false);
                    EffectManager.sendUIEffectVisibility(3289, Owner.channel.owner.transportConnection, true, $"Filler{CurrentUiBarIndex}", CurrentUiBarIndex <= OldPorcentage);
                }
                AnimController = Main.Instance.StartCoroutine(ExpAnimation(OldPorcentage, Owner));
            }
        }

        public IEnumerator ExpAnimation(float OldPorcentage, Player Owner = null)
        {
            if (Owner == null)
            {
                Owner = PlayerTool.getPlayer(new Steamworks.CSteamID(PlayerID));
                if (Owner == null)
                    yield break;
            }
            yield return new WaitForSeconds(0.2f);

            float NewPorcentage = (CurrentXP * 100) / Main.Instance.Configuration.Instance.Levels.FirstOrDefault(X => X.Level == CurrentLevel).LevelUpXP;
            if (OldPorcentage > NewPorcentage)
            {
                //-XP
                for (int UiIndex = (int)OldPorcentage; UiIndex >= NewPorcentage; UiIndex--)
                {
                    EffectManager.sendUIEffectVisibility(3289, Owner.channel.owner.transportConnection, true, $"Filler{UiIndex} Red", true);
                    yield return new WaitForSeconds(0.06f);
                    EffectManager.sendUIEffectVisibility(3289, Owner.channel.owner.transportConnection, true, $"Filler{UiIndex}", false);
                }
            }
            else
            {

                //+XP
                for (int UiIndex = (int)OldPorcentage; UiIndex <= NewPorcentage; UiIndex++)
                {
                    EffectManager.sendUIEffectVisibility(3289, Owner.channel.owner.transportConnection, true, $"Filler{UiIndex} Green", true);
                    yield return new WaitForSeconds(0.06f);
                }
            }
            yield return new WaitForSeconds(0.06f);

            if (OldPorcentage < NewPorcentage)
            {
                //+XP
                for (int UiIndex = (int)OldPorcentage; UiIndex <= NewPorcentage; UiIndex++)
                {
                    EffectManager.sendUIEffectVisibility(3289, Owner.channel.owner.transportConnection, true, $"Filler{UiIndex}", true);
                    yield return new WaitForSeconds(0.06f);
                    EffectManager.sendUIEffectVisibility(3289, Owner.channel.owner.transportConnection, true, $"Filler{UiIndex} Green", false);
                }
            }
            else
            {
                for (int UiIndex = (int)OldPorcentage; UiIndex >= NewPorcentage; UiIndex--)
                {
                    EffectManager.sendUIEffectVisibility(3289, Owner.channel.owner.transportConnection, true, $"Filler{UiIndex} Red", false);
                    yield return new WaitForSeconds(0.06f);
                }
            }

            EffectManager.sendUIEffectText(3289, Owner.channel.owner.transportConnection, true, "Current", $"<size=18>Lv. {CurrentLevel}</size> <size=4>|</size> {CurrentXP}/{Main.Instance.Configuration.Instance.Levels.FirstOrDefault(X => X.Level == CurrentLevel).LevelUpXP}");
            AnimController = null;
            if (DisappearController != null)
                Main.Instance.StopCoroutine(DisappearController);
            DisappearController = Main.Instance.StartCoroutine(RemoveUI(Owner));
        }
        public IEnumerator RemoveUI(Player Owner = null)
        {
            if (Main.Instance.Configuration.Instance.AlwaysActiveUI)
                yield break;

            if (Owner == null)
            {
                Owner = PlayerTool.getPlayer(new Steamworks.CSteamID(PlayerID));
                if (Owner == null)
                    yield break;
            }

            yield return new WaitForSeconds(10);
            yield return new WaitUntil(() => AnimController == null);

            if (AnimController != null)
            {
                Main.Instance.StopCoroutine(AnimController);
                AnimController = null;
            }
            EffectManager.askEffectClearByID(3289, Owner.channel.owner.transportConnection);
            IsActive = false;
            DisappearController = null;
        }
    }
}
