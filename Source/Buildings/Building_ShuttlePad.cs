using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Spaceports.Buildings
{
    public class Building_ShuttlePad : Building
    {
        private bool ShuttleInbound = false;

        private int AccessState = 0; //-1 for none, 0 for all, 1 for visitors, 2 for traders, 3 for hospitality guests

        private Utils.AnimateOver landingPatternAnimation;
        private Utils.AnimateOver rimLightAnimation;
        private Utils.DrawOver holdingPattern;
        private Utils.DrawOver blockedPattern;

        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            if (IsUnroofed() == false) 
            {
                text += "Pad blocked by roofing.";
            }
            return text;
        }

        public override void PostMake()
        {
            landingPatternAnimation = new Utils.AnimateOver(SpaceportsFramesLists.LandingPatternFrames, 30, this, 7f, 5f);
            rimLightAnimation = new Utils.AnimateOver(SpaceportsFramesLists.RimPatternFrames, 30, this, 7f, 5f);
            holdingPattern = new Utils.DrawOver(SpaceportsFrames.HoldingPatternGraphic, 30, this, 7f, 5f);
            blockedPattern = new Utils.DrawOver(SpaceportsFrames.BlockedPatternGraphic, 30, this, 7f, 5f);
            base.PostMake();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref AccessState, "accessState", 0);
        }

        public override void Draw()
        {
            if (LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().padAnimationsGlobal)
            {
                if (ShuttleInbound && LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().landingAnimations)
                {
                    landingPatternAnimation.FrameStep();
                }
                if (LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().rimLightsAnimations)
                {
                    rimLightAnimation.FrameStep();
                }
            }
            if (IsShuttleOnPad()) 
            {
                holdingPattern.FrameStep();
            }
            if (!IsShuttleOnPad() && !IsUnroofed())
            {
                blockedPattern.FrameStep();
            }
            if (AccessState == -1) {
                blockedPattern.FrameStep();
            }
            base.Draw();
        }

        public override void Tick()
        {
            if (IsShuttleOnPad()) {
                ShuttleInbound = false;
            }
            base.Tick();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Command_Action()
            {

                defaultLabel = "AccessControlButton".Translate(),
                defaultDesc = "AccessControlDesc".Translate(),
                //icon = getMealIcon(),
                order = -100,
                action = delegate ()
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();

                    foreach (Utils.AccessControlState state in SpaceportsMisc.AccessStates)
                    {
                        string label = state.GetLabel();
                        FloatMenuOption option = new FloatMenuOption(label, delegate ()
                        {
                            SetAccessState(state.getValue());
                        });
                        options.Add(option);
                    }

                    if (options.Count > 0)
                    {
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                }
            };

        }

        public void NotifyIncoming() 
        {
            ShuttleInbound = true;
        }

        public bool IsAvailable() 
        {
            if (!IsUnroofed() || IsShuttleOnPad()) {
                return false;
            }
            if (CheckAirspaceLockdown()) {
                return false;
            }
            return true;
        }

        private bool IsUnroofed() 
        {
            foreach (IntVec3 cell in this.OccupiedRect().Cells) {
                if (cell.Roofed(this.Map)) {
                    return false;
                }
            }
            return true;
        }

        private bool IsShuttleOnPad()
        {
            if (this.Position.GetFirstThingWithComp<CompShuttle>(this.Map) != null) {
                return true;
            }
            return false;
        }

        private void SetAccessState(int val) {
            AccessState = val;
        }

        public bool CheckAccessGranted(int val) {
            if (AccessState == -1) {
                return false;
            }
            if (AccessState == 0 || val == 0)
            {
                return true;
            }
            else return AccessState == val;
        }

        private bool CheckAirspaceLockdown() {
            if (!LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().airspaceLockdown) {
                return false;
            }
            return GenHostility.AnyHostileActiveThreatToPlayer_NewTemp(this.Map, true);
        }

    }
}
