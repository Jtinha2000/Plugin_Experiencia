using HarmonyLib;
using Level_Plugin__Agiota_.Models;
using Newtonsoft.Json;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Extensions;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Diagnostics;
using static UnityEngine.UI.GridLayoutGroup;

namespace Level_Plugin__Agiota_
{
    public class Main : RocketPlugin<Configuration>
    {
        public static Main Instance { get; set; }
        public Harmony HarmonyInstance { get; set; }
        public List<PlayerLevel> PlayerDatabase { get; set; }
        protected override void Load()
        {
            Instance = this;

            HarmonyInstance = new Harmony("LevelPlugin.CommunistDoggu.com");
            HarmonyInstance.PatchAll();

            //Db
            if (!File.Exists(this.Directory + @"\LevelDatabase.json"))
                File.Create(this.Directory + @"\LevelDatabase.json");
            else
                PlayerDatabase = JsonConvert.DeserializeObject<List<PlayerLevel>>(File.ReadAllText(this.Directory + @"\LevelDatabase.json"));
            if (PlayerDatabase is null)
                PlayerDatabase = new List<PlayerLevel>();

            //Events
            U.Events.OnPlayerDisconnected += Events_OnPlayerDisconnected;
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            DamageTool.damageAnimalRequested += DamageTool_damageAnimalRequested;
            InteractableFarm.OnHarvestRequested_Global += InteractableFarm_OnHarvestRequested_Global;
            ResourceManager.onDamageResourceRequested += ResourceManager_DamageResourceRequested;

            //Rocket Reload Sync
            if (Level.isLoaded)
                foreach (SteamPlayer Client in Provider.clients)
                    Events_OnPlayerConnected(Client.ToUnturnedPlayer());
        }
        protected override void Unload()
        {
            HarmonyInstance.UnpatchAll("LevelPlugin.CommunistDoggu.com");

            //Db
            using (StreamWriter Writer = new StreamWriter(this.Directory + @"\LevelDatabase.json", false))
            {
                Writer.Write(JsonConvert.SerializeObject(PlayerDatabase));
            }

            //Events
            U.Events.OnPlayerDisconnected -= Events_OnPlayerDisconnected;
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            DamageTool.damageAnimalRequested -= DamageTool_damageAnimalRequested;
            InteractableFarm.OnHarvestRequested_Global -= InteractableFarm_OnHarvestRequested_Global;
            ResourceManager.onDamageResourceRequested -= ResourceManager_DamageResourceRequested;

            //Rocket Unload Treatment
            foreach (SteamPlayer Client in Provider.clients)
            {
                Events_OnPlayerDisconnected(Client.ToUnturnedPlayer());
                EffectManager.askEffectClearByID(3289, Client.transportConnection);
            }
        }
        public override TranslationList DefaultTranslations => new TranslationList
        {
            { "Harvesting", "<color=green>[XPReward]</color> Você recebeu <color=green>{0} XP</color> por coletar!" },
            { "Online", "<color=green>[XPReward]</color> Você recebeu <color=green>{0} XP</color> pelo seu tempo online!" },
            { "Extracting", "<color=green>[XPReward]</color> Você recebeu <color=green>{0} XP</color> por extrair!" },
            { "Hunting", "<color=green>[XPReward]</color> Você recebeu <color=green>{0} XP</color> por caçar!" },
            { "Fishing", "<color=green>[XPReward]</color> Você recebeu <color=green>{0} XP</color> por pescar!" },

            { "XpReceived", "<color=green>[XPReward]</color> Você recebeu <color=green>{1} XP</color> de <color=blue>{0}</color>!" },
            { "XpSended", "<color=green>[XPCommand]</color> Você enviou com sucesso <color=green>{1} XP</color> para <color=blue>{0}</color>!" },
            { "XpReflexive", "<color=green>[XPCommand]</color> Você se presenteou com <color=green>{0} XP</color>!" },
            { "NotValidNumber", "<color=red>[XPCommand]</color> Insira um número correto para a quantidade de XP a ser enviada! <color=red>{0}</color> não é válido." },
            { "PlayerNotFinded", "<color=red>[XPCommand]</color> O jogador '<color=red>{0}</color>' não foi encontrado... Verifique novamente a ortografia!" },
            { "WrongUsage", "<color=red>[{0}]</color> Faça o uso correto do comando! Tente novamente dessa forma: <color=red>{1}</color>" },
            { "InvalidRangeRegistered", "<color=red>[XPCommand]</color> Insira um número entre 1 ou {0}!" },
            { "InvalidRangeLeveled", "<color=red>[XPCommand]</color> Você ainda não alcançou esse nível! (<color=red>Nível Atual: {0}</color>)" },
            { "AlreadyClaimed", "<color=red>[XPCommand]</color> Você ja resgatou as recompensas do nível <color=red>{0}</color>!" },
            { "NoReward", "<color=red>[XPCommand]</color> O nível <color=red>{0}</color> não oferece nenhuma recompensa..." },
            { "NoLevels", "<color=red>[XPCommand]</color> Você ja resgatou todos as recompensas disponíveis..." },
            { "Redeemed", "<color=green>[XPCommand]</color> Você resgatou com sucesso as recompensas do nível <color=green>{0}</color>! (<color=green>{1}</color> Itens) (<color=green>{2}</color> Veiculos) (<color=green>{3}</color> XP) (<color=green>{4}</color> Permissões)" },
            { "Redeemeds", "<color=green>[XPCommand]</color> Você resgatou com sucesso as recompensas dos níveis: <color=green>{0}</color>! (<color=green>{1}</color> Itens) (<color=green>{2}</color> Veiculos) (<color=green>{3}</color> XP) (<color=green>{4}</color> Permissões)" },
        };

        //UiManaging Events & TimeXP Events
        private void Events_OnPlayerConnected(Rocket.Unturned.Player.UnturnedPlayer player)
        {
            if (!PlayerDatabase.Any(X => X.PlayerID == player.CSteamID.m_SteamID))
                PlayerDatabase.Add(new PlayerLevel(player.CSteamID.m_SteamID));

            PlayerLevel PlayerLvl = PlayerDatabase.First(X => X.PlayerID == player.CSteamID.m_SteamID);
            if (Main.Instance.Configuration.Instance.AlwaysActiveUI)
                EffectManager.sendUIEffect(3289, 3289, player.Player.channel.owner.transportConnection, true);
            PlayerLvl.SendUI();
            PlayerLvl.TimingController = StartCoroutine(PlayerLvl.Counter());
        }
        private void Events_OnPlayerDisconnected(Rocket.Unturned.Player.UnturnedPlayer player)
        {
            PlayerLevel PlayerLvl = PlayerDatabase.First(X => X.PlayerID == player.CSteamID.m_SteamID);
            if (PlayerLvl.TimingController != null)
                StopCoroutine(PlayerLvl.TimingController);
            if (PlayerLvl.DisappearController != null)
                StopCoroutine(PlayerLvl.DisappearController);
            if (PlayerLvl.AnimController != null)
                StopCoroutine(PlayerLvl.AnimController);
        }

        //Actions XP Events
        private void ResourceManager_DamageResourceRequested(CSteamID instigatorSteamID, Transform objectTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            Player Instigator = PlayerTool.getPlayer(instigatorSteamID);
            if (!ResourceManager.tryGetRegion(objectTransform, out byte x, out byte y, out ushort index) || Instigator == null)
                return;

            ResourceSpawnpoint Resource = ResourceManager.getResourceSpawnpoint(x, y, index);
            if ((!Configuration.Instance.MiningResourcesIDs.Contains(Resource.asset.id) && !Configuration.Instance.MiningResourcesIDs.Contains(0)) || Resource.health - pendingTotalDamage > 0)
                return;

            PlayerDatabase.First(X => X.PlayerID == instigatorSteamID.m_SteamID).AddXP(Configuration.Instance.MiningXP, "Extracting");
        }
        private void InteractableFarm_OnHarvestRequested_Global(InteractableFarm harvestable, SteamPlayer instigatorPlayer, ref bool shouldAllow)
        {
            BarricadeDrop Barricade = BarricadeManager.FindBarricadeByRootTransform(harvestable.transform);
            if (instigatorPlayer == null || Barricade == null || Configuration.Instance.HarvestablesBlacklist.Contains(Barricade.asset.id) || !shouldAllow)
                return;

            PlayerDatabase.First(X => X.PlayerID == instigatorPlayer.playerID.steamID.m_SteamID).AddXP(Configuration.Instance.HarvestXP, "Harvesting");
        }
        private void DamageTool_damageAnimalRequested(ref DamageAnimalParameters parameters, ref bool shouldAllow)
        {
            Player Instigator = parameters.instigator as Player;
            if (Instigator == null || parameters.animal.GetHealth() - parameters.damage > 0)
                return;

            PlayerDatabase.FirstOrDefault(X => X.PlayerID == Instigator.channel.owner.playerID.steamID.m_SteamID).AddXP(Configuration.Instance.HuntXP, "Hunting");
        }
    }
}
