using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Spaceports.Buildings
{
    public class Building_ShuttlePad : Building
    {
        private int currentFrame = 0;
        private int currentFrameRim = 0;
        private int ticksPerFrame = 30;
        private int ticksPrev = 0;

        private static readonly Material HoldingPatternGraphic = MaterialPool.MatFrom("Animations/HoldingPattern", ShaderDatabase.TransparentPostLight, Color.white);
        private static readonly Material LandingPatternAlpha = MaterialPool.MatFrom("Animations/TouchdownLights/TouchdownLightsA", ShaderDatabase.TransparentPostLight, Color.white);
        private static readonly Material LandingPatternBeta = MaterialPool.MatFrom("Animations/TouchdownLights/TouchdownLightsB", ShaderDatabase.TransparentPostLight, Color.white);
        private static readonly Material LandingPatternGamma = MaterialPool.MatFrom("Animations/TouchdownLights/TouchdownLightsC", ShaderDatabase.TransparentPostLight, Color.white);
        private static readonly Material RimPatternOn = MaterialPool.MatFrom("Animations/RimLights/RimLights_On", ShaderDatabase.TransparentPostLight, Color.white);
        private static readonly Material RimPatternOff = MaterialPool.MatFrom("Animations/RimLights/RimLights_Off", ShaderDatabase.TransparentPostLight, Color.white);

        private List<Material> LandingPattern = new List<Material>();
        private List<Material> RimPattern = new List<Material>();

        public override void PostMake()
        {
            base.PostMake();
            LandingPattern.Add(LandingPatternAlpha);
            LandingPattern.Add(LandingPatternBeta);
            LandingPattern.Add(LandingPatternGamma);
            RimPattern.Add(RimPatternOn);
            RimPattern.Add(RimPatternOff);
        }

        public override void Draw()
        {
            int ticksCurrent = Find.TickManager.TicksGame;
            if (ticksCurrent >= ticksPrev + ticksPerFrame)
            {
                ticksPrev = ticksCurrent;
                currentFrame++;
                currentFrameRim++;
            }
            if (currentFrame >= 3)
            {
                currentFrame = 0;
            }
            if (currentFrameRim >= 2) 
            { 
                currentFrameRim = 0;
            }
            DrawOverlayTex(LandingPattern[currentFrame]);
            DrawOverlayTex(RimPattern[currentFrameRim]);
            base.Draw();
        }

        private void DrawOverlayTex(Material mat) {
            Matrix4x4 matrix = default(Matrix4x4);
            Vector3 pos = this.TrueCenter();
            Vector3 s = new Vector3(7f, 1f, 5f); //x and z should correspond to the DrawSize values of the base building
            matrix.SetTRS(pos, this.Rotation.AsQuat, s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
        }
    }
}
