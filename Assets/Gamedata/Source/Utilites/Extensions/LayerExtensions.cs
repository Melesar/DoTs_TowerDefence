namespace DoTs.Utilites
{
    public static class LayerExtensions
    {
        public static LayerMask SetLayer(this LayerMask mask, Layer layer)
        {
            var currentMask = mask.value;
            currentMask |= (int) layer;
            mask.value = currentMask;

            return mask;
        }

        public static LayerMask UnsetLayer(this LayerMask mask, Layer layer)
        {
            if (mask.HasLayer(layer))
            {
                mask.value -= (int) layer;
            }

            return mask;
        }

        public static bool HasLayer(this LayerMask mask, Layer layer)
        {
            return layer == Layer.Default || (mask.value & (int) layer) != 0;
        }

        public static Layer ToLayer(this LayerMask mask)
        {
            return (Layer) mask.value;
        }
    }
}