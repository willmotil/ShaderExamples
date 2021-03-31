﻿using System;

namespace ShaderExamples
{
    public static class Program
    {

        [STAThread]
        static void Main()
        { 

            // conceptual examples.
            //using (var game = new Game1_GammaCorrection()) game.Run();
            //using (var game = new Game1_DotAndCrossProduct()) game.Run();
            //using (var game = new Game1_Matrices()) game.Run();



            // 2D shader examples.
            //using (var game = new Game1_GreyScale()) game.Run();
            //using (var game = new Game1_BarFillColorReplace()) game.Run();
            //using (var game = new Game1_FadeByDistance()) game.Run();
            //using (var game = new Game1_TextureWrapping()) game.Run();
            //using (var game = new Game1_Blur()) game.Run();
            //using (var game = new Game1_RadialBlur()) game.Run();
            //using (var game = new Game1_ShockWaveRipple()) game.Run();
            //using (var game = new Game1_MaskAndBlend()) game.Run();
            //using (var game = new Game1_MaskBlendScroll()) game.Run();
            //using (var game = new Game1_Refraction()) game.Run();
            //using (var game = new Game1_RefractionDirectional()) game.Run();
            //using (var game = new Game1_BloomGlow()) game.Run();
            //using (var game = new Game1_ShadowText_RenderTarget()) game.Run();



            //3D basic shader examples
            //using (var game = new Game1_TriangleDirectlyToTheGpu()) game.Run();
            //using (var game = new Game1_QuadWithMatrices()) game.Run();
            //using (var game = new Game1_ManipulatingTheMatrices()) game.Run();
            //using (var game = new Game1_ViewPerspectiveProjection()) game.Run();
            //using (var game = new Game1_IndexedMesh()) game.Run();
            //using (var game = new Game1_ImprovedIndexedMesh()) game.Run();

            // 3D Shader examples
            using (var game = new Game1_DiffuseLighting()) game.Run();


            // tests or imcomplete.
            //using (var game = new Game1_XXXXXXX()) game.Run();
            //using (var game = new Game1_TestingCanidateEffects()) game.Run();
            // gonna have to make full orientation matrix waypoint system or at least a set of up vectors or a gravity system.
            //using (var game = new Game1_Mesh_TestsAndStuff()) game.Run();

        }
    }
}
