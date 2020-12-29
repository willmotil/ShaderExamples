using System;

namespace ShaderExamples
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            
            //using (var game = new Game1_GreyScale()) game.Run();
            using (var game = new Game1_FadeByDistance()) game.Run();
            //using (var game = new Game1_Blur()) game.Run();
            //using (var game = new Game1_MaskAndBlend()) game.Run();
            //using (var game = new Game1_MaskBlendScroll()) game.Run();
        }
    }
}
