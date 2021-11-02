using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Runtime.CompilerServices;

namespace Spaceports
{

    public static class SpaceportsHarmony
    {
        //TODO - patches for Hospitality that hijack visitor arrival methods

        //These two are sneaky patches to make ShuttleComps work w/o Royalty
        //Ethically dubious but I think it's justifiable considering the code is so simple (I'd just end up remaking it near-identically otherwise)
        //and we'll be using custom shuttle assets
        [HarmonyPatch(typeof(ThingComp), "PostSpawnSetup")]
        class Patch
        {
            [HarmonyReversePatch]
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void PostSpawnSetup(ThingComp instance, bool respawningAfterLoad)
            {
                Patch.PostSpawnSetup(instance, respawningAfterLoad);
            }
        }

       
        [HarmonyPatch(typeof(CompShuttle), nameof(CompShuttle.PostSpawnSetup))]
        public static class Harmony_CompShuttle_PostSpawnSetup
        {
            static bool Prefix(bool respawningAfterLoad)
            {
                var comp = new CompShuttle();
                Patch.PostSpawnSetup(comp, respawningAfterLoad);
                return false;
            }
        }
    }
}
