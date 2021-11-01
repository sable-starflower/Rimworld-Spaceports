using System.Collections.Generic;
using Verse;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System;
using RimWorld;

namespace Spaceports
{

    
    public class SpaceportsSettings : ModSettings
    {
        public bool regularVisitors;
        public bool visitorNotifications;
        public float visitorFrequencyDays;
        public bool allowLandingRough;
        public bool enableShuttleLimit;
        public int shuttleLimit;
        public string limitBuffer;
        //TODO - chance to use shuttle as Hospitality arrival mode (+ other Hospitality settings?)

        public override void ExposeData()
        {
            Scribe_Values.Look(ref regularVisitors, "regularVisitors", true);
            Scribe_Values.Look(ref visitorNotifications, "visitorNotifications", false);
            Scribe_Values.Look(ref visitorFrequencyDays, "visitorFrequencyDays", 1.0f);
            Scribe_Values.Look(ref allowLandingRough, "allowLandingRough", false);
            Scribe_Values.Look(ref enableShuttleLimit, "enableShuttleLimit", true);
            Scribe_Values.Look(ref shuttleLimit, "shuttleLimit", 5);
            base.ExposeData();
        }
    }

    public class SpaceportsMod : Mod
    {
        SpaceportsSettings settings;
        public SpaceportsMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<SpaceportsSettings>();
            Log.Message("[Spaceports] Okay, showtime!");
            Harmony har = new Harmony("Spaceports");
            har.PatchAll(Assembly.GetExecutingAssembly());
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Spaceports_Spaceports_RegularVisitorsToggle".Translate(), ref settings.regularVisitors, "Spaceports_RegularVisitorsTooltip".Translate());
            if (settings.regularVisitors) {
                listingStandard.CheckboxLabeled("Spaceports_VisitorNotificationsToggle".Translate(), ref settings.visitorNotifications, "Spaceports_VisitorNotificationsTooltip".Translate());
                listingStandard.Label("Spaceports_VisitorFreqSlider".Translate() + settings.visitorFrequencyDays);
                settings.visitorFrequencyDays = (float)Math.Round(listingStandard.Slider(settings.visitorFrequencyDays, 0.10f, 5f) * 10, MidpointRounding.ToEven) / 10;
            }
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled("Spaceports_AllowRoughLandingToggle".Translate(), ref settings.allowLandingRough, "Spaceports_AllowRoughLandingTooltip".Translate());
            listingStandard.CheckboxLabeled("Spaceports_EnableShuttleLimitToggle".Translate(), ref settings.enableShuttleLimit);
            if (settings.enableShuttleLimit) 
            {
                listingStandard.Label("Spaceports_ShuttleLimitLabel".Translate() + settings.shuttleLimit);
                settings.shuttleLimit = (int)Math.Round(listingStandard.Slider(settings.shuttleLimit, 1f, 20f));
            }
            listingStandard.End();
        }

        public override string SettingsCategory()
        {
            return "SpaceportsModName".Translate();
        }

    }
}