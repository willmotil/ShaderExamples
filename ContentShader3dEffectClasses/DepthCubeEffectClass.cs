﻿
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShaderExamples 
{
    // Wrap up our effect.
    public class DepthCubeEffectClass
    {
        public static Effect effect;

        public static string DirectoryForEffect = @"Content/Shaders3D";

        public static void Load(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            Content.RootDirectory = DirectoryForEffect;
            effect = Content.Load<Effect>("DepthCubeEffect");
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

        public static void Technique_Render_VisualizationDepthCube()
        {
            effect.CurrentTechnique = effect.Techniques["Render_VisualizationDepthCube"];
        }

        public static void Technique_Render_LightDepth()
        {
            effect.CurrentTechnique = effect.Techniques["Render_LightDepth"];
        }

        public static void Technique_Render_BasicUnalteredRenderTargetCubeMap()
        {
            effect.CurrentTechnique = effect.Techniques["Render_BasicUnalteredRenderTargetCubeMap"];
        }

        public static void Technique_Render_BasicCubeMapScene()
        {
            effect.CurrentTechnique = effect.Techniques["Render_BasicCubeMapScene"];
        }

        public static void Technique_Render_BasicSkyCubeMapScene()
        {
            effect.CurrentTechnique = effect.Techniques["Render_BasicSkyCubeMapScene"];
        }

        public static void Technique_Render_BasicScene()
        {
            effect.CurrentTechnique = effect.Techniques["Render_BasicScene"];
        }
        

        public static Texture2D TextureDiffuse
        {
            set { effect.Parameters["TextureDiffuse"].SetValue(value); }
        }
        public static TextureCube TextureCubeDiffuse
        {
            set { effect.Parameters["TextureCubeDiffuse"].SetValue(value); }
        }

        public static TextureCube TextureCubeEnviromental
        {
            set { effect.Parameters["TextureCubeEnviromental"].SetValue(value); }
        }

        public static bool UseFlips
        {
            set 
            { 
                if(value)
                    effect.Parameters["UseFlips"].SetValue(1f); 
                else
                    effect.Parameters["UseFlips"].SetValue(0f);
            }
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
