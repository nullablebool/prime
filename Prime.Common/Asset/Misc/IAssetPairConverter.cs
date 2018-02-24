namespace Prime.Common
{
    public interface IAssetPairConverter
    {
        string Seperator { get; }

        string ToLocalPair(string remoteCode);

        string ToRemotePair(AssetPair localAsset);
    }
}