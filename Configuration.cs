using Level_Plugin__Agiota_.Models.Configuration;
using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Level_Plugin__Agiota_
{
    public class Configuration : IRocketPluginConfiguration
    {
        public bool AlwaysActiveUI { get; set; }
        public List<LevelConfig> Levels { get; set; }
        [XmlArrayItem("ResourceID")]
        public List<ushort> MiningResourcesIDs { get; set; }
        [XmlArrayItem("HarvestableID")]
        public List<ushort> HarvestablesBlacklist { get; set; }
        public uint HuntXP { get; set; }
        public uint FishingXP { get; set; }
        public uint OnlineTimeXP { get; set; }
        public uint OnlineTimeSeconds { get; set; }
        public uint HarvestXP { get; set; }
        public uint MiningXP { get; set; }
        public void LoadDefaults()
        {
            AlwaysActiveUI = true;

            MiningResourcesIDs = new List<ushort> { 0 };
            HarvestablesBlacklist = new List<ushort> { 0 };
            HuntXP = 100;
            FishingXP = 50;
            OnlineTimeXP = 25;
            OnlineTimeSeconds = 30;
            HarvestXP = 5;
            MiningXP = 10;
            
            Levels = new List<LevelConfig>
            {
                new LevelConfig(
                    1,
                    100,
                    new LevelReward(new List<ushort> { 0 }, 0, new List<string> { "" }, new List<ushort> { 0 }))
            };
            for (int CurrentLevel = 2; CurrentLevel <= 100; CurrentLevel++) 
            {
                Levels.Add(new LevelConfig(
                    (byte)CurrentLevel,
                    (uint)(Levels[CurrentLevel-2].LevelUpXP * (1.05)),
                    new LevelReward(new List<ushort> { 0 }, 0, new List<string> { "" }, new List<ushort> { 0 })));
            }
        }
    }
}
