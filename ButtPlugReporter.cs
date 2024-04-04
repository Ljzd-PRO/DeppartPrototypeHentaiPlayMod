using System;
using System.Linq;
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
        private double _baseVibrateScalar;

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

        private void SendCommand(double[] scalars, int interval = 0)
        {
            new Thread(() =>
            {
                _mutexLock.WaitOne();
                try
                {
                    foreach (var device in _buttplugClient.Devices)
                        try
                        {
                            foreach (var scalar in scalars)
                            {
                                device.VibrateAsync(scalar);
                                if (interval != 0)
                                    Thread.Sleep(interval);
                            }
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
            var tempEvents = new[]
                { EventEnum.BulbBroken.ToString(), EventEnum.ZombieRun.ToString(), EventEnum.EnterLevel1.ToString() };
            if (tempEvents.Contains(eventName))
            {
                SendCommand(new[] { 0.5, _baseVibrateScalar }, 5000);
            }
            else
            {
                _baseVibrateScalar = 0.5;
                SendCommand(new[] { _baseVibrateScalar });
            }
        }

        public override void ReportDeactivateEvent(string eventName)
        {
            base.ReportDeactivateEvent(eventName);
            _baseVibrateScalar = 0;
            SendCommand(new[] { _baseVibrateScalar });
        }

        public override void ReportGameEnterEvent()
        {
            base.ReportGameEnterEvent();
            SendCommand(new[] { _baseVibrateScalar });
        }

        public override void ReportGameExitEvent()
        {
            base.ReportGameExitEvent();
            SendCommand(new[] { _baseVibrateScalar });
        }

        public override void ReportShot()
        {
            base.ReportShot();
            SendCommand(new[] { 1, _baseVibrateScalar });
        }
    }
}