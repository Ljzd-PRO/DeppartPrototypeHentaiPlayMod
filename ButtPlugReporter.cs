using System;
using System.Collections.Generic;
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

        private readonly Dictionary<ButtplugClientDevice, Mutex> _deviceMutexMap =
            new Dictionary<ButtplugClientDevice, Mutex>();

        private readonly double _shotVibrateScalar;
        private readonly uint[] _vibrateCmdIndex;
        private double _baseVibrateScalar;

        public ButtPlugReporter
        (
            HentaiPlayMod melonMod,
            string buttPlugServerUrl,
            double activeVibrateScalar = 0.5,
            double shotVibrateScalar = 1,
            uint[] vibrateCmdIndex = null
        ) : base(melonMod)
        {
            _activeVibrateScalar = activeVibrateScalar;
            _shotVibrateScalar = shotVibrateScalar;
            _vibrateCmdIndex = vibrateCmdIndex;
            _buttplugClient = new ButtplugClient(MelonMod.Info.Name);
            _buttplugClient.DeviceAdded +=
                (sender, args) =>
                {
                    MelonMod.LoggerInstance.Msg($"ButtPlug device added: {args.Device.Name}");
                    _deviceMutexMap.Add(args.Device, new Mutex());
                };
            _buttplugClient.DeviceRemoved +=
                (sender, args) =>
                {
                    MelonMod.LoggerInstance.Msg($"ButtPlug device removed: {args.Device.Name}");
                    _deviceMutexMap.Remove(args.Device);
                };
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
            foreach (var device in _buttplugClient.Devices)
                new Thread(() =>
                {
                    _deviceMutexMap[device].WaitOne();
                    try
                    {
                        foreach (var scalar in scalars)
                        {
                            if (_vibrateCmdIndex == null || _vibrateCmdIndex.Length == 0)
                                device.VibrateAsync(scalar);
                            else
                                device.VibrateAsync(_vibrateCmdIndex.Select(index => (index, scalar)));
                            if (interval != 0)
                                Thread.Sleep(interval);
                        }
                    }
                    catch (ButtplugDeviceException e)
                    {
                        MelonMod.LoggerInstance.Msg($"ButtPlug device {device.Name} vibrate failed", e);
                    }
                    finally
                    {
                        _deviceMutexMap[device].ReleaseMutex();
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