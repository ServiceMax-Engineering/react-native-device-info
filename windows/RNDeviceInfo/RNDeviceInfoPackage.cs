using Microsoft.ReactNative.Managed;
using Microsoft.ReactNative;

namespace RNDeviceInfo
{
    public sealed class RNDeviceInfoPackage : IReactPackageProvider
    {
        public void CreatePackage(IReactPackageBuilder packageBuilder)
        {
            packageBuilder.AddAttributedModules();
        }
    }
}
