﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Spaceports
{
    public class Utils
    {
		//Abstract class.
		//Classes derived from Tracker are used to track the status of objects generated by an Incident once they leave the aegis
		//of their parent IncidentWorker.

		//The object(s) that need to be tracked should be stored in private fields, which should in turn be initialized by a constructor.
		//The Check() method is where any conditionals relevant to the object(s) should be implemented, plus any logic that follows from
		//a conditional being met. Check() should only return true if a conditional or an edge case (such as a tick limit) is met;
		//otherwise, it should return false.

		//The desired Tracker derivative should be instantiated in the IncidentWorker and passed into the appropriate instance of
		//SpaceportsMapComp using the LoadTracker() method. 
		public abstract class Tracker : IExposable
        {
			public abstract bool Check();

			public abstract void ExposeData();
        }

		//abstraction class for restricting chillspots and pads
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
		//Required arguments: List of passenger pawns, target cell
		//Optional arguments: a specific TransportShipDef to use, whether or not the shuttle should ever leave
		public static TransportShip GenerateInboundShuttle(List<Pawn> pawns, IntVec3 padCell, List<Thing> items = null, TransportShipDef forcedType = null, bool canLeave = true, bool dropAndGo = false) {
			TransportShip shuttle = TransportShipMaker.MakeTransportShip(SpaceportsShuttleVariants.AllShuttleVariants.RandomElement(), null);
			if(forcedType != null)
            {
				shuttle = TransportShipMaker.MakeTransportShip(forcedType, null);
			}
			List<Thing> checkTargets = new List<Thing>();
			if(pawns != null)
            {
				foreach (Pawn p in pawns)
				{
					shuttle.TransporterComp.innerContainer.TryAdd(p.SplitOff(1));
					checkTargets.Add(p);
				}
			}
			if(items != null)
            {
				shuttle.TransporterComp.innerContainer.TryAddRangeOrTransfer(items);
            }
			shuttle.ArriveAt(padCell, Find.CurrentMap.Parent);
			ShipJob_Unload unload = new ShipJob_Unload();
			unload.loadID = Find.UniqueIDsManager.GetNextShipJobID();
			shuttle.AddJob(unload);
            if (canLeave && !dropAndGo)
            {
				if (pawns != null) { shuttle.ShuttleComp.requiredPawns = pawns; }
				ShipJob_WaitForever wait = new ShipJob_WaitForever();
				wait.loadID = Find.UniqueIDsManager.GetNextShipJobID();
				wait.leaveImmediatelyWhenSatisfied = true;
				wait.showGizmos = false;
				wait.sendAwayIfAllDespawned = checkTargets;
				shuttle.AddJob(wait);
			}
			else if (dropAndGo)
            {
				ShipJob_FlyAway leave = new ShipJob_FlyAway();
				leave.loadID = Find.UniqueIDsManager.GetNextShipJobID();
				shuttle.AddJob(leave);
			}
			else
            {
				ShipJob_WaitForever wait = new ShipJob_WaitForever();
				wait.loadID = Find.UniqueIDsManager.GetNextShipJobID();
				wait.showGizmos = false;
				shuttle.AddJob(wait);
				Buildings.Building_Shuttle b = shuttle.shipThing as Buildings.Building_Shuttle;
				if(b != null)
                {
					b.disabled = true;
                }
			}
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
				if (closestValidSpot == null && spot.CheckAccessGranted(accessVal) && spot != null) {
					closestValidSpot = spot;
				}
				else if(closestValidSpot != null)
                {
					if (spot.Position.DistanceTo(originCell) < closestValidSpot.Position.DistanceTo(originCell) && spot.CheckAccessGranted(accessVal) && spot != null)
					{
						closestValidSpot = spot;
					}
				}
			}

			if(closestValidSpot != null)
			{
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

		public static bool AnyPoweredSpaceportPads(Map map)
		{
			foreach (Spaceports.Buildings.Building_ShuttlePad pad in map.listerBuildings.AllBuildingsColonistOfDef(SpaceportsDefOf.Spaceports_ShuttleLandingPad))
			{
                if (pad.GetComp<CompPowerTrader>().PowerOn)
                {
					return true;
				}
			}
			return false;
		}

		//Check if a given map is at the player-set shuttle limit or higher
		public static bool AtShuttleCapacity(Map map) {
			if (!LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().enableShuttleLimit) 
			{ 
				return false;
			}

			int count = 0;
			foreach (Building b in map.listerBuildings.allBuildingsNonColonist)
			{
				Building val = b as Spaceports.Buildings.Building_Shuttle;
				if (val != null)
				{
					count++;
				}
			}

			if (count >= LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().shuttleLimit) 
			{
				return true;
			}

			return false;
		}

		//Checks if a shuttle event can fire
		//Qualifying conditions:
		//1) Kessler Syndrome must not be in effect
		//2) Airspace lockdown must not be in effect
		//3) There must be a valid spaceport pad present
		//4) The map must not be at the shuttle limit
		//5) There must be a powered beacon
		public static bool CheckIfClearForLanding(Map map, int typeVal) {
            if (map.gameConditionManager.ConditionIsActive(SpaceportsDefOf.Spaceports_KesslerSyndrome))
            {
				return false;
            }
			if (LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().airspaceLockdown && GenHostility.AnyHostileActiveThreatToPlayer_NewTemp(map, true))
			{
				return false;
			}
            if (map.GetComponent<SpaceportsMapComp>().ForcedLockdown)
            {
				return false;
            }
			if (!Utils.AnyValidSpaceportPad(map, typeVal))
			{
				return false;
			}
			if (AtShuttleCapacity(map)) 
			{
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

		public static void StripPawn(Pawn p)
		{
			if (p.inventory != null)
			{
				p.inventory.DestroyAll();
			}
			if (p.apparel != null)
			{
				p.apparel.DestroyAll();
			}
			if (p.equipment != null)
			{
				p.equipment.DestroyAllEquipment();
			}
		}

	}
}
