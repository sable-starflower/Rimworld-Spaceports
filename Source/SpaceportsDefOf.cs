using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using static Spaceports.Utils;

namespace Spaceports
{
    [DefOf]
    public static class SpaceportsDefOf
    {
        public static ThingDef Spaceports_ShuttleLandingPad;
        public static IncidentDef Spaceports_VisitorShuttleArrival;
        public static DutyDef Spaceports_TryShuttleWoundedGuest;
        public static JobDef Spaceports_Kidnap;
    }

    [StaticConstructorOnStartup]
    public static class SpaceportsShuttleVariants
    {
        public static List<ShuttleVariant> AllShuttleVariants = new List<ShuttleVariant>();
        public static ShuttleVariant RoyaltyShuttle = new ShuttleVariant(ThingDefOf.Shuttle, ThingDefOf.ShuttleIncoming, ThingDefOf.ShuttleLeaving);

        static SpaceportsShuttleVariants()
        {
            AllShuttleVariants.Add(RoyaltyShuttle);
        }
    }
}