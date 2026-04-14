using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Level_Plugin__Agiota_.Models.Configuration
{
    public class LevelConfig
    {
        public int Level { get; set; }
        public uint LevelUpXP { get; set; }
        public LevelReward Reward { get; set; }
        public LevelConfig()
        {
            
        }
        public LevelConfig(int level, uint levelUpXP, LevelReward reward)
        {
            Level = level;
            LevelUpXP = levelUpXP;
            Reward = reward;
        }
    }
}
