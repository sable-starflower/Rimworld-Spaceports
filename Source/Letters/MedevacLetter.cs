using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Spaceports.Letters
{
    public class MedevacTracker : Utils.Tracker
    {
		private Pawn hurtPawn;
		private Thing shuttle;

		public MedevacTracker(Pawn pawn, Thing shuttle)
        {
			hurtPawn = pawn;
			this.shuttle = shuttle;
        }

        public override bool Check()
        {
			if (!hurtPawn.Downed && hurtPawn.Spawned)
			{
				List<Pawn> list = new List<Pawn>();
				list.Add(hurtPawn);
				LordJob lordJob = new LordJobs.LordJob_SpaceportDepart(shuttle);
				LordMaker.MakeNewLord(hurtPawn.Faction, lordJob, hurtPawn.Map, list);

				if(Rand.RangeInclusive(1, 100) <= 50)
                {
					IncidentQueue stQueue = Find.Storyteller.incidentQueue;
					IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(SpaceportsDefOf.Spaceports_MedevacReward.category, hurtPawn.Map);
					var qi = new QueuedIncident(new FiringIncident(SpaceportsDefOf.Spaceports_MedevacReward, null, incidentParms), (int)(Find.TickManager.TicksGame + GenDate.TicksPerDay * 15));
					stQueue.Add(qi);
				}

				return true;
			}
			else if (hurtPawn.Dead)
            {
				return true;
            }
			return false;
		}
    }

	internal class MedevacLetter : ChoiceLetter
    {
		public Faction faction;
		public Map map;
		public override IEnumerable<DiaOption> Choices
		{
			get
			{
				if (base.ArchivedOnly)
				{
					yield return base.Option_Close;
					yield break;
				}
				DiaOption diaAccept = new DiaOption("Spaceports_ShuttleMedevacAccept".Translate());
				DiaOption diaDeny = new DiaOption("Spacepots_ShuttleMedevacDeny".Translate());
				diaAccept.action = delegate
				{
					IntVec3 pad = Utils.FindValidSpaceportPad(Find.CurrentMap, faction, 0);
					Pawn pawn = PawnGenerator.GeneratePawn(faction.RandomPawnKind(), faction);
					HealthUtility.DamageUntilDowned(pawn);
					HealthUtility.DamageLegsUntilIncapableOfMoving(pawn);
					HealthUtility.GiveInjuriesOperationFailureRidiculous(pawn);
					List<Pawn> list = new List<Pawn>();
					list.Add(pawn);
					TransportShip shuttle = Utils.GenerateInboundShuttle(list, pad, 0);
					map.GetComponent<SpaceportsMapComp>().LoadTracker(new MedevacTracker(pawn, shuttle.shipThing));
					Find.LetterStack.RemoveLetter(this);
				};
				diaAccept.resolveTree = true;
				diaDeny.action = delegate
				{
					Find.LetterStack.RemoveLetter(this);
				};
				diaDeny.resolveTree = true;
				yield return diaAccept;
				yield return diaDeny;
				yield return base.Option_Postpone;
			}
		}
        public override void ExposeData()
        {
			Scribe_References.Look(ref faction, "faction");
			Scribe_References.Look(ref map, "map");
			base.ExposeData();
        }
    }
}
