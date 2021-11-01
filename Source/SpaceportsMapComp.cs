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

		private float IntervalTicks => 60000f * 0.25f;

		private float intervalDays = 0.25f;

		private bool repeat = true;

		private float occurTick;

		private bool isFinished;

		public SpaceportsMapComp(Map map) : base(map) { 
            
        }

        public override void MapComponentTick()
        {
			base.MapComponentTick();
			if (Find.AnyPlayerHomeMap == null || isFinished)
			{
				return;
			}
			if (incident == null)
			{
				Log.Error("Trying to tick ScenPart_CreateIncident but the incident is null");
				isFinished = true;
			}
			else if ((float)Find.TickManager.TicksGame >= occurTick)
			{
				IncidentParms parms = StorytellerUtility.DefaultParmsNow(incident.category, Find.Maps.Where((Map x) => x.IsPlayerHome).RandomElement());
				if (!incident.Worker.TryExecute(parms))
				{
					isFinished = true;
				}
				else if (repeat && intervalDays > 0f)
				{
					occurTick += IntervalTicks;
				}
				else
				{
					isFinished = true;
				}
			}
		}

    }
}
