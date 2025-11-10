// Scene.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Camera;
using MonoKit.Input;
using PhotonLab.Source.Input;
using PhotonLab.Source.Lights;
using PhotonLab.Source.Materials;
using PhotonLab.Source.Bodies;
using PhotonLab.Source.RayTracing;
using System.Collections.Generic;
using System.Linq;

namespace PhotonLab.Source
{
    internal class Scene
    {
        public readonly Camera3D Camer3D;
        public readonly List<MeshBody> Shapes = [];
        public readonly List<ILightSource> LightSources = [];
        private readonly List<MeshBody> LightShapes = [];

        public int FaceCount => Shapes.Sum(s => s.FaseCount);

        public Scene(GraphicsDevice graphicsDevice)
        {
            Camer3D = new(graphicsDevice);
            Camer3D.AddBehaviour(new MoveByMouse());
            Camer3D.AddBehaviour(new ZoomByMouse(1));

            float scale = 20f;

            // Floor
            var quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationX(-float.Pi / 2); // floor rotation
            quad.Material = new PhongMaterial(Color.Gray) { AmbientStrength = .1f, SpecularStrength = 0 };
            Shapes.Add(quad);

            // Ceiling
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationX(float.Pi / 2) *
                Matrix.CreateTranslation(0, scale, 0);
            quad.Material = new PhongMaterial(Color.Black) { AmbientStrength = .1f, SpecularStrength = 0 };
            Shapes.Add(quad);

            // Back wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateTranslation(0, scale/2, -scale/2);
            quad.Material = new PhongMaterial(Color.Yellow) { AmbientStrength = .1f, SpecularStrength = 0 };
            Shapes.Add(quad);

            // Front wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationX(float.Pi) *
                Matrix.CreateTranslation(0, scale/2, scale / 2);
            quad.Material = new PhongMaterial(Color.Red) { AmbientStrength = .1f, SpecularStrength = 0 };
            Shapes.Add(quad);

            // Left wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationY(float.Pi / 2) *
                Matrix.CreateTranslation(-scale/2, scale / 2, 0);
            quad.Material = new PhongMaterial(Color.Green) { AmbientStrength = .1f, SpecularStrength = 0 };
            Shapes.Add(quad);

            // Right wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationY(-float.Pi / 2) *
                Matrix.CreateTranslation(scale / 2, scale / 2, 0);
            quad.Material = new PhongMaterial(Color.Blue) { AmbientStrength = .1f, SpecularStrength = 0 };
            Shapes.Add(quad);

            var sphere = BasicBodies.CreateSphere(graphicsDevice, 20, 20);
            sphere.ModelTransform = Matrix.CreateScale(3) * Matrix.CreateTranslation(6, 3, 6);
            sphere.Material = new MirrorMaterial(Color.White, .75f);
            Shapes.Add(sphere);

            var cube = BasicBodies.CreateCube(graphicsDevice);
            cube.ModelTransform = Matrix.CreateScale(5) * Matrix.CreateTranslation(-6, 2, -6);
            cube.Material = new PhongMaterial(Color.White, NormalMode.Face) { AmbientStrength = .1f };
            Shapes.Add(cube);

            var tetrahedron = BasicBodies.CreateTetrahedron(graphicsDevice);
            tetrahedron.Material = new MirrorMaterial(Color.White, .75f, NormalMode.Face);
            tetrahedron.ModelTransform = Matrix.CreateScale(8) * Matrix.CreateRotationY(0) * Matrix.CreateTranslation(-5, 0, 5);
            Shapes.Add(tetrahedron);

            LightSources.Add(new LightSources.SpotLight(new Vector3(0, 18f, 0), new(0, -1, 0), 45, Color.LightYellow));
            foreach (var lightSource in LightSources)
            {
                var lightMesh = BasicBodies.CreateSphere(graphicsDevice, 4, 4);
                lightMesh.ModelTransform = Matrix.CreateScale(.1f) * Matrix.CreateTranslation(lightSource.Position);
                LightShapes.Add(lightMesh);
            }
        }

        public bool Intersect(RaySIMD ray, out HitInfo closestHit)
        {
            closestHit = new HitInfo();
            var hitFound = false;
            foreach (var shape in Shapes)
            {
                if (shape.Intersect(ray, out var hit)
                    && hit <= closestHit)
                {
                    closestHit = hit;
                    hitFound = true;
                }
            }
            return hitFound;
        }

        public void Update(double elapsedMilliseconds, InputHandler inputHandler)
        {
            Camer3D.Update(elapsedMilliseconds, inputHandler);
        }

        public void Draw(BasicEffect basicEffect, GraphicsDevice graphicsDevice)
        {
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullNone;

            basicEffect.World = Matrix.Identity;
            basicEffect.View = Camer3D.View;
            basicEffect.Projection = Camer3D.Projection;

            foreach (var shape in Shapes)
                shape.Draw(graphicsDevice, basicEffect);

            foreach (var shape in LightShapes)
                shape.Draw(graphicsDevice, basicEffect);
        }
    }
}
