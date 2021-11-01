using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Spaceports
{
    public class Utils
    {
		public class ShuttleVariant
		{
			public ThingDef shipThing;
			public ThingDef arrivingSkyfaller;
			public ThingDef leavingSkyfaller;

			public ShuttleVariant(ThingDef ship, ThingDef arriver, ThingDef leaver)
			{
				shipThing = ship;
				arrivingSkyfaller = arriver;
				leavingSkyfaller = leaver;
			}

			public List<ThingDef> GetShuttle()
			{
				List<ThingDef> shuttle = new List<ThingDef>();
				shuttle.Add(shipThing);
				shuttle.Add(arrivingSkyfaller);
				shuttle.Add(leavingSkyfaller);
				return shuttle;
			}
		}

		public static TransportShip GenerateInboundShuttle(List<Pawn> pawns, IncidentParms parms) {
			TransportShipDef shuttleDef = new TransportShipDef();
			ShuttleVariant variantToUse = SpaceportsShuttleVariants.AllShuttleVariants.RandomElement();
			shuttleDef.shipThing = variantToUse.shipThing;
			shuttleDef.arrivingSkyfaller = variantToUse.arrivingSkyfaller;
			shuttleDef.leavingSkyfaller = variantToUse.leavingSkyfaller;
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
			foreach (Building pad in map.listerBuildings.AllBuildingsColonistOfDef(SpaceportsDefOf.Spaceports_ShuttleLandingSpot)) 
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

		public static bool AnyValidSpaceportPads(Map map) {
			foreach (Building pad in map.listerBuildings.AllBuildingsColonistOfDef(SpaceportsDefOf.Spaceports_ShuttleLandingSpot))
			{
				if (!pad.Position.Roofed(pad.Map) && pad.Position.Standable(map))
				{
					return true;
				}
			}
			return false;
		}

		public static bool AtShuttleCapacity(Map map) {
			if (!LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().enableShuttleLimit) 
			{ 
				return false;
			}
			if (map.listerBuildings.AllBuildingsNonColonistOfDef(ThingDefOf.Shuttle).Count() >= LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().shuttleLimit) 
			{
				return true;
			}
			return false;
		}

		public static bool CheckIfClearForLanding(Map map) {
			if (!LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().allowLandingRough && !Utils.AnyValidSpaceportPads(map))
			{
				return false;
			}
			if (AtShuttleCapacity(map)) {
				return false;
			}
			if (!map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole))
			{
				return true; //remember to correct these to false
			}
			if (map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole && !b.GetComp<CompPowerTrader>().PowerOn))
			{
				return true;
			}
			return true;
		}

    }
}
