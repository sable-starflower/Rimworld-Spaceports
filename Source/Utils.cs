using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
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

		public class AnimateOver
		{
			private List<Material> frames;
			private int currentFrame = 0;
			private int ticksPerFrame = 30;
			private int ticksPrev;
			private Thing thing;
			private float xSize;
			private float ySize;

			public AnimateOver(List<Material> frames, int ticksPerFrame, Thing thing, float xSize, float ySize) {
				this.frames = frames;
				this.ticksPerFrame = ticksPerFrame;
				this.thing = thing;
				this.xSize = xSize;
				this.ySize = ySize;
			}

			public void FrameStep() {
				int ticksCurrent = Find.TickManager.TicksGame;
				if (ticksCurrent >= this.ticksPrev + ticksPerFrame)
				{
					this.ticksPrev = ticksCurrent;
					currentFrame++;
				}
				if (currentFrame >= frames.Count)
				{
					currentFrame = 0;
				}
				DrawOverlayTex(currentFrame);
			}

			private void DrawOverlayTex(int currFrame)
			{
				Matrix4x4 matrix = default(Matrix4x4);
				Vector3 pos = thing.TrueCenter();
				Vector3 s = new Vector3(xSize, 1f, ySize); //x and z should correspond to the DrawSize values of the base building
				matrix.SetTRS(pos, thing.Rotation.AsQuat, s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, frames[currFrame], 0);
			}
		}

		public class DrawOver
		{
			private Material frame;
			private Thing thing;
			private float xSize;
			private float ySize;

			public DrawOver(Material frame, int ticksPerFrame, Thing thing, float xSize, float ySize)
			{
				this.frame = frame;
				this.thing = thing;
				this.xSize = xSize;
				this.ySize = ySize;
			}

			public void FrameStep()
			{
				DrawOverlayTex();
			}

			private void DrawOverlayTex()
			{
				Matrix4x4 matrix = default(Matrix4x4);
				Vector3 pos = thing.TrueCenter();
				Vector3 s = new Vector3(xSize, 1f, ySize); //x and z should correspond to the DrawSize values of the base building
				matrix.SetTRS(pos, thing.Rotation.AsQuat, s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, frame, 0);
			}
		}

		public class AccessControlState
        {
			private string label;
			private int value;

			public AccessControlState(String key, int value)
            {
				label = key.Translate();
				this.value = value;
            }

			public string GetLabel() {
				return label;
			}

			public int getValue() {
				return value;
			}
        }

		public static TransportShip GenerateInboundShuttle(List<Pawn> pawns, IncidentParms parms, IntVec3 padCell, int typeVal) {
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
			shuttle.ArriveAt(padCell, Find.CurrentMap.Parent);
			shuttle.AddJob(new ShipJob_Unload());
			shuttle.ShuttleComp.requiredPawns = pawns;
			ShipJob_WaitForever wait = new ShipJob_WaitForever();
			wait.leaveImmediatelyWhenSatisfied = true;
			wait.showGizmos = false;
			shuttle.AddJob(wait);
			return shuttle;
		}

		public static IntVec3 FindValidSpaceportPad(Map map, Faction faction, int typeVal) {
			foreach (Spaceports.Buildings.Building_ShuttlePad pad in map.listerBuildings.AllBuildingsColonistOfClass<Spaceports.Buildings.Building_ShuttlePad>())
			{
				if (!pad.IsAvailable())
				{
					continue;
				}
				if (pad.IsAvailable() && pad.CheckAccessGranted(typeVal)) {
					pad.NotifyIncoming();
					return pad.Position;
				}
			}
			return DropCellFinder.GetBestShuttleLandingSpot(Find.CurrentMap, faction);
		}

		public static IntVec3 GetBestChillspot(Map map, IntVec3 originCell, int accessVal) {
			Spaceports.Buildings.Building_ShuttleSpot closestValidSpot = null;
			foreach (Spaceports.Buildings.Building_ShuttleSpot spot in map.listerBuildings.AllBuildingsColonistOfClass<Spaceports.Buildings.Building_ShuttleSpot>())
			{
				if (closestValidSpot == null && spot.CheckAccessGranted(accessVal)) {
					closestValidSpot = spot;
				}
				else if (spot.Position.DistanceTo(originCell) < closestValidSpot.Position.DistanceTo(originCell) && spot.CheckAccessGranted(accessVal))
				{
					closestValidSpot = spot;
				}
				return closestValidSpot.Position;
			}
			IntVec3 fallbackChillspot = originCell;
			fallbackChillspot.z = fallbackChillspot.z - 2;
			return fallbackChillspot;
		}

		public static bool AnyValidSpaceportPads(Map map) {
			foreach (Building pad in map.listerBuildings.AllBuildingsColonistOfDef(SpaceportsDefOf.Spaceports_ShuttleLandingPad))
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
			if (LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().airspaceLockdown && GenHostility.AnyHostileActiveThreatToPlayer_NewTemp(map, true))
			{
				return false;
			}
			if (!LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().allowLandingRough && !Utils.AnyValidSpaceportPads(map))
			{
				return false;
			}
			if (AtShuttleCapacity(map)) {
				return false;
			}
			if (!map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole))
			{
				return false;
			}
			if (map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole && !b.GetComp<CompPowerTrader>().PowerOn))
			{
				return false;
			}
			return true;
		}

		public static bool CheckIfSpaceport(Map map) {
			if (!LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().allowLandingRough && !Utils.AnyValidSpaceportPads(map))
			{
				return false;
			}
			if (!map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole))
			{
				return false;
			}
			if (map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole && !b.GetComp<CompPowerTrader>().PowerOn))
			{
				return false;
			}
			return true;
		}

		public static bool AnyShuttlesOnMap(Map map)
		{
			foreach(Building b in map.listerBuildings.allBuildingsNonColonist)
			{
				Building val = b as Spaceports.Buildings.Building_Shuttle;
				if (val != null)
				{
					return true;
				}
			}
			return false;
		}

	}
}
