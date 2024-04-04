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
        private readonly double _activeVibrateScalar;
        private readonly ButtplugClient _buttplugClient;
        private readonly Mutex _mutexLock = new Mutex();
        private readonly double _shotVibrateScalar;
        private double _baseVibrateScalar;

        public ButtPlugReporter
        (
            HentaiPlayMod melonMod,
            string buttPlugServerUrl,
            double activeVibrateScalar = 0.5,
            double shotVibrateScalar = 1
        ) : base(melonMod)
        {
            _activeVibrateScalar = activeVibrateScalar;
            _shotVibrateScalar = shotVibrateScalar;
            _buttplugClient = new ButtplugClient(MelonMod.Info.Name);
            _buttplugClient.DeviceAdded +=
                (sender, args) => MelonMod.LoggerInstance.Msg($"ButtPlug device added: {args.Device.Name}");
            _buttplugClient.DeviceRemoved +=
                (sender, args) => MelonMod.LoggerInstance.Msg($"ButtPlug device removed: {args.Device.Name}");
            _buttplugClient.ScanningFinished += (sender, args) =>
                MelonMod.LoggerInstance.Msg($"ButtPlug scanning finished: {args}");
            var connector = new ButtplugWebsocketConnector(new Uri(buttPlugServerUrl));
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
                catch (AggregateException e)
                {
                    MelonMod.LoggerInstance.Msg("Failed to connect to ButtPlug server", e);
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
                SendCommand(new[] { _activeVibrateScalar, _baseVibrateScalar }, 5000);
            }
            else
            {
                _baseVibrateScalar = _activeVibrateScalar;
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
            SendCommand(new[] { _shotVibrateScalar, _baseVibrateScalar });
        }
    }
}