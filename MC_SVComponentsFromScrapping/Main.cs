
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MC_SVComponentsFromScrapping
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string pluginGuid = "mc.starvalor.componentsfromscrapping";
        public const string pluginName = "SV Components From Scrapping";
        public const string pluginVersion = "1.0.0";

        private const int idFineComponent = 44;
        private const int idSupComponent = 45;
        private const int idArdComponent = 46;
        private const int idSkill = 35; // Nano Boosters
        private const int baseChance = 10;
        private const int skillBuff = 10;
        private const int legAdditionalSup = 40;
        private const int legAdditionalArd = 75;

        private static ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource(pluginName);

        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Main));
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.ScrapItem))]
        [HarmonyPrefix]
        private static void ScrapItem_Pre(Inventory __instance)
        {
            int selectedItem = (int)typeof(Inventory).GetField("selectedItem", AccessTools.all).GetValue(__instance);
            if (selectedItem == -1)
                return;
            
            CargoSystem cs = ((CargoSystem)typeof(Inventory).GetField("cs", AccessTools.all).GetValue(__instance));
            CargoItem item = cs.cargo[selectedItem];
            if (item.itemType > 2)
                return;

            int chance = baseChance;            
            if (PChar.Char.SK[35] == 1)
            {
                log.LogInfo(SkillDB.Skills[idSkill].skillName + " buff");
                chance += skillBuff;
            }
            chance += PChar.TechLevel() / 2;

            log.LogInfo("Chance: " + chance);

            int roll = Random.Range(0, 100);
            log.LogInfo("Roll: " + roll);
            if (roll > chance)
                return;

            int qnt = Mathf.Clamp(Enumerable.Range(roll, chance - roll + 1).Select(z => Mathf.FloorToInt(chance / z)).Distinct().Count(), 1, 10);

            switch (item.rarity)
            {
                case (int)ItemRarity.Uncommon_2:
                    Store(cs, idFineComponent, qnt);
                    break;
                case (int)ItemRarity.Rare_3:
                    Store(cs, idSupComponent, qnt);
                    break;
                case (int)ItemRarity.Epic_4:
                    Store(cs, idArdComponent, qnt);
                    break;
                case (int)ItemRarity.Legendary_5:
                    Store(cs, idArdComponent, qnt);
                    roll = Random.Range(0, 100);
                    if (roll <= 50)
                        Store(cs, idFineComponent, qnt);
                    else if (roll <= 85)
                        Store(cs, idSupComponent, qnt);
                    else
                        Store(cs, idArdComponent, qnt);
                    break;
            }
        }

        private static void Store(CargoSystem cs, int id, int qnt)
        {
            cs.StoreItem(3, id, 1, qnt, 0, -1, -1);
            string str = ((int)qnt > 1) ? ("(" + (int)qnt + ") ") : "";
            SideInfo.AddMsg(Lang.Get(6, 18, str + ItemDB.GetItemNameModified(id, 2)));
        }
    }
}

