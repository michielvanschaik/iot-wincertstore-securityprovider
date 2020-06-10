# Security Provider for Windows Certificate Store
This sample contains an implementation of the Azure IoT Device SDK Security Provider that uses the Windows Certificate Store to access the certificates needed for provisioning the device with the Device Provisioning Service and authenticating the device to IoT Hub. The default implementation in the Device SDK will read the certificate from the local file system, which is by no means secure.

For this sample to work, a device certificate and private key is needed at the device, and an enrollment configured for that device in the Device Provisioning Service. 

The Security Provider reads the certificates from the LocalMachine store. This means that the device.pfx must be present in the Local Machine section of the Certificate Store.


## Note
This sample only works if the current user has access to the LocalMachine store (for example the admin user), because the private key is stored in MachineKeySet which needs specific access. When trying to read this from LocalMachine store while the current user doesn't have access to the private key store at C:\ProgramData\Microsoft\Crypto\RSA\MachineKeys, this will throw an error like "The credentials supplied to the package were not recognized".
