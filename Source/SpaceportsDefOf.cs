using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using static Spaceports.Utils;

namespace Spaceports
{
    [DefOf]
    public static class SpaceportsDefOf
    {
        public static ThingDef Spaceports_ShuttleLandingPad;
        public static ThingDef ShuttleA_Crashing;
        public static ThingDef Spaceports_Shrapnel;

        public static TransportShipDef Spaceports_ShuttleA;
        public static TransportShipDef Spaceports_ShuttleInert;
        public static TransportShipDef Spaceports_SurpriseShuttle;

        public static IncidentDef Spaceports_VisitorShuttleArrival;
        public static IncidentDef Spaceports_TraderShuttleArrival;
        public static IncidentDef Spaceports_MedevacReward;

        public static DutyDef Spaceports_TryShuttleWoundedGuest;
        public static JobDef Spaceports_Kidnap;
        public static GameConditionDef Spaceports_KesslerSyndrome;

        public static ThoughtDef Spaceports_PsychicCharge;
    }

    [StaticConstructorOnStartup]
    public static class SpaceportsMisc //Misc complex constants 
    {
        public static List<AccessControlState> AccessStates = new List<AccessControlState>();
        static SpaceportsMisc()
        {
            AccessStates.Add(new AccessControlState("Spaceports_None", -1));
            AccessStates.Add(new AccessControlState("Spaceports_AllTypes", 0));
            AccessStates.Add(new AccessControlState("Spaceports_JustVisitors", 1));
            AccessStates.Add(new AccessControlState("Spaceports_JustTraders", 2));
            if (Verse.ModLister.HasActiveModWithName("Hospitality"))
            {
                AccessStates.Add(new AccessControlState("Spaceports_JustGuests", 3));
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class SpaceportsFrames //Material constants used in animations/visual changes. Spun up at runtime.
    {
        public static readonly Material HoldingPatternGraphic = MaterialPool.MatFrom("Animations/HoldingPattern", ShaderDatabase.TransparentPostLight, Color.white);
        public static readonly Material BlockedPatternGraphic = MaterialPool.MatFrom("Animations/BlockedPattern", ShaderDatabase.TransparentPostLight, Color.white);
        public static readonly Material LandingPatternAlpha = MaterialPool.MatFrom("Animations/TouchdownLights/TouchdownLightsA", ShaderDatabase.TransparentPostLight, Color.white);
        public static readonly Material LandingPatternBeta = MaterialPool.MatFrom("Animations/TouchdownLights/TouchdownLightsB", ShaderDatabase.TransparentPostLight, Color.white);
        public static readonly Material LandingPatternGamma = MaterialPool.MatFrom("Animations/TouchdownLights/TouchdownLightsC", ShaderDatabase.TransparentPostLight, Color.white);
        public static readonly Material RimPatternOn = MaterialPool.MatFrom("Animations/RimLights/RimLights_On", ShaderDatabase.TransparentPostLight, Color.white);
        public static readonly Material RimPatternOff = MaterialPool.MatFrom("Animations/RimLights/RimLights_Off", ShaderDatabase.TransparentPostLight, Color.white);

        public static readonly Material ChillSpot_All = MaterialPool.MatFrom("Buildings/SpaceportChillSpot/ChillSpot_all");
        public static readonly Material ChillSpot_None = MaterialPool.MatFrom("Buildings/SpaceportChillSpot/ChillSpot_none");
        public static readonly Material ChillSpot_Visitors = MaterialPool.MatFrom("Buildings/SpaceportChillSpot/ChillSpot_visitors");
        public static readonly Material ChillSpot_Traders = MaterialPool.MatFrom("Buildings/SpaceportChillSpot/ChillSpot_traders");
        public static readonly Material ChillSpot_Guests = MaterialPool.MatFrom("Buildings/SpaceportChillSpot/ChillSpot_guests");

        public static readonly Material BeaconRadarDish = MaterialPool.MatFrom("Animations/Beacon/SpaceportBeacon_Dish");
        public static readonly Material BeaconLightsOn = MaterialPool.MatFrom("Animations/Beacon/SpaceportBeacon_LightsOn", ShaderDatabase.TransparentPostLight, Color.white);
        public static readonly Material BeaconLightsOff = MaterialPool.MatFrom("Animations/Beacon/SpaceportBeacon_LightsOff", ShaderDatabase.TransparentPostLight, Color.white);
    }

    [StaticConstructorOnStartup]
    public static class SpaceportsShuttleVariants //Compiles list of available shuttle variants at runtime.
    {
        public static List<TransportShipDef> AllShuttleVariants = new List<TransportShipDef>();
        static SpaceportsShuttleVariants()
        {
            AllShuttleVariants.Add(SpaceportsDefOf.Spaceports_ShuttleA);
        }
    }

    [StaticConstructorOnStartup]
    public static class SpaceportsFramesLists //Compiles Material Lists used in building animations at runtime.
    {
        public static List<Material> LandingPatternFrames = new List<Material>();
        public static List<Material> RimPatternFrames = new List<Material>();
        public static List<Material> BeaconRimFrames = new List<Material>();
        static SpaceportsFramesLists()
        {
            LandingPatternFrames.Add(SpaceportsFrames.LandingPatternAlpha);
            LandingPatternFrames.Add(SpaceportsFrames.LandingPatternBeta);
            LandingPatternFrames.Add(SpaceportsFrames.LandingPatternGamma);

            RimPatternFrames.Add(SpaceportsFrames.RimPatternOn);
            RimPatternFrames.Add(SpaceportsFrames.RimPatternOff);

            BeaconRimFrames.Add(SpaceportsFrames.BeaconLightsOn);
            BeaconRimFrames.Add(SpaceportsFrames.BeaconLightsOff);
        }
    }
}