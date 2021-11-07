using System.Collections.Generic;
using RimWorld;
using Verse;


namespace Spaceports
{
    public class Alert_AirspaceLockdown : Alert_Critical
    {

        private static string ColoniesUnderLockdown {
            get {
                string output = "Spaceports_AirspaceLockdownDesc".Translate();
                List<Map> maps = Find.Maps;
                if (maps != null)
                {
                    foreach (Map map in maps)
                    {
                        if (map.IsPlayerHome && GenHostility.AnyHostileActiveThreatToPlayer_NewTemp(map, true))
                        {
                            output = output + "- " + map.info.parent.Label + "\n";
                        }
                    }
                }
                output = output.Substring(output.Length - 2);
                return output;
            }
        }
        
        public Alert_AirspaceLockdown()
        {
            defaultLabel = "Spaceports_AirspaceLockdown".Translate();
        }

        public override TaggedString GetExplanation()
        {
            return ColoniesUnderLockdown;
        }

        public override AlertReport GetReport()
        {
            if (!LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().airspaceLockdown) {
                return false;
            }
            if (Find.AnyPlayerHomeMap == null)
            {
                return false;
            }
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                if (maps[i].IsPlayerHome && GenHostility.AnyHostileActiveThreatToPlayer_NewTemp(maps[i], true) && Utils.CheckIfSpaceport(maps[i]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
