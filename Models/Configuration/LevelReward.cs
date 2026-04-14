using Rocket.Core;
using Rocket.Unturned.Extensions;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Level_Plugin__Agiota_.Models.Configuration
{
    public class LevelReward
    {
        [XmlArrayItem("ItemID")]
        public List<ushort> ItemsReward { get; set; }
        public int ExperienceReward { get; set; }
        [XmlArrayItem("GroupName")]
        public List<string> PermissionsReward { get; set; }
        [XmlArrayItem("VehicleID")]
        public List<ushort> VehiclesReward { get; set; }
        public LevelReward()
        {
            
        }
        public LevelReward(List<ushort> itemsReward, int experienceReward, List<string> permissionsReward, List<ushort> vehiclesReward)
        {
            ItemsReward = itemsReward;
            ExperienceReward = experienceReward;
            PermissionsReward = permissionsReward;
            VehiclesReward = vehiclesReward;
        }

        public void RedeemReward(Player Caller)
        {
            if (Caller == null)
                return;

            foreach (ushort ID in ItemsReward)
                if (!Caller.inventory.tryAddItemAuto(new Item(ID, true), true, true, true, true))
                    ItemManager.dropItem(new Item(ID, true), Caller.transform.position, true, true, true);
            foreach (ushort ID in VehiclesReward)
                VehicleManager.spawnLockedVehicleForPlayerV2(ID, Caller.transform.position, Caller.transform.rotation, Caller);
            foreach (string Permission in PermissionsReward)
                R.Permissions.AddPlayerToGroup(Permission, Caller.channel.owner.ToUnturnedPlayer());
            Caller.skills.ServerModifyExperience(ExperienceReward);
        }
    }
}
