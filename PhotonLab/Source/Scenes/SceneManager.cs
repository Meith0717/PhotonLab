// SceneManager.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Input;

namespace PhotonLab.Source.Scenes
{
    internal class SceneManager(GraphicsDevice graphicsDevice)
    {
        private readonly BasicEffect _basicEffect = new(graphicsDevice);
        private readonly Dictionary<string, Scene> _scenesDict = [];
        private readonly List<Scene> _scenes = [];
        private int _currentSceneIndex;
        private Scene _currentScene;

        public Scene CurrentScene => _currentScene;

        public void AddScene(string name, Scene scene, bool overwrite = false)
        {
            if (overwrite)
                _scenesDict[name] = scene;
            else if (!_scenesDict.TryGetValue(name, out _))
            {
                _scenes.Add(scene);
                _scenesDict.Add(name, scene);
                _currentScene = scene;
            }
            else
                throw new InvalidOperationException("The Scene already exists!");

            scene.Initialize();
            _currentScene ??= scene;
        }

        public void Set(string name)
        {
            if (_scenesDict.TryGetValue(name, out var scene))
                _currentScene = scene;
            else
                throw new InvalidOperationException("The scene does not exist!");
        }

        public void NextScene()
        {
            var scenesLength = _scenes.Count;
            _currentSceneIndex++;
            _currentSceneIndex %= scenesLength;
            _currentScene = _scenes[_currentSceneIndex];
        }

        public void Update(double elapsedMilliseconds, InputHandler inputHandler)
        {
            _currentScene?.Update(elapsedMilliseconds, inputHandler);
        }

        public void Draw()
        {
            _currentScene?.Draw(_basicEffect);
        }

        public override string ToString()
        {
            var str = string.Empty;

            for (var i = 0; i < _scenesDict.Count; i++)
            {
                var kvp = _scenesDict.ElementAt(i);
                str += $"{i}: {kvp.Key}\n";
            }

            return str;
        }
    }
}
