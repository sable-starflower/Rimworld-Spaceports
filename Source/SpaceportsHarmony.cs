using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Runtime.CompilerServices;
using Verse.AI.Group;
using Spaceports.LordToils;
using Verse.AI;

namespace Spaceports
{
    [StaticConstructorOnStartup]
    public static class SpaceportsHarmony
    {

        static SpaceportsHarmony() {
            DoPatches();
        }
        public static void DoPatches()
        {
            Harmony harmony = new Harmony("Spaceports_Plus_Hospitality");
            if (Verse.ModLister.HasActiveModWithName("Hospitality")) //conditional patch to Hospitality
            {
                Log.Message("[Spaceports] Hospitality FOUND, attempting to patch...");
                var mOriginal = AccessTools.Method("Hospitality.IncidentWorker_VisitorGroup:CreateLord");
                var mPostfix = typeof(SpaceportsHarmony).GetMethod("CreateLordPostfix");

                if (mOriginal != null)
                {
                    var hospPatch = new HarmonyMethod(mPostfix);
                    Log.Message("[Spaceports] Attempting to postfix Hospitality.IncidentWorker_VisitorGroup.CreateLord...");
                    harmony.Patch(mOriginal, postfix: hospPatch);
                }
            }
            else if(!Verse.ModLister.HasActiveModWithName("Hospitality"))
            {
                Log.Message("[Spaceports] Hospitality not found, patches bypassed.");
            }
        }

        [HarmonyPostfix] //Black magic fuckery w/ Lords
        public static void CreateLordPostfix(Faction faction, IntVec3 chillSpot, List<Pawn> pawns, Map map, bool showLetter, bool getUpsetWhenLost, int duration)
        {
            //Conditional check
            //IF rand in range 1-100 is less than or equal to configured chance
            //AND Hospitality integration is enabled
            //AND we are clear for landing
            if (Rand.RangeInclusive(1, 100) <= (LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().hospitalityChance * 100) && LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().hospitalityEnabled && Utils.CheckIfClearForLanding(map, 3) && faction.def.techLevel.ToString() != "Neolithic" && !map.gameConditionManager.ConditionIsActive(SpaceportsDefOf.Spaceports_KesslerSyndrome))
            {
                if (pawns != null)
                {
                    IntVec3 pad = Utils.FindValidSpaceportPad(Find.CurrentMap, faction, 3); //Find valid landing pad or touchdown spot
                    TransportShip shuttle = Utils.GenerateInboundShuttle(pawns, pad, 3); //Initialize shuttle

                    StateGraph graphExit = new LordJobs.LordJob_SpaceportDepart(shuttle.shipThing).CreateGraph(); //Intialize patched subgraph

                    Lord lord = pawns[0].GetLord(); //Get Lord
                    List<LordToil> lordToils = lord.Graph.lordToils.ToList(); //Get and copy Lord lordToils to list

                    LordToil_TryShuttleWoundedGuest lordToil_TakeWoundedGuest = new LordToil_TryShuttleWoundedGuest(shuttle.shipThing, LocomotionUrgency.Sprint, canDig: false, interruptCurrentJob: true); //Initialize patched guest rescue
                    lordToil_TakeWoundedGuest.lord = lord;
                    lordToils.Add(lordToil_TakeWoundedGuest);

                    LordToil patchedToilExit = lord.Graph.AttachSubgraph(graphExit).StartingToil; //Attach patched subgraph
                    LordToil patchedToilExit2 = graphExit.lordToils[1]; //Get second toil from subgraph
                    patchedToilExit.lord = lord; //Ensure lord is assigned correctly - stops "curLordToil lord is null (forgot to add toil to graph?)" bullshit
                    patchedToilExit2.lord = lord; //ditto
                    lordToils.Add(patchedToilExit); //Attach first patched toil
                    lordToils.Add(patchedToilExit2); //Attach second patched toil
                    lord.Graph.lordToils = lordToils; //Ensure lord graph is updated (might be redundant)


                    List<Transition> transitions = lord.Graph.transitions.ToList(); //Get and copy transitions to list
                    foreach (Transition transition in transitions)
                    {
                        foreach (Trigger trigger in transition.triggers) //foreach trigger in each transition
                        {
                            //determine which transition we're dealing with by referencing its triggers
                            //might replace with string matching since this is the equivalent of fucking triangulation
                            if (trigger is Trigger_PawnExperiencingDangerousTemperatures || trigger is Trigger_BecamePlayerEnemy || (transition.preActions.Any(x => x is TransitionAction_Message) && !(trigger is Trigger_WoundedGuestPresent)))
                            {
                                transition.target = patchedToilExit;
                            }
                            //Edge case to patch wounded guest trigger
                            else if (trigger is Trigger_WoundedGuestPresent)
                            {
                                transition.target = lordToil_TakeWoundedGuest;
                            }
                            //Remove all occurences of a custom Hospitality preaction that throws NRE (I think because it assumes it is interacting w/ a LordToil_Travel but gets a LordToil_Wait)
                            foreach(TransitionAction preAction in transition.preActions.ToList())
                            {
                                if (preAction.ToString().Equals("Hospitality.TransitionAction_EnsureHaveNearbyExitDestination"))
                                {
                                    transition.preActions.Remove(preAction);
                                }
                            }
                        }
                    }
                    lord.Graph.transitions = transitions; //Ensure lord graph is updated (might be redundant)
                }

                return;
            }

            else //Patched behavior not called
            {
                return;
            }

        }


        //Postfix to CompShuttle's Tick() that checks to see if any required pawns are despawned (e.g. left the map through alternate means) and removes them
        //from the shuttle's required list accordingly
        //This actually fixes a bug in the base game as well
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
