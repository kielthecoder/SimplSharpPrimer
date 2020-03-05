using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Diagnostics;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;

namespace Part5
{
    public class ControlSystem : CrestronControlSystem
    {
        private DmTx200C2G _tx1;

        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in ControlSystem constructor: {0}", e.StackTrace);
            }
        }

        public override void InitializeSystem()
        {
            try
            {
                // Currently only runs on DMPS architecture
                if (this.SystemControl == null)
                {
                    // Eventually we'll handle external switchers, too
                    ErrorLog.Error("Sorry, this program only runs on DMPS3 processors!");
                }
                else
                {
                    // Assume DM transmitter is connected to DM input 6
                    //_tx1 = new DmTx4k202C(0x14, this.SwitcherInputs[6] as DMInput);
                    _tx1 = new DmTx200C2G(0x14, this.SwitcherInputs[6] as DMInput);
                    _tx1.OnlineStatusChange += new OnlineStatusChangeEventHandler(tx_OnlineStatusChange);
                    _tx1.HdmiInput.InputStreamChange += new EndpointInputStreamChangeEventHandler(tx_InputStreamChange);
                    _tx1.HdmiInput.VideoAttributes.AttributeChange += new GenericEventHandler(tx_VideoAttributeChange);
                    _tx1.VgaInput.InputStreamChange += new EndpointInputStreamChangeEventHandler(tx_InputStreamChange);
                    _tx1.VgaInput.VideoAttributes.AttributeChange += new GenericEventHandler(tx_VideoAttributeChange);
                    //_tx1.HdmiInputs[1].InputStreamChange += new EndpointInputStreamChangeEventHandler(tx_InputStreamChange);
                    //_tx1.HdmiInputs[2].InputStreamChange += new EndpointInputStreamChangeEventHandler(tx_InputStreamChange);
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.StackTrace);
            }
        }

        void tx_VideoAttributeChange(object sender, GenericEventArgs args)
        {
            var vid = sender as VideoAttributesEnhanced;

            switch (args.EventId)
            {
                case VideoAttributeEventIds.VerticalResolutionFeedbackEventId:
                    CrestronConsole.PrintLine("Vert res = {0}", vid.VerticalResolutionFeedback.UShortValue);
                    break;
                case VideoAttributeEventIds.HorizontalResolutionFeedbackEventId:
                    CrestronConsole.PrintLine("Horz res = {0}", vid.HorizontalResolutionFeedback.UShortValue);
                    break;
                default:
                    CrestronConsole.PrintLine("tx_InputStreamChange: event id {0}", args.EventId);
                    break;
            }
        }

        void tx_OnlineStatusChange(GenericBase device, OnlineOfflineEventArgs args)
        {
            CrestronConsole.PrintLine("{0} is {1}", device, args.DeviceOnLine ? "ONLINE" : "OFFLINE");
        }

        void tx_InputStreamChange(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
        {
            switch (inputStream.StreamType)
            {
                case eDmStreamType.Hdmi:
                    var hdmiStream = inputStream as EndpointHdmiInput;
                    switch (args.EventId)
                    {
                        case EndpointInputStreamEventIds.SyncDetectedFeedbackEventId:
                            CrestronConsole.PrintLine("HDMI sync detected = {0}", hdmiStream.SyncDetectedFeedback.BoolValue);
                            break;
                        default:
                            CrestronConsole.PrintLine("HDMI event id {0}", args.EventId);
                            break;
                    }
                    break;
                case eDmStreamType.Vga:
                    var vgaStream = inputStream as EndpointVgaInput;
                    switch (args.EventId)
                    {
                        case EndpointInputStreamEventIds.SyncDetectedFeedbackEventId:
                            CrestronConsole.PrintLine("VGA sync detected = {0}", vgaStream.SyncDetectedFeedback.BoolValue);
                            break;
                        default:
                            CrestronConsole.PrintLine("VGA event id {0}", args.EventId);
                            break;
                    }
                    break;
                default:
                    CrestronConsole.PrintLine("InputStreamChange: StreamType = {0}", inputStream.StreamType);
                    break;
            }
        }
    }
}