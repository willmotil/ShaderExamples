
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples //.ContentEffectClasses._3dEffectClasses
{
    // Wrap up our effect.
    public class SpecularLightEffectClass
    {
        public static Effect effect;

        public static string DirectoryForEffect = @"Content/Shaders3D";

        private static float totalStrength = 1f;
        private static float ambientStrength = .1f;
        private static float diffuseStrength = .6f;
        private static float specularStrength = .4f;

        public static void Load(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            Content.RootDirectory = DirectoryForEffect;
            effect = Content.Load<Effect>("SpecularLightEffect");
            Technique_Lighting_Phong();
            AmbientStrength = .1f;
            DiffuseStrength = .6f;
            SpecularStrength = .4f;
            World = Matrix.Identity;
            View = Matrix.Identity;
            Projection = Matrix.CreatePerspectiveFieldOfView(1, 1.33f, 1f, 10000f); // just something default;
            LightPosition = new Vector3(0, 0, 10000);
            LightColor = new Vector3(1f, 1f, 1f);
        }

        public static Effect GetEffect
        {
            get { return effect; }
        }

        public static void Technique_Lighting_Phong()
        {
            effect.CurrentTechnique = effect.Techniques["Lighting_Phong"];
        }
        public static void Technique_Lighting_Blinn()
        {
            effect.CurrentTechnique = effect.Techniques["Lighting_Blinn"];
        }
        public static void Technique_Lighting_Wills()
        {
            effect.CurrentTechnique = effect.Techniques["Lighting_Wills"];
        }

        public static Texture2D TextureDiffuse
        {
            set { effect.Parameters["TextureDiffuse"].SetValue(value); }
        }

        public static Texture2D TextureNormalMap
        {
            set { effect.Parameters["TextureNormalMap"].SetValue(value); }
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

        public static Vector3 CameraPosition
        {
            set { effect.Parameters["CameraPosition"].SetValue(value); }
        }

        public static Vector3 LightPosition
        {
            set { effect.Parameters["LightPosition"].SetValue(value); }
        }

        public static Vector3 LightColor
        {
            set { effect.Parameters["LightColor"].SetValue(value); }
        }

        public static float AmbientStrength
        {
            set 
            { 
                ambientStrength = value;
                totalStrength = ambientStrength + diffuseStrength + specularStrength;
                effect.Parameters["AmbientStrength"].SetValue(ambientStrength / totalStrength);
            }
        }
        public static float SpecularStrength
        {
            set 
            {
                diffuseStrength = value;
                totalStrength = ambientStrength + diffuseStrength + specularStrength;
                effect.Parameters["DiffuseStrength"].SetValue(diffuseStrength / totalStrength);
            }
        }
        public static float DiffuseStrength
        {
            set 
            {
                specularStrength = value;
                totalStrength = ambientStrength + diffuseStrength + specularStrength;
                effect.Parameters["SpecularStrength"].SetValue(specularStrength / totalStrength);
            }
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
