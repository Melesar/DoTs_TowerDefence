using UnityEngine;

namespace DoTs.Utilites
{
    public static class SpriteExtensions
    {
        public static Vector4 GetUvRect(this Sprite sprite)
        {
            var texture = sprite.texture;
            var spriteRect = sprite.rect;

            var xSize = spriteRect.size.x / texture.width;
            var ySize = spriteRect.size.y / texture.height;
            var xOffset = spriteRect.min.x / texture.width;
            var yOffset = spriteRect.min.y / texture.height;

            return new Vector4(xSize, ySize, xOffset, yOffset);
        }
    }
}