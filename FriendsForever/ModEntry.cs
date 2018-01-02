using System;
using System.Linq;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace IsaacS.FriendsForever {
    public class Config {
        /// <summary>Whether spouses should be prevented from having friendship decay.</summary>
        public bool AffectSpouses { get; set; } = false;
        /// <summary>Whether dates should be prevented from having friendship decay. Does nothing if married.</summary>
        public bool AffectDates { get; set; } = true;
        /// <summary>Whether everyone else should be prevented from having friendship decay. Effects everyone
        /// but the spouse if they are married.</summary>
        public bool AffectEveryoneElse { get; set; } = true;
    }

    public class ModEntry : Mod {
        Config config;

        /// <summary>Mod entry point. Reads the config and adds the listeners.</summary>
        /// <param name="helper">Helper object for various mod functions (such as loading config files).</param>
        public override void Entry(IModHelper helper) {
            config = helper.ReadConfig<Config>();
            TimeEvents.AfterDayStarted += this.StartDay;

            if (!(config.AffectDates || config.AffectDates || config.AffectEveryoneElse)) {
                this.Monitor.Log("This mod can be removed, all features currently disabled.", LogLevel.Warn);
            }
        }

        /// <summary>Start the day out by 'talking' to every NPC that we don't want friendship decay for.</summary>
        private void StartDay(object sender, EventArgs e) {
            var farmers = Game1.getAllFarmers();
            var npcs = Utility.getAllCharacters().Distinct();

            foreach (NPC character in npcs) {
                if (!character.isVillager())
                    continue;

                foreach (Farmer farmer in farmers) {
                    if (farmer.spouse == character.name && !config.AffectSpouses) {
                        continue;
                    //If the they are 'dating' and we're not to affect dates, skip them. The exception to this is if
                    //the farmer has a spouse, in which case we want to treat them like everyone else:
                    } else if (character.datingFarmer && !config.AffectDates && farmer.spouse == null) {
                        continue;
                    } else if (!config.AffectEveryoneElse) {
                        continue;
                    }

                    //Set the flag for having talked to that character, but don't add any points.
                    //The player can talk to the person themselves and still get the 20 points.
                    farmer.talkToFriend(character, 0);
                }
            }
        }
    }
}
