using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Spaceports
{
    public class Utils
    {
        public static TransportShip GenerateInboundShuttle(List<Pawn> pawns, IncidentParms parms) {
			TransportShipDef shuttleDef = new TransportShipDef();
			shuttleDef.shipThing = ThingDefOf.Shuttle;
			shuttleDef.arrivingSkyfaller = ThingDefOf.ShuttleIncoming;
			shuttleDef.leavingSkyfaller = ThingDefOf.ShuttleLeaving;
			TransportShip shuttle = TransportShipMaker.MakeTransportShip(shuttleDef, null);
			foreach (Pawn p in pawns)
			{
				shuttle.TransporterComp.innerContainer.TryAdd(p.SplitOff(1));
			}
			shuttle.ArriveAt(FindValidSpaceportPad(Find.CurrentMap, parms.faction), Find.CurrentMap.Parent);
			shuttle.AddJob(new ShipJob_Unload());
			shuttle.ShuttleComp.requiredPawns = pawns;
			ShipJob_WaitForever wait = new ShipJob_WaitForever();
			wait.leaveImmediatelyWhenSatisfied = true;
			wait.showGizmos = false;
			shuttle.AddJob(wait);
			return shuttle;
		}

		public static IntVec3 FindValidSpaceportPad(Map map, Faction faction) {
			foreach (Building pad in map.listerBuildings.AllBuildingsColonistOfDef(SpaceportsThingDefOf.Spaceports_ShuttleLandingSpot)) 
			{
				if (pad.Position.Roofed(pad.Map))
				{
					continue;
				}
				if (pad.Position.Standable(map)) {
					return pad.Position;
				}
			}
			return DropCellFinder.GetBestShuttleLandingSpot(Find.CurrentMap, faction);
		}
    }
}
