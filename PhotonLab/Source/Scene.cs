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
        private readonly MeshBody _mashBody;
        private float _bodyRotation;

        public int FaceCount => Shapes.Sum(s => s.FaseCount);

        public Scene(GraphicsDevice graphicsDevice)
        {
            Camer3D = new(new(0, 10, -30), new(-1, 0, 0),  graphicsDevice);
            Camer3D.AddBehaviour(new MoveByMouse());
            Camer3D.AddBehaviour(new ZoomByMouse(1));

            CornellBox.Build(graphicsDevice, Shapes, LightSources, 20);

            var model = BasicBodies.CreateCube(graphicsDevice, 1, 1.5f, 1);
            model.Material = new PhongMaterial(Color.White, NormalMode.Face);
            model.ModelTransform = Matrix.CreateScale(7)
               * Matrix.CreateTranslation(-5, 5, 5);
            Shapes.Add(model);

            model = BasicBodies.CreateSphere(graphicsDevice, 30, 30);
            model.Material = new MirrorMaterial(Color.White, .75f, NormalMode.Interpolated);
            model.ModelTransform = Matrix.CreateScale(4)
                * Matrix.CreateTranslation(5, 5, 5);
            Shapes.Add(model);

            _mashBody = BasicBodies.CreateTetrahedron(graphicsDevice);
            _mashBody.Material = new PhongMaterial(Color.LightYellow, NormalMode.Face);

            foreach (var lightSource in LightSources)
            {
                var lightMesh = BasicBodies.CreateSphere(graphicsDevice, 4, 4);
                lightMesh.ModelTransform = Matrix.CreateScale(.1f) * Matrix.CreateTranslation(lightSource.Position);
                LightShapes.Add(lightMesh);
            }
        }

        public bool Intersect(in RaySIMD ray, out HitInfo closestHit)
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
            _bodyRotation += .05f;
            _mashBody.ModelTransform = Matrix.CreateScale(7) * Matrix.CreateRotationY(_bodyRotation) * Matrix.CreateTranslation(0, 0, -5);
            Camer3D.Update(elapsedMilliseconds, inputHandler);
        }

        public void Draw(BasicEffect basicEffect, GraphicsDevice graphicsDevice)
        {
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullClockwise;

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
