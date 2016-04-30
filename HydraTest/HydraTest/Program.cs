using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

namespace HydraTest
{
    public partial class Program
    {
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
            rfidReader.IdReceived += rfidReader_IdReceived;
            button.ButtonPressed += button_ButtonPressed;
        }

        GT.Color getRandomColor()
        {
            var colors = new GT.Color[] { GT.Color.Red, GT.Color.Green, GT.Color.Blue, GT.Color.Yellow, GT.Color.Orange, GT.Color.Purple, GT.Color.Cyan, GT.Color.Magenta };
            Random rnd = new Random();
            var pilih = colors[rnd.Next(colors.Length)];
            return pilih;
        }
        void button_ButtonPressed(Button sender, Button.ButtonState state)
        {
            TimeSpan ts = new TimeSpan (0,0,1);
            multicolorLED.FadeRepeatedly(getRandomColor(), ts, getRandomColor(), ts);
        }

        void rfidReader_IdReceived(RFIDReader sender, string e)
        {
            led7R.Animate(100, true, true, false);
        }
    }
}
