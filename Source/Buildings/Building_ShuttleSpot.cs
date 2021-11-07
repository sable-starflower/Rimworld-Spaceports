using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Spaceports.Buildings
{
    class Building_ShuttleSpot : Building
    {
        private int AccessState = 0;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref AccessState, "accessState", 0);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Command_Action()
            {

                defaultLabel = "AccessControlButton".Translate(),
                defaultDesc = "AccessControlDesc".Translate(),
                //icon = getMealIcon(),
                order = -100,
                action = delegate ()
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();

                    foreach (Utils.AccessControlState state in SpaceportsMisc.AccessStates)
                    {
                        string label = state.GetLabel();
                        FloatMenuOption option = new FloatMenuOption(label, delegate ()
                        {
                            SetAccessState(state.getValue());
                        });
                        options.Add(option);
                    }

                    if (options.Count > 0)
                    {
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                }
            };

        }

        private void SetAccessState(int val)
        {
            AccessState = val;
        }

        public bool CheckAccessGranted(int val)
        {
            if (AccessState == -1)
            {
                return false;
            }
            if (AccessState == 0 || val == 0)
            {
                return true;
            }
            else return AccessState == val;
        }
    }
}
