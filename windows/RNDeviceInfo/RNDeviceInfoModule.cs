using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Windows.ApplicationModel;
using Windows.Devices.Power;
using Windows.System;
using Windows.Security.Credentials.UI;
using Windows.Networking;
using Windows.Networking.Connectivity;
using System.Linq;
using Microsoft.ReactNative.Managed;
using System.Diagnostics;
using Windows.Globalization.DateTimeFormatting;

namespace RNDeviceInfo
{
    [ReactModule]
    internal sealed class RNDeviceInfo
    {
        private bool IsEmulator(string model)
        {
            Regex rgx = new Regex("(?i:virtual)");
            return rgx.IsMatch(model);
        }

        private bool IsTablet(string os)
        {
            Regex rgx = new Regex("(?i:windowsphone)");
            return !rgx.IsMatch(os);
        }

        private bool is24Hour()
        {
            return DateTimeFormatInfo.CurrentInfo.ShortTimePattern.Contains("H");
        }

        [ReactMethod]
         public async void isPinOrFingerprintSet(ReactCallback<bool> actionCallback)
         {
             try
             {
                 var ucvAvailability = await UserConsentVerifier.CheckAvailabilityAsync();

                 actionCallback(ucvAvailability == UserConsentVerifierAvailability.Available);
             }
             catch (Exception ex)
             {
                 actionCallback(false);
             }
         }

        [ReactMethod]
        public void getIpAddress(ReactPromise<string> promise)
        {
            var hostNameType = HostNameType.Ipv4;
            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp?.NetworkAdapter == null)
            {
                ReactError error = new ReactError();
                error.Message = "Network adapter not found.";
                promise.Reject(error);
            }
            else
            {
                var hostname = NetworkInformation.GetHostNames()
                    .FirstOrDefault(
                        hn =>
                            hn.Type == hostNameType &&
                            hn.IPInformation?.NetworkAdapter != null &&
                            hn.IPInformation.NetworkAdapter.NetworkAdapterId == icp.NetworkAdapter.NetworkAdapterId);
                promise.Resolve(hostname?.CanonicalName);
            }
        }

        [ReactMethod]
         public void getBatteryLevel(IReactPromise<double> promise)
         {
             // Create aggregate battery object
             var aggBattery = Battery.AggregateBattery;

             // Get report
             var report = aggBattery.GetReport();

             if ((report.FullChargeCapacityInMilliwattHours == null) ||
                 (report.RemainingCapacityInMilliwattHours == null))
             {
                 ReactError error = new ReactError();
                 error.Message = "Could not fetch battery information.";
                 promise.Reject(error);
             }
             else
             {
                 var max = Convert.ToDouble(report.FullChargeCapacityInMilliwattHours);
                 var value = Convert.ToDouble(report.RemainingCapacityInMilliwattHours);
                 promise.Resolve(value / max);
             }
         }
        #region Constants
        [ReactConstantProvider]
        public void ConstantsViaConstantsProvider(ReactConstantProvider provider)
        {

            provider.Add("appVersion", "not available");
            provider.Add("buildVersion", "not available");
            provider.Add("buildNumber", 0);

            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            String bundleId = packageId.Name;
            String appName = package.DisplayName;

            try
            {
                provider.Add("appVersion", string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision));
                provider.Add("buildVersion", version.Build.ToString());
                provider.Add("buildNumber", version.Build.ToString());
            }
            catch
            {
            }

            String deviceName = "not available";
            String manufacturer = "not available";
            String device_id = "not available";
            String model = "not available";
            String hardwareVersion = "not available";
            String osVersion = "not available";
            String os = "not available";

            var cultureName = new DateTimeFormatter("longdate", new[] { "US" }).ResolvedLanguage;
            CultureInfo culture = new CultureInfo(cultureName);

            var geographicRegion = new Windows.Globalization.GeographicRegion();
            string countryAbbrivation = geographicRegion.CodeTwoLetter;

            try
            {
                var deviceInfo = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
                deviceName = deviceInfo.FriendlyName;
                manufacturer = deviceInfo.SystemManufacturer;
                device_id = deviceInfo.Id.ToString();
                model = deviceInfo.SystemProductName;
                hardwareVersion = deviceInfo.SystemHardwareVersion;
                os = deviceInfo.OperatingSystem;


                string deviceFamilyVersion = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
                ulong version2 = ulong.Parse(deviceFamilyVersion);
                ulong major = (version2 & 0xFFFF000000000000L) >> 48;
                ulong minor = (version2 & 0x0000FFFF00000000L) >> 32;
                osVersion = $"{major}.{minor}";
            }
            catch
            {
            }
            provider.Add("instanceId", "not available");
            provider.Add("deviceName", deviceName);
            provider.Add("systemName", "Windows");
            provider.Add("systemVersion", osVersion);
            provider.Add("apiLevel", "not available");
            provider.Add("model", model);
            provider.Add("brand", model);
            provider.Add("deviceId", hardwareVersion);
            provider.Add("deviceLocale", culture.Name);
            provider.Add("deviceCountry", countryAbbrivation);
            provider.Add("uniqueId", device_id);
            provider.Add("systemManufacturer", manufacturer);
            provider.Add("bundleId", bundleId);
            provider.Add("appName", appName);
            provider.Add("userAgent", "not available");
            provider.Add("timezone", TimeZoneInfo.Local.Id);
            provider.Add("isEmulator", IsEmulator(model));
            provider.Add("isTablet", IsTablet(os));
            provider.Add("carrier", "not available");
            provider.Add("is24Hour", is24Hour());
            provider.Add("maxMemory", MemoryManager.AppMemoryUsageLimit);
            provider.Add("firstInstallTime", package.InstalledDate.ToUnixTimeMilliseconds());
        }
        #endregion
    }
}
