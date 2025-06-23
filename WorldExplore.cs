﻿using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Expeditions144
{
    public class WorldExplore : ModSystem
    {
        internal static Expedition syncedDailyExpedition = null;
        private static int expeditionIndex = -1;
        internal static int dailyExpIndex { get { return expeditionIndex; } }
        
        public override void Load() //**** was "Initialize"
        {
            syncedDailyExpedition = null;
        }

        public override void PostUpdateEverything()
        {
            if(Main.dayTime && Main.time == 0.0)
            {
                List<Expedition> dailys = new List<Expedition>();
                foreach (ModExpedition me in Expeditions144.GetExpeditionsList())
                {
                    if (me.expedition.CheckDailyAssigned()) dailys.Add(me.expedition);
                }

                if (dailys.Count > 0)
                {
                    // Try for one that we didn't have last time
                    Expedition previousDaily = syncedDailyExpedition;
                    expeditionIndex = -1;
                    for (int i = 0; i < 100; i++)
                    {
                        expeditionIndex = Main.rand.Next(dailys.Count);
                        if (previousDaily != dailys[expeditionIndex]) break;
                    }

                    if (Expeditions144.DEBUG) Main.NewText("dailys = " + dailys.Count + ", picked " + expeditionIndex);
                    NetSyncDaily(dailys[expeditionIndex]);
                    if (Main.netMode == 2)
                    {
                        Expeditions144.SendNet_NewDaily(Mod);
                    }
                }
                else
                {
                    if(Main.netMode != 1) syncedDailyExpedition = null;
                }

            }
        }

        internal static void NetSyncDaily(Expedition expedition)
        {
            syncedDailyExpedition = expedition;
            syncedDailyExpedition.ResetProgress(true);
            if (Expeditions144.DEBUG)
            {
                if (syncedDailyExpedition != null)
                {
                    Expeditions144.DisplayUnlockedExpedition(expedition, "Daily Expedition: ");
                    Main.NewText("Daily EXP = " + syncedDailyExpedition.name);
                }
            }
        }

        public static bool IsCurrentDaily(Expedition expedition)
        {
            if (syncedDailyExpedition == null) return false;
            return Expedition.CompareExpeditions(expedition, syncedDailyExpedition);
        }


        #region SaveLoad overrides

        public override void SaveWorldData(TagCompound tag)
        {
            int dQuestID = 0;
            if (syncedDailyExpedition != null) dQuestID = Expedition.GetHashID(syncedDailyExpedition);
			tag.Add("dailyQuestID", dQuestID);
        }

        public override void LoadWorldData(TagCompound tag)
        {
            syncedDailyExpedition = Expeditions144.FindExpedition(tag.GetInt("dailyQuestID"));
        }
        #endregion
    }
}
