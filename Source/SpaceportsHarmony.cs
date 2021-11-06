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

        [HarmonyPatch(typeof(CompShuttle), nameof(CompShuttle.CompTick))]
        private static class Harmony_CompShuttle_CompTick
        {
            static void Postfix(List<Pawn> ___requiredPawns, TransportShip ___shipParent)
            {
                List<Pawn> pawnsToMurk = new List<Pawn>();
                foreach (Pawn pawn in ___requiredPawns) {
                    if (!pawn.Spawned && !___shipParent.TransporterComp.innerContainer.Contains(pawn)) {
                        pawnsToMurk.Add(pawn);
                    }
                }
                foreach (Pawn pawn in pawnsToMurk)
                {
                    ___requiredPawns.Remove(pawn);
                }
            }
        }
    }
}
