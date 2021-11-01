using RimWorld;
using System.Collections.Generic;
using Verse;
using static Spaceports.Utils;

namespace Spaceports
{
    [DefOf]
    public static class SpaceportsThingDefOf
    {
        public static ThingDef Spaceports_ShuttleLandingSpot;
        public static IncidentDef Spaceports_VisitorShuttleArrival;
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