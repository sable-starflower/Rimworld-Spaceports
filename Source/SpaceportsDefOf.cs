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
        public static ThingDef Spaceports_TestShuttle;
        public static IncidentDef Spaceports_VisitorShuttleArrival;
        public static IncidentDef Spaceports_TraderShuttleArrival;
        public static DutyDef Spaceports_TryShuttleWoundedGuest;
        public static JobDef Spaceports_Kidnap;
    }

    [StaticConstructorOnStartup]
    public static class SpaceportsMisc
    {
        public static List<AccessControlState> AccessStates = new List<AccessControlState>();
        static SpaceportsMisc()
        {
            AccessStates.Add(new AccessControlState("Spaceports_None", -1));
            AccessStates.Add(new AccessControlState("Spaceports_AllTypes", 0));
            AccessStates.Add(new AccessControlState("Spaceports_JustVisitors", 1));
            AccessStates.Add(new AccessControlState("Spaceports_JustTraders", 2));
            //if (Verse.ModLister.HasActiveModWithName("Hospitality"))
            //{
                //AccessStates.Add(new AccessControlState("Spaceports_JustGuests", 3));
            //}
        }
    }

    public static class SpaceportsFrames
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

    }

    [StaticConstructorOnStartup]
    public static class SpaceportsShuttleVariants
    {
        public static List<ShuttleVariant> AllShuttleVariants = new List<ShuttleVariant>();
        public static ShuttleVariant RoyaltyShuttle = new ShuttleVariant(ThingDefOf.Shuttle, ThingDefOf.ShuttleIncoming, ThingDefOf.ShuttleLeaving);
        public static ShuttleVariant TestShuttle = new ShuttleVariant(SpaceportsDefOf.Spaceports_TestShuttle, ThingDefOf.ShuttleIncoming, ThingDefOf.ShuttleLeaving);

        static SpaceportsShuttleVariants()
        {
            AllShuttleVariants.Add(TestShuttle);
        }
    }

    [StaticConstructorOnStartup]
    public static class SpaceportsFramesLists
    {
        public static List<Material> LandingPatternFrames = new List<Material>();
        public static List<Material> RimPatternFrames = new List<Material>();
        static SpaceportsFramesLists()
        {
            LandingPatternFrames.Add(SpaceportsFrames.LandingPatternAlpha);
            LandingPatternFrames.Add(SpaceportsFrames.LandingPatternBeta);
            LandingPatternFrames.Add(SpaceportsFrames.LandingPatternGamma);

            RimPatternFrames.Add(SpaceportsFrames.RimPatternOn);
            RimPatternFrames.Add(SpaceportsFrames.RimPatternOff);

        }
    }
}