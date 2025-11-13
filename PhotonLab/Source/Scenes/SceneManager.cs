// SceneManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Graphics;
using MonoKit.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhotonLab.Source.Scenes
{
    internal class SceneManager(GraphicsDevice graphicsDevice)
    {
        private readonly BasicEffect _basicEffect = new(graphicsDevice);
        private readonly Dictionary<string, Scene> _scenes = new();
        private Scene _currentScene;

        public Scene CurrentScene => _currentScene;

        public void AddScene(string name, Scene scene, bool overwrite = false)
        {
            if (overwrite)
                _scenes[name] = scene;
            else if (!_scenes.TryGetValue(name, out _))
                _scenes.Add(name, scene);
            else
                throw new InvalidOperationException("The Scene already exists!");

            _currentScene ??= scene;
        }

        public void RemoveScene(string name)
        {
            _scenes.Remove(name);
        }

        public void Set(string name)
        {
            if (_scenes.TryGetValue(name, out var scene))
                _currentScene = scene;
            else
                throw new InvalidOperationException("The scene does not exist!");
        }

        public void Update(double elapsedMilliseconds, InputHandler inputHandler)
        {
            _currentScene?.Update(elapsedMilliseconds, inputHandler);
        }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            _currentScene?.Draw(_basicEffect, graphicsDevice);
        }

        public override string ToString()
        {
            var str = string.Empty;

            for (var i = 0; i < _scenes.Count; i++)
            {
                var kvp = _scenes.ElementAt(i);
                str += $"{i}: {kvp.Key}\n";
            }

            return str;
        }
    }
}
