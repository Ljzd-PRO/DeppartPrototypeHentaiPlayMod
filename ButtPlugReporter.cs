using System;
using System.Threading;
using Buttplug.Client;
using Buttplug.Client.Connectors.WebsocketConnector;
using Buttplug.Core;

namespace DeppartPrototypeHentaiPlayMod
{
    public class ButtPlugReporter : BaseReporter
    {
        private readonly ButtplugClient _buttplugClient;
        private readonly Mutex _mutexLock = new Mutex();
        
        public ButtPlugReporter(HentaiPlayMod melonMod, Uri buttPlugServerUrl) : base(melonMod)
        {
            _buttplugClient = new ButtplugClient(MelonMod.Info.Name);
            _buttplugClient.DeviceAdded +=
                (sender, args) => MelonMod.LoggerInstance.Msg($"ButtPlug device added: {args}");
            _buttplugClient.DeviceRemoved +=
                (sender, args) => MelonMod.LoggerInstance.Msg($"ButtPlug device removed: {args}");
            _buttplugClient.ScanningFinished += (sender, args) =>
                MelonMod.LoggerInstance.Msg($"ButtPlug scanning finished: {args}");
            var connector = new ButtplugWebsocketConnector(buttPlugServerUrl);
            connector.Disconnected +=
                (sender, args) => MelonMod.LoggerInstance.Msg($"ButtPlug scanning finished: {args}");
            new Thread(() =>
            {
                try
                {
                    _buttplugClient.ConnectAsync(connector).Wait();
                }
                catch (ButtplugHandshakeException e)
                {
                    MelonMod.LoggerInstance.Msg("ButtPlug handshake failed", e);
                }

                _buttplugClient.StartScanningAsync().Wait();
            }).Start();
        }

        private void SendRequest(double vibrateSpeed)
        {
            new Thread(() =>
            {
                _mutexLock.WaitOne();
                try
                {
                    foreach (var device in _buttplugClient.Devices)
                        try
                        {
                            device.VibrateAsync(vibrateSpeed).Wait();
                        }
                        catch (ButtplugDeviceException e)
                        {
                            MelonMod.LoggerInstance.Msg($"ButtPlug device {device.Name} vibrate failed", e);
                        }
                }
                finally
                {
                    _mutexLock.ReleaseMutex();
                }
            }).Start();
        }

        public override void ReportActivateEvent(string eventName)
        {
            base.ReportActivateEvent(eventName);
            SendRequest(0.5);
        }

        public override void ReportDeactivateEvent(string eventName)
        {
            base.ReportDeactivateEvent(eventName);
            SendRequest(0);
        }

        public override void ReportGameEnterEvent()
        {
            base.ReportGameEnterEvent();
            SendRequest(0);
        }

        public override void ReportGameExitEvent()
        {
            base.ReportGameExitEvent();
            SendRequest(0);
        }

        public override void ReportShot()
        {
            base.ReportShot();
            SendRequest(1);
            SendRequest(0);
        }
    }
}