using System;

namespace ShaderExamples
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {

            // 2D shader examples.
            //using (var game = new Game1_GreyScale()) game.Run();
            //using (var game = new Game1_FadeByDistance()) game.Run();
            //using (var game = new Game1_TextureWrapping()) game.Run();
            //using (var game = new Game1_Blur()) game.Run();
            //using (var game = new Game1_RadialBlur()) game.Run();
            //using (var game = new Game1_ShockWaveRipple()) game.Run();
            //using (var game = new Game1_MaskAndBlend()) game.Run();
            //using (var game = new Game1_MaskBlendScroll()) game.Run();
            //using (var game = new Game1_Refraction()) game.Run();
            //using (var game = new Game1_RefractionDirectional()) game.Run();
            using (var game = new Game1_BloomGlow()) game.Run();


            // conceptual examples.
            //using (var game = new Game1_GammaCorrection()) game.Run();
            //using (var game = new Game1_DotAndCrossProduct()) game.Run();
            //using (var game = new Game1_Matrices()) game.Run();



            // tests or imcomplete.
            //using (var game = new Game1_XXXXXXX()) game.Run();
            //using (var game = new Game1_TestingCanidateEffects()) game.Run();

            //using (var game = new Game1_Mesh_TestsAndStuff()) game.Run();

        }
    }
}
