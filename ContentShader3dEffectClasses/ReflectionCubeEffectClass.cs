
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples 
{
    // Wrap up our effect.
    public class ReflectionCubeEffectClass
    {
        public static Effect effect;

        public static string DirectoryForEffect = @"Content/Shaders3D";

        private static float ambientStrength = .1f;
        private static float diffuseStrength = .6f;
        private static float specularStrength = .4f;

        public static void Load(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            Content.RootDirectory = DirectoryForEffect;
            effect = Content.Load<Effect>("ReflectionCubeEffect");
            Technique_Render_PhongWithNormMapEnviromentalLight();
            AmbientStrength = .3f;
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

        public static void Technique_Render_PhongWithNormMapEnviromentalLight()
        {
            effect.CurrentTechnique = effect.Techniques["Render_PhongWithNormMapEnviromentalLight"];
        }

        public static void Technique_Render_PhongWithNormMap()
        {
            effect.CurrentTechnique = effect.Techniques["Render_PhongWithNormMap"];
        }

        public static void Technique_Render_PhongWithEnviromentalMap()
        {
            effect.CurrentTechnique = effect.Techniques["Render_PhongWithEnviromentalMap"];
        }

        public static void Technique_Render_Cube()
        {
            effect.CurrentTechnique = effect.Techniques["Render_Cube"];
        }

        public static void Technique_Render_Skybox()
        {
            effect.CurrentTechnique = effect.Techniques["Render_Skybox"];
        }

        public static void Technique_Render_CubeWithEnviromentalLight()
        {
            effect.CurrentTechnique = effect.Techniques["Render_CubeWithEnviromentalLight"];
        }

        public static void Technique_Render_VisualizationDepthCube()
        {
            effect.CurrentTechnique = effect.Techniques["Render_VisualizationDepthCube"];
        }

        public static void Technique_Render_LightDepth()
        {
            effect.CurrentTechnique = effect.Techniques["Render_LightDepth"];
        }

        public static TextureCube TextureCubeDiffuse
        {
            set { effect.Parameters["TextureCubeDiffuse"].SetValue(value); }
        }

        public static TextureCube TextureCubeEnviromental
        {
            set { effect.Parameters["TextureCubeEnviromental"].SetValue(value); }
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
                effect.Parameters["AmbientStrength"].SetValue(ambientStrength);
            }
        }
        public static float DiffuseStrength
        {
            set
            {
                diffuseStrength = value;
                effect.Parameters["DiffuseStrength"].SetValue(diffuseStrength);
            }
        }
        public static float SpecularStrength
        {
            set
            {
                specularStrength = value;
                effect.Parameters["SpecularStrength"].SetValue(specularStrength);
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
