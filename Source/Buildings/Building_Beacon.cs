using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Spaceports.Buildings
{
    class Building_Beacon : Building
    {
        private Utils.SpinOver RadarDish;
        private Utils.AnimateOver RimLights;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            RadarDish = new Utils.SpinOver(SpaceportsFrames.BeaconRadarDish, this, 3f, 3f, 1.5f, powerDependent: true);
            RimLights = new Utils.AnimateOver(SpaceportsFramesLists.BeaconRimFrames, 30, this, 3f, 3f);
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void PostMake()
        {
            RimLights = new Utils.AnimateOver(SpaceportsFramesLists.BeaconRimFrames, 30, this, 3f, 3f);
            RadarDish = new Utils.SpinOver(SpaceportsFrames.BeaconRadarDish, this, 3f, 3f, 1.5f, powerDependent: true);
            base.PostMake();
        }

        public override string GetInspectString()
        {
            string info = base.GetInspectString();
            if (this.Map.gameConditionManager.ConditionIsActive(SpaceportsDefOf.Spaceports_KesslerSyndrome))
            {
                info += "Spaceports_BeaconKessler".Translate();
            }
            else if (!this.GetComp<CompPowerTrader>().PowerOn)
            {
                info += "Spaceports_BeaconUnpowered".Translate();
            }
            else if (!Utils.AnyPoweredSpaceportPads(this.Map))
            {
                info += "Spaceports_BeaconNoPads".Translate();
            }
            else if (Utils.AtShuttleCapacity(this.Map))
            {
                info += "Spaceports_BeaconAtCap".Translate();
            }
            else if(!Utils.AnyValidSpaceportPad(this.Map, 0))
            {
                info += "Spaceports_BeaconPadsFull".Translate();
            }
            else
            {
                info += "Spaceports_BeaconOK".Translate();
            }
            return info;
        }
        public override void Draw()
        {
            base.Draw();
            RadarDish.FrameStep();
            if (this.GetComp<CompPowerTrader>().PowerOn)
            {
                RimLights.FrameStep();
            }
        }
    }
}
