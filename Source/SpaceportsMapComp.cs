using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Spaceports
{
    public class SpaceportsMapComp : MapComponent
    {

		private IncidentDef incident = SpaceportsThingDefOf.Spaceports_VisitorShuttleArrival;

		private float IntervalTicks => 60000f * intervalDays;

		private float intervalDays => LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().visitorFrequencyDays;

		private float occurTick;

		public SpaceportsMapComp(Map map) : base(map) { 
            
        }

        public override void MapComponentTick()
        {
			base.MapComponentTick();

			if (LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().visitorFrequencyDays != intervalDays)
			{
				if (intervalDays > 0f)
				{
					occurTick = (float)Find.TickManager.TicksGame;
					occurTick += IntervalTicks;
				}
			}

			if (Find.AnyPlayerHomeMap == null)
			{
				return;
			}
			if (incident == null)
			{
				Log.Error("Trying to tick ScenPart_CreateIncident but the incident is null");
			}
			else if ((float)Find.TickManager.TicksGame >= occurTick)
			{
				IncidentParms parms = StorytellerUtility.DefaultParmsNow(incident.category, Find.Maps.Where((Map x) => x.IsPlayerHome).RandomElement());
				if (!incident.Worker.TryExecute(parms))
				{
					return;
				}
				else if (intervalDays > 0f)
				{
					occurTick += IntervalTicks;
				}
				else
				{
					return;
				}
			}
		}

    }
}
