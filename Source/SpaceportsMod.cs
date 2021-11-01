using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace Spaceports
{
    public class SpaceportsSettings : ModSettings
    {
        /// <summary>
        /// The three settings our mod has.
        /// </summary>
        public bool visitorNotifications;
        public float visitorFrequencyDays;
        public bool allowLandingRough;

        /// <summary>
        /// The part that writes our settings to file. Note that saving is by ref.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref visitorNotifications, "visitorNotifications", false);
            Scribe_Values.Look(ref visitorFrequencyDays, "visitorFrequencyDays", 1.0f);
            Scribe_Values.Look(ref allowLandingRough, "allowLandingRough", false);
            base.ExposeData();
        }
    }

    public class SpaceportsMod : Mod
    {
        SpaceportsSettings settings;
        public SpaceportsMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<SpaceportsSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("Spaceports_VisitorHeader".Translate());
            listingStandard.CheckboxLabeled("Spaceports_VisitorNotificationsToggle".Translate(), ref settings.visitorNotifications, "Spaceports_VisitorNotificationsTooltip".Translate());
            listingStandard.Label("Spaceports_VisitorFreqSlider".Translate() + +settings.visitorFrequencyDays);
            settings.visitorFrequencyDays = listingStandard.Slider(settings.visitorFrequencyDays, 0.1f, 5f);
            listingStandard.GapLine();
            listingStandard.Label("Spaceports_TrafficControlHeader".Translate());
            listingStandard.CheckboxLabeled("Spaceports_AllowRoughLandingToggle".Translate(), ref settings.allowLandingRough, "Spaceports_AllowRoughLandingTooltip".Translate());
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "SpaceportsModName".Translate();
        }
    }
}