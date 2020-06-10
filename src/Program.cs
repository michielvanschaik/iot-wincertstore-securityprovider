using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;

namespace WindowsCertStoreSample
{
    class Program
    {
        private const string GlobalDeviceEndpoint = "global.azure-devices-provisioning.net";

        private static string s_idScope = Environment.GetEnvironmentVariable("DPS_IDSCOPE");
        private static string RegistrationId = "myDevice1";

        public static async Task RunAsync(ProvisioningDeviceClient provisioningDeviceClient, SecurityProvider security)
        {
            Console.WriteLine($"RegistrationID = {security.GetRegistrationID()}");

            Console.Write("ProvisioningClient RegisterAsync . . . ");
            DeviceRegistrationResult result = await provisioningDeviceClient.RegisterAsync().ConfigureAwait(false);

            Console.WriteLine($"{result.Status}");
            Console.WriteLine($"ProvisioningClient AssignedHub: {result.AssignedHub}; DeviceID: {result.DeviceId}");

            if (result.Status == ProvisioningRegistrationStatusType.Assigned)
            {
                X509Certificate2 certificate = (security as SecurityProviderX509).GetAuthenticationCertificate();
                Console.WriteLine($"Registration certificate thumbprint: {certificate.Thumbprint}");
                IAuthenticationMethod auth = new DeviceAuthenticationWithX509Certificate(result.DeviceId, certificate);
                using (DeviceClient iotClient = DeviceClient.Create(result.AssignedHub, auth, TransportType.Amqp))
                {
                    Console.WriteLine("DeviceClient OpenAsync.");
                    await iotClient.OpenAsync().ConfigureAwait(false);
                    Console.WriteLine("DeviceClient SendEventAsync.");
                    await iotClient.SendEventAsync(new Message(Encoding.UTF8.GetBytes("TestMessage"))).ConfigureAwait(false);
                    Console.WriteLine("DeviceClient CloseAsync.");
                    await iotClient.CloseAsync().ConfigureAwait(false);
                }
            }
        }

        static void Main(string[] args)
        {
            using (var security = new SecurityProviderX509Windows(RegistrationId))
            {
                using (var transport = new ProvisioningTransportHandlerAmqp(TransportFallbackType.TcpOnly))
                {
                    ProvisioningDeviceClient provClient =
                        ProvisioningDeviceClient.Create(GlobalDeviceEndpoint, s_idScope, security, transport);
                    RunAsync(provClient, security).GetAwaiter().GetResult();
                }
            }
        }
    }
}
