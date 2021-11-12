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
