// CornellMirror.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Input;
using PhotonLab.Source.Bodies;
using PhotonLab.Source.Input;
using PhotonLab.Source.Materials;

namespace PhotonLab.Source.Scenes;

internal class CornellMirrorScene: Scene
{
    private static readonly Vector3 LookAtPos = new(0, 12.5f, 0);

    public CornellMirrorScene(GraphicsDevice graphicsDevice) : base(graphicsDevice)
    {   
        Camer3D.AddBehaviour(new MoveByMouse(5));
        
        CornellBox.MirrorBuild(graphicsDevice, this, 25, 1f);

        var model = BasicBodies.CreateSphere(graphicsDevice, 30, 30);
        model.Material = new PhongMaterial(Color.Yellow);
        model.ModelTransform = Matrix.CreateScale(4)
                               * Matrix.CreateTranslation(0, 5f, 0);
        AddBody(model);
    }

    public override void Update(double elapsedMilliseconds, InputHandler inputHandler)
    {
        Camer3D.Target = LookAtPos;
        base.Update(elapsedMilliseconds, inputHandler);
    }
}