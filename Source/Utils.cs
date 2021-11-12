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

		public class SpinOver
        {
			private Material spinner;
			private Thing thing;
			private float xSize;
			private float ySize;
			private float curRotationInt;
			private float degreesPerTick;
			private int direction; //0 for cw, 1 for ccw
			private bool powerDependent;

			public SpinOver(Material spinner, Thing thing, float xSize, float ySize, float degreesPerTick, int direction = 0, bool powerDependent=false)
            {
				this.spinner = spinner;
				this.thing = thing;
				this.xSize = xSize;
				this.ySize = ySize;
				this.degreesPerTick = degreesPerTick;
				this.direction = direction;
				this.powerDependent = powerDependent;
            }

			public float CurRotation
			{
				get
				{
					return curRotationInt;
				}
				set
				{
					if(powerDependent && !thing.TryGetComp<CompPowerTrader>().PowerOn)
                    {
						return;
                    }

					curRotationInt = value;
					if (curRotationInt > 360f)
					{
						curRotationInt -= 360f;
					}
					if (curRotationInt < 0f)
					{
						curRotationInt += 360f;
					}
				}
			}

			public void FrameStep()
			{
				if(!Find.TickManager.Paused)
                {
					if (direction == 0)
					{
						CurRotation += degreesPerTick * Find.TickManager.TickRateMultiplier;
					}
					else
					{
						CurRotation -= degreesPerTick * Find.TickManager.TickRateMultiplier;
					}
				}

				DrawOverlayTex();
			}

			private void DrawOverlayTex()
			{
				Matrix4x4 matrix = default(Matrix4x4);
				Vector3 pos = thing.TrueCenter();
				Vector3 s = new Vector3(xSize, 1f, ySize); //x and z should correspond to the DrawSize values of the base building
				matrix.SetTRS(pos + Altitudes.AltIncVect, CurRotation.ToQuat(), s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, spinner, 0);
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
				matrix.SetTRS(pos + Altitudes.AltIncVect, thing.Rotation.AsQuat, s);
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
				matrix.SetTRS(pos + Altitudes.AltIncVect, thing.Rotation.AsQuat, s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, frame, 0);
			}
		}

		//simplification class for restricting chillspots and pads
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

		//Generates an inbound shuttle of random appearance and sets up its job queue
		public static TransportShip GenerateInboundShuttle(List<Pawn> pawns, IntVec3 padCell, int typeVal) {
			TransportShipDef shuttleDef = new TransportShipDef();
			ShuttleVariant variantToUse = SpaceportsShuttleVariants.AllShuttleVariants.RandomElement();
			shuttleDef.shipThing = variantToUse.shipThing;
			shuttleDef.arrivingSkyfaller = variantToUse.arrivingSkyfaller;
			shuttleDef.leavingSkyfaller = variantToUse.leavingSkyfaller; //TODO patch this, game doesn't preserve dynamic defs between reloads
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

		//Returns the cell of an open spaceport pad
		//Also notifies that pad of an incoming shuttle, reserving it and playing the animation
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

		public static bool AnyValidSpaceportPad(Map map, int typeVal)
        {
			foreach (Spaceports.Buildings.Building_ShuttlePad pad in map.listerBuildings.AllBuildingsColonistOfClass<Spaceports.Buildings.Building_ShuttlePad>())
			{
				if (pad.IsAvailable() && pad.CheckAccessGranted(typeVal) && Utils.CheckIfSpaceport(map))
				{
					return true;
				}
			}
			return false;
		}

		//Seeks out closest valid chillspot for shuttle visitors
		//If none found, defaults to tile that is z-2 below shuttle tile
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

		//Checks if there are any pads on a map
		public static bool AnySpaceportPads(Map map) {
			foreach (Spaceports.Buildings.Building_ShuttlePad pad in map.listerBuildings.AllBuildingsColonistOfDef(SpaceportsDefOf.Spaceports_ShuttleLandingPad))
			{
				return true;
			}
			return false;
		}

		//Check if a given map is at the player-set shuttle limit or higher
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

		//Checks if a shuttle event can fire
		//Qualifying conditions:
		//A) Airspace lockdown must not be in effect
		//B) There must be a valid spaceport pad present OR rough landing must be enabled
		//C) The map must not be at the shuttle limit
		//D) There must be a powered comms console
		public static bool CheckIfClearForLanding(Map map, int typeVal) {
			if (LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().airspaceLockdown && GenHostility.AnyHostileActiveThreatToPlayer_NewTemp(map, true))
			{
				return false;
			}
			if (!Utils.AnyValidSpaceportPad(map, 0))
			{
				return false;
			}
			if (AtShuttleCapacity(map)) {
				return false;
			}

			int count = 0;
			foreach (Spaceports.Buildings.Building_ShuttlePad pad in map.listerBuildings.AllBuildingsColonistOfDef(SpaceportsDefOf.Spaceports_ShuttleLandingPad))
			{
				if (pad.CheckAccessGranted(typeVal)) {
					count++;
				}
			}
			if (count == 0) {
				return false;
			}

			if (!map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.defName == "Spaceports_Beacon"))
			{
				return false;
			}
			if (map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.defName == "Spaceports_Beacon" && !b.GetComp<CompPowerTrader>().PowerOn))
			{
				return false;
			}
			return true;
		}

		//Checks if a given map is considered to be a "spaceport"
		//Qualifying conditions are A) a comms console (does not need to be powered unless you want shuttles to actually land)
		//and B) either the presence of a valid shuttlepad or rough landing being enabled
		public static bool CheckIfSpaceport(Map map) {
			if (!Utils.AnySpaceportPads(map))
			{
				return false;
			}
			if (!map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.defName == "Spaceports_Beacon"))
			{
				return false;
			}
			return true;
		}

		//Checks if ANY shuttles are on a given map
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
