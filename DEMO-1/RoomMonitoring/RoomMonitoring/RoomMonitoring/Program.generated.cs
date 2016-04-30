//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RoomMonitoring {
    using Gadgeteer;
    using GTM = Gadgeteer.Modules;
    
    
    public partial class Program : Gadgeteer.Program {
        
        /// <summary>The USB Client EDP module using socket 1 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.USBClientEDP usbClientEDP;
        
        /// <summary>The Relay X1 module using socket 12 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.RelayX1 relayX1;
        
        /// <summary>The LightSense module using socket 9 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.LightSense lightSense;
        
        /// <summary>The TempHumid SI70 module using socket 4 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.TempHumidSI70 tempHumidSI70;
        
        /// <summary>The Character Display module using socket 5 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.CharacterDisplay characterDisplay;
        
        /// <summary>The GasSense module using socket 10 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.GasSense gasSense;
        
        /// <summary>The Serial Camera L1 module using socket 11 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.SerialCameraL1 serialCameraL1;
        
        /// <summary>The Ethernet ENC28 module using socket 6 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.EthernetENC28 ethernetENC28;
        
        /// <summary>This property provides access to the Mainboard API. This is normally not necessary for an end user program.</summary>
        protected new static GHIElectronics.Gadgeteer.FEZSpiderII Mainboard {
            get {
                return ((GHIElectronics.Gadgeteer.FEZSpiderII)(Gadgeteer.Program.Mainboard));
            }
            set {
                Gadgeteer.Program.Mainboard = value;
            }
        }
        
        /// <summary>This method runs automatically when the device is powered, and calls ProgramStarted.</summary>
        public static void Main() {
            // Important to initialize the Mainboard first
            Program.Mainboard = new GHIElectronics.Gadgeteer.FEZSpiderII();
            Program p = new Program();
            p.InitializeModules();
            p.ProgramStarted();
            // Starts Dispatcher
            p.Run();
        }
        
        private void InitializeModules() {
            this.usbClientEDP = new GTM.GHIElectronics.USBClientEDP(1);
            this.relayX1 = new GTM.GHIElectronics.RelayX1(12);
            this.lightSense = new GTM.GHIElectronics.LightSense(9);
            this.tempHumidSI70 = new GTM.GHIElectronics.TempHumidSI70(4);
            this.characterDisplay = new GTM.GHIElectronics.CharacterDisplay(5);
            this.gasSense = new GTM.GHIElectronics.GasSense(10);
            this.serialCameraL1 = new GTM.GHIElectronics.SerialCameraL1(11);
            this.ethernetENC28 = new GTM.GHIElectronics.EthernetENC28(6);
        }
    }
}
