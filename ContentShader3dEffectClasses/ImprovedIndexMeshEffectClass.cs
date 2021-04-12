using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace ShaderExamples  //.ContentEffectClasses._3dEffectClasses
{

    
    /// <summary>
    /// Here we wrap up the improved mesh effect class so we can make simple calls to set the shader up.
    /// In this version stuff is static however later well change that so specific instances can store there own data.
    /// </summary>
    public class ImprovedIndexMeshEffectClass
    {
        public static Effect effect;

        public static void Load(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            Content.RootDirectory = @"Content/Shaders3D";
            effect = Content.Load<Effect>("ImprovedIndexMeshEffect");
            effect.CurrentTechnique = effect.Techniques["IndexedMeshDraw"];
            World = Matrix.Identity;
            View = Matrix.Identity;
            Projection = Matrix.CreatePerspectiveFieldOfView(1, 1.33f, 1f, 10000f); // just something default;
        }
        public static Effect GetEffect
        {
            get { return effect; }
        }
        public static string Technique
        {
            set { effect.CurrentTechnique = effect.Techniques[value]; }
        }
        public static Texture2D SpriteTexture//(Texture2D value)
        {
            set { effect.Parameters["SpriteTexture"].SetValue(value); }
        }
        public static Matrix World
        {
            set { effect.Parameters["World"].SetValue(value); }
        }
        public static Matrix View
        {
            set { effect.Parameters["View"].SetValue(value); }
        }
        public static Matrix Projection
        {
            set { effect.Parameters["Projection"].SetValue(value); }
        }

        public static void InfoForCreateMethods()
        {
            Console.WriteLine($"\n effect.Name: \n   {effect.Name} ");
            Console.WriteLine($"\n effect.Parameters: \n ");
            var pparams = effect.Parameters;
            foreach (var p in pparams)
            {
                Console.WriteLine($"   p.Name: {p.Name}  ");
            }
            Console.WriteLine($"\n effect.Techniques: \n ");
            var tparams = effect.Techniques;
            foreach (var t in tparams)
            {
                Console.WriteLine($"   t.Name: {t.Name}  ");
            }
        }
    }

}
