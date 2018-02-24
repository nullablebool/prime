//using System.Security.AccessControl;
//using Prime.Common.Annotations;
//using Prime.Radiant.Components.IPFS.Messenging;

namespace Prime.Common
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUserContext
    {
        ApiKey GetApiKey(INetworkProvider networkProvider);
    }
}
