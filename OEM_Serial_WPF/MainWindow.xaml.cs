using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO.Ports;
using System.Text.RegularExpressions;

using CRCSpace;



namespace OEM_Serial_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region variables
        // COM Port Specific
        SerialPort serial = new SerialPort();
        string[] Portnames = System.IO.Ports.SerialPort.GetPortNames();

        // Command Protocol Specific
        private byte lastDeviceCommand;
        private byte lastDeviceCommandType;
        const int CMDFrontOverhead = 3;
        const int CMDBackendOverhead = 2;
        const int CMDOverhead = 5;
        const int CMDIndex = 3;
        CRC Crc;
        byte[] nullData = new byte[0];
        string recieved_data;
        byte[] recieved_data_buffer = new byte[1024];
        int recieved_data_buffer_index = 0;
        #endregion

        #region deviceEnums
        enum DeviceCommand
        {
            AcquireImage = 0x0A,
            ReadFirmwareRevision = 0x0D,
            ReadFPGARevision = 0x10,
            IntegrationTime = 0x11,
            CCDSignalOffset = 0x13,
            CCDSignalGain = 0x14,
            ReadPixelCount = 0x15,
            CCDTemperatureSetpoint = 0x16,
            LaserModulationDuration = 0x17,
            LaserModulationPulseDelay = 0x18,
            LaserModulationPeriod = 0x19,
            LaserModulationPulseWidth = 0x1E,
            GetActualIntegrationTime = 0x1F,
            GetActualFrameCount = 0x20,
            TriggerDelay = 0x28,
            OutputTestPattern = 0x30,
            SelectUSBFullSpeed = 0x32,
            LaserModulation = 0x33,
            LaserOn = 0x34,
            CCDTemperatureEnable = 0x38,
            LaserModLinkToIntegrationTime = 0x39,
            CCDTemperature = 0x49,
            PassFailLED = 0x4A,
            ConnectionPing = 0x4B,
            ClearAcquireButtonPressed = 0x4C,
            Write = 0x80                            // Write opperations for the above commands
        }                                           // is the read command, plus 0x80

        enum DeviceResponse
        {
            Busy = -4,
            InternalAddressInvalid,
            InternalCommunicationFailure,
            InternalDataError,
            Success,
            LengthError,
            CRCError,
            UnrecognizedCommand,
            PortNotAvailable
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            Crc = new CRC();
            disableAllAcquistionButtons();
            setupCombobox();

            dateStampOutput();
            textBox_Output.Text += "Initialized" + System.Environment.NewLine;
        }

        /// <summary>
        /// Build our available port list
        /// </summary>
        public void setupCombobox()
        {
            comboBox_PortList.Items.Clear();
            Portnames = System.IO.Ports.SerialPort.GetPortNames();

            foreach (string currentPort in Portnames )
            {
                comboBox_PortList.Items.Add(currentPort);
            }
                
        }

        /// <summary>
        /// Datestamps in the debug terminal
        /// </summary>
        public void dateStampOutput()
        {
            System.DateTime dateTimeNow = DateTime.Now;
            string printDateNow = dateTimeNow.ToString();
            textBox_Output.Text += System.Environment.NewLine + printDateNow + " : ";
        }

        /// <summary>
        /// Refreshes the avavilable serial port list in the comboBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Refresh_Click(object sender, RoutedEventArgs e)
        {
            setupCombobox();

            if (serial.IsOpen)
            {
                serial.Close();
                dateStampOutput();
                textBox_Output.Text += "Closed active COM port." + System.Environment.NewLine;
            }

            disableAllAcquistionButtons();
            dateStampOutput();
            textBox_Output.Text += "Refreshed COM port list." + System.Environment.NewLine;
        }

        /// <summary>
        /// Function automatically commands the textbox to scroll to the bottom
        /// after text has been written
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_Output_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBox_Output.ScrollToEnd();
        }

        /// <summary>
        /// Simple regular expression for textbox validation to only allow numeric characters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsTextAllowed(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]"); //regex that matches disallowed text
            e.Handled = regex.IsMatch(e.Text);
        }

        /// <summary>
        /// Whenever we change the selected port, notify in the debug terminal
        /// and enable the connection button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox_PortList.SelectedItem != null)
            {
                dateStampOutput();
                textBox_Output.Text += "Selected port " + comboBox_PortList.SelectedItem.ToString() + System.Environment.NewLine;
                button_Connect.IsEnabled = true; 
            }
            else
                button_Connect.IsEnabled = false;
        }

        /// <summary>
        /// When we try to connect to a serial port, we need to configure the port 
        /// to the correct paramaters of an OEM WP Raman, then notify the user if
        /// the selected port successfully opens or not.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Connect_Click(object sender, RoutedEventArgs e)
        {
            // Configure the serial port
            serial.BaudRate = 921600; // Baud rate is hardcoded in device
            serial.PortName = comboBox_PortList.Text.ToString();
            serial.Handshake = System.IO.Ports.Handshake.None;
            serial.Parity = Parity.None;
            serial.DataBits = 8;
            serial.StopBits = StopBits.One;
            serial.ReadTimeout = 200;
            serial.WriteTimeout = 50;
            serial.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(RecieveData);

            // Attempt to connect to specified port and display errors as appropriate
            if (serial.IsOpen == false)
            {
                try
                {
                    serial.Open();
                    dateStampOutput();
                    textBox_Output.Text += "Opened serial port " + comboBox_PortList.SelectedItem.ToString() + System.Environment.NewLine;
                    enableAllAcquisitionButtons();
                }
                catch
                {
                    dateStampOutput();
                    textBox_Output.Text += "Failed to open serial port " + comboBox_PortList.SelectedItem.ToString() + System.Environment.NewLine;
                }                
            }                
            else
            {
                dateStampOutput();
                textBox_Output.Text += "Serial port " + comboBox_PortList.SelectedItem.ToString() + " is unavailable." + System.Environment.NewLine;
            }
        }

        private void enableAllAcquisitionButtons()
        {
            button_getIntegrationTime.IsEnabled = true;
            button_setIntegrationTime.IsEnabled = true;
            button_Ping.IsEnabled = true;
            button_Acquire.IsEnabled = true;
            button_FirmwareRev.IsEnabled = true;
            button_FpgaRev.IsEnabled = true;
        }

        private void disableAllAcquistionButtons()
        {
            button_getIntegrationTime.IsEnabled = false;
            button_setIntegrationTime.IsEnabled = false;
            button_Ping.IsEnabled = false;
            button_Acquire.IsEnabled = false;
            button_FirmwareRev.IsEnabled = false;
            button_FpgaRev.IsEnabled = false;
        }

        /// <summary>
        /// Assembles our packet and transmits it over the current COM port
        /// </summary>
        /// <param name="command">Raman command as per API</param>
        /// <param name="write">1 for a write, 0 for a read</param>
        /// <param name="dataT">Data to be sent for a write, null for a read</param>
        private void SendData(DeviceCommand command, int write, byte[] dataT)
        {
            recieved_data_buffer = new byte[1024];  // Re-initialize our return buffer
            recieved_data_buffer_index = 0; 

            byte[] cmd = new byte[CMDOverhead + dataT.Length];
            byte[] data = new byte[cmd.Length + 1];

            int dataArrayLength = dataT.Length;
            dataArrayLength = dataArrayLength + 1;

            // Assemble command information
            cmd[0] = 0;
            cmd[1] = (byte)dataArrayLength;
            if (write > 0)
            {
                byte temp = (byte)(command + 0x80);
                cmd[2] = temp;
            }
            else
                cmd[2] = (byte)command;
            if (dataArrayLength > 1)
            {
                for (int i = 0; i < dataT.Length; i++)
                {
                    cmd[i + 3] = dataT[i];
                }
                cmd[3 + dataArrayLength - 1] = Crc.CalculateCRC(cmd, 3 + dataT.Length);
            }
            else
                cmd[3] = Crc.CalculateCRC(cmd, 3);


            // add command data to packet
            cmd.CopyTo(data, 1);

            // add delimiters to packet
            data[0] = (byte)'<';
            data[data.Length - 1] = (byte)'>';                     

            // Send data out the serial port
            if (serial.IsOpen)
            {
                try
                {
                    // Send the binary data out the port
                    foreach (byte byteval in data)
                    {
                        byte[] _byteval = new byte[] { byteval };     // need to convert byte 
                        serial.Write(_byteval, 0, 1);
                        Thread.Sleep(1);
                    }

                    // Print data to debug terminal
                    dateStampOutput();
                    textBox_Output.Text += "Data Sent: ";
                    for (int j = 0; j < data.Length; j++)
                        textBox_Output.Text += data[j].ToString() + " ";
                    textBox_Output.Text += System.Environment.NewLine;
                }
                catch (Exception ex)
                {
                    // Print data to debug terminal
                    dateStampOutput();
                    textBox_Output.Text += "Data FAILED to send: ";
                    for (int j = 0; j < data.Length; j++)
                        textBox_Output.Text += data[j].ToString() + " ";
                    textBox_Output.Text += System.Environment.NewLine;
                }
            }

            else
            {
                // Print data to debug terminal
                dateStampOutput();
                textBox_Output.Text += "Serial port is closed. Cannot Send data.";
                textBox_Output.Text += System.Environment.NewLine;
            }

            enableAllAcquisitionButtons();
        }

        private delegate void UpdateUiTextDelegate(string text);

        // 
        // 
        /// <summary>
        /// Whenever a byte comes in over the serial port we need to store it in a buffer. 
        /// A buffer is used because we cannot assume that the entirety of the return packet
        /// comes in through a single chunk.
        /// 
        /// If the buffer is empty then we check for our start delimiter and start storing data
        ///
        /// Once we reach the minimum return packet size and see the end delimiter then we
        /// can process the data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecieveData(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // Collecting the characters received to our 'buffer' (string).
            recieved_data = serial.ReadExisting();

            byte[] array = Encoding.ASCII.GetBytes(recieved_data);

            foreach (byte inputByte in array)
            {
                if ((recieved_data_buffer_index < 1 && array[0] == 60) || recieved_data_buffer_index > 0)
                {
                    recieved_data_buffer[recieved_data_buffer_index] = inputByte;
                    recieved_data_buffer_index++;
                }
            }            

            Dispatcher.Invoke(DispatcherPriority.Send, new UpdateUiTextDelegate(WriteData), recieved_data);
        }


        private void WriteData(string text)
        {
            // If the last command sent was a regular command (not an image) process it
            // regularly.
            if (lastDeviceCommand != (byte)DeviceCommand.AcquireImage)
            {
                if (recieved_data_buffer_index >= 6 && recieved_data_buffer[recieved_data_buffer_index - 1] == 62)
                {
                    dateStampOutput();
                    textBox_Output.Text += "Data Recieved: ";
                    for (int j = 0; j < recieved_data_buffer_index; j++)
                        textBox_Output.Text += recieved_data_buffer[j].ToString() + " ";
                    textBox_Output.Text += System.Environment.NewLine;
                    processIncomingPacket();
                }
            }

            // An image is handled differently. It's simply the raw data and does not contain header information
            // or delimiters, so we're just going to pipe it directly into the output window.
            else
            {
                byte[] array = Encoding.ASCII.GetBytes(recieved_data);

                string outputString = "";

                foreach (byte inputByte in array)
                {
                    outputString += inputByte.ToString() + " ";
                }

                textBox_Output.Text += outputString;
            }
        }

        /// <summary>
        /// Processes the response from the instrument
        /// </summary>
        private void processIncomingPacket()
        {
            uint k;

            // If we're getting the information, the response
            // will be the value on the device. This information
            // is parsed in the case statements below.
            if (lastDeviceCommandType < 1)
            {
                switch (lastDeviceCommand)
                {

                    case (byte)DeviceCommand.ReadFirmwareRevision:
                        byte[] firmwareRevisionBuffer = new byte[4];
                        for (k = 0; k < firmwareRevisionBuffer.Length; k++)
                        {
                            firmwareRevisionBuffer[k] = recieved_data_buffer[4 + k];
                        }
                        dateStampOutput();
                        textBox_Output.Text += "Firmware revision  -  "
                                            + firmwareRevisionBuffer[0].ToString() + "."
                                            + firmwareRevisionBuffer[1].ToString() + "."
                                            + firmwareRevisionBuffer[2].ToString() + "."
                                            + firmwareRevisionBuffer[3].ToString()
                                            + "  -  returned from device. " + System.Environment.NewLine;
                        break;

                    case (byte)DeviceCommand.ReadFPGARevision:
                        byte[] fpgaRevisionNumberBuffer = new byte[7];
                        for (k = 0; k < fpgaRevisionNumberBuffer.Length; k++)
                        {
                            fpgaRevisionNumberBuffer[k] = recieved_data_buffer[4 + k];
                        }
                        dateStampOutput();
                        string str = System.Text.Encoding.ASCII.GetString(fpgaRevisionNumberBuffer);
                        textBox_Output.Text += "FPGA revision  -  " + str + "  -  returned from device. " + System.Environment.NewLine;
                        break;

                    case (byte)DeviceCommand.ConnectionPing:
                        enableAllAcquisitionButtons();
                        dateStampOutput();
                        textBox_Output.Text += "Ping returned from device " + System.Environment.NewLine;
                        break;

                    case (byte)DeviceCommand.IntegrationTime:
                        byte[] integrationTimeBuffer = new byte[3];
                        uint integrationTimeAccum = 0;
                        for (k = 0; k < integrationTimeBuffer.Length; k++)
                        {
                            integrationTimeBuffer[k] = recieved_data_buffer[4 + k];
                        }

                        // Combine and account for endian swap
                        integrationTimeAccum = (uint)integrationTimeBuffer[0] + ((uint)integrationTimeBuffer[1] << 8) + ((uint)integrationTimeBuffer[2] << 16);

                        dateStampOutput();
                        textBox_Output.Text += "Integration time: " + integrationTimeAccum.ToString() + System.Environment.NewLine;
                        break;

                    default:
                        break;
                }
            }

            // For a SET command, the response is a single byte as found in the
            // enum DeviceResponse found at the top
            else
            {
                dateStampOutput();
                textBox_Output.Text += "SET " + ((DeviceCommand)lastDeviceCommand).ToString() + " Response: "
                                    + ((DeviceResponse)recieved_data_buffer[4]).ToString()
                                    + System.Environment.NewLine;
            }
            
        }

        private void button_Ping_Click(object sender, RoutedEventArgs e)
        {
            disableAllAcquistionButtons();
            lastDeviceCommand = (byte)DeviceCommand.ConnectionPing;
            lastDeviceCommandType = 0;
            SendData(DeviceCommand.ConnectionPing, 0, nullData);
        }

        private void button_FirmwareRev_Click(object sender, RoutedEventArgs e)
        {
            disableAllAcquistionButtons();
            lastDeviceCommand = (byte)DeviceCommand.ReadFirmwareRevision;
            lastDeviceCommandType = 0;
            SendData(DeviceCommand.ReadFirmwareRevision, 0, nullData);
        }

        private void button_FpgaRev_Click(object sender, RoutedEventArgs e)
        {
            disableAllAcquistionButtons();
            lastDeviceCommand = (byte)DeviceCommand.ReadFPGARevision;
            lastDeviceCommandType = 0;
            SendData(DeviceCommand.ReadFPGARevision, 0, nullData);
        }

        private void button_Acquire_Click(object sender, RoutedEventArgs e)
        {
            disableAllAcquistionButtons();
            lastDeviceCommand = (byte)DeviceCommand.AcquireImage;
            lastDeviceCommandType = 0;
            SendData(DeviceCommand.AcquireImage, 0, nullData);

        }

        private void button_getIntegrationTime_Click(object sender, RoutedEventArgs e)
        {
            disableAllAcquistionButtons();
            lastDeviceCommand = (byte)DeviceCommand.IntegrationTime;
            lastDeviceCommandType = 0;
            SendData(DeviceCommand.IntegrationTime, 0, nullData);
        }

        private void button_setIntegrationTime_Click(object sender, RoutedEventArgs e)
        {
            disableAllAcquistionButtons();            

            if (int.Parse(textBox_integrationTime.Text) > 1677215)
            {
                textBox_integrationTime.Text = "1677215";
            }

            int tempInt = int.Parse(textBox_integrationTime.Text);
            byte[] intBytes = BitConverter.GetBytes(tempInt);
            byte[] payload = { 0, 0, 0 };
            payload[0] = intBytes[0];       // We have to trim down the four byte int to three bytes
            payload[1] = intBytes[1];       // as per the API
            payload[2] = intBytes[2];

            lastDeviceCommand = (byte)DeviceCommand.IntegrationTime;
            lastDeviceCommandType = 1; 
            SendData(DeviceCommand.IntegrationTime, 1, payload);
        }
    }
}
