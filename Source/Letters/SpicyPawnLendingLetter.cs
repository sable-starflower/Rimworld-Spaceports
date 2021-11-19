using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Spaceports.Letters
{
    public class LoanedPawnTracker : Utils.Tracker
    {
        private Pawn LoanedPawn;
        private Map map;
        private bool WasInjured;
        private int TicksPassed = 0;
        private int ReturnAfterTicks;
        private List<Thing> rewards;

        public LoanedPawnTracker(Pawn pawn, bool injured, Map map, List<Thing> rewards)
        {
            LoanedPawn = pawn;
            WasInjured = injured;
            this.map = map;
            this.rewards = rewards;
            ReturnAfterTicks = Rand.RangeInclusive(GenDate.TicksPerDay * 1, GenDate.TicksPerDay * 1);
            Log.Message("[Spaceports] Lent pawn will return in " + ReturnAfterTicks / 60000 + "d");
        }

        public override bool Check()
        {
            if(TicksPassed < ReturnAfterTicks)
            {
                TicksPassed++;
                return false;
            }
            else if(TicksPassed >= ReturnAfterTicks && Utils.AnyValidSpaceportPad(map, 0))
            {
                if (WasInjured)
                {
                    //injure pawn + send message
                }
                else
                {
                    Messages.Message("Spaceports_SpicyPawnLendingReturnSafe".Translate(LoanedPawn.NameShortColored), MessageTypeDefOf.PositiveEvent);
                }
                IntVec3 pad = Utils.FindValidSpaceportPad(map, null, 0);
                List<Pawn> pawn = new List<Pawn>();
                pawn.Add(LoanedPawn);
                Utils.GenerateInboundShuttle(pawn, pad, dropAndGo: true, items: rewards);
                return true;
            }
            return false;
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref LoanedPawn, "LoanedPawn");
            Scribe_References.Look(ref map, "map");
            Scribe_Values.Look(ref WasInjured, "WasInjured");
            Scribe_Values.Look(ref TicksPassed, "TicksPassed");
            Scribe_Values.Look(ref ReturnAfterTicks, "ReturnAfterTicks");
            Scribe_Collections.Look(ref rewards, "rewards", LookMode.Deep);
        }
    }

    internal class SpicyPawnLendingLetter : ChoiceLetter
    {
        public Faction faction;
        public Map map;
        public Pawn RequestedPawn;
        public List<Thing> rewards;
        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (base.ArchivedOnly)
                {
                    yield return base.Option_Close;
                    yield break;
                }
                DiaOption diaAccept = new DiaOption("Spaceports_SpicyPawnLendingAccept".Translate());
                DiaOption diaDeny = new DiaOption("Spaceports_SpicyPawnLendingDeny".Translate());
                diaAccept.action = delegate
                {
                    IntVec3 pad = Utils.FindValidSpaceportPad(Find.CurrentMap, faction, 0);
                    TransportShip shuttle = Utils.GenerateInboundShuttle(null, pad); //TODO add rewards
                    List<Pawn> pawn = new List<Pawn>();
                    pawn.Add(RequestedPawn);
                    shuttle.ShuttleComp.requiredPawns = pawn;
                    Messages.Message("Spaceports_PickupInbound".Translate(RequestedPawn.Name + ""), new LookTargets(shuttle.shipThing), MessageTypeDefOf.NeutralEvent);
                    map.GetComponent<SpaceportsMapComp>().LoadTracker(new LoanedPawnTracker(RequestedPawn, false, map, rewards));
                    Find.LetterStack.RemoveLetter(this);
                };
                diaAccept.resolveTree = true;
                diaAccept.disabledReason = "Spaceports_ShuttleDisabled".Translate();
                if (!Utils.AnyValidSpaceportPad(Find.CurrentMap, 0))
                {
                    diaAccept.disabled = true;
                }
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
            Scribe_References.Look(ref RequestedPawn, "RequestedPawn");
            Scribe_Collections.Look(ref rewards, "rewards", LookMode.Deep);
            base.ExposeData();
        }
    }
}
