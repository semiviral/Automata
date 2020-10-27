using System.Collections.Generic;

namespace Automata.Engine.Rendering.OpenGL.Textures
{
    public class TextureRegistry : Singleton<TextureRegistry>
    {
        private readonly Dictionary<string, Texture> _Textures;

        public TextureRegistry()
        {
            AssignSingletonInstance(this);

            _Textures = new Dictionary<string, Texture>();
        }

        public void AddTexture(string textureName, Texture texture) => _Textures.Add(textureName, texture);
        public void RemoveTexture(string textureName) => _Textures.Remove(textureName);
        public Texture GetTexture(string textureName) => _Textures[textureName];
        public TTextureCast? GetTexture<TTextureCast>(string textureName) where TTextureCast : Texture => _Textures[textureName] as TTextureCast;
    }
}
