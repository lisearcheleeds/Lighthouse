namespace LighthouseExtends.Addressable
{
    public sealed class AssetRequest<T> where T : UnityEngine.Object
    {
        internal int Index { get; }

        internal AssetRequest(int index)
        {
            Index = index;
        }
    }
}
