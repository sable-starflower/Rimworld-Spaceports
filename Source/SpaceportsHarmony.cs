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

        //Postfix to CompShuttle's Tick() that checks to see if any required pawns are despawned (e.g. left the map through alternate means) and removes them
        //from the shuttle's required list accordingly
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
