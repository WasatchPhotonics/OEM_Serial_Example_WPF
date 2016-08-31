# Serial Example for OEM WP Raman Example

This repository is meant as an example implementation of our OEM WP Raman instruments using the provided serial interface. 

This example provides the code to querry device information such as the firmware revision, acquire a line of specta, and GET/SET the integration time. 

To connect to the device you will need an OEM WP Raman spectrometer and a USB to TTL cable that is configured for 3.3V. Using a 5V TTL signal could cause damage to the device and should be avoided.

![interface](https://github.com/WasatchPhotonics/OEM_Serial_Example_WPF/blob/master/images/interface.PNG?raw=true)

## API Documentation
[A detailed API specification can be found on our WasatchDevice.com website. Just click this link to download a PDF version.](http://wasatchdevices.com/wp-content/uploads/2016/08/OEM-API-Specification.pdf)

If any part of this specification is unclear, do not hesitate creating an issue in this repository or emailing us directly.