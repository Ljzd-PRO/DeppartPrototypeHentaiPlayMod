using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Buttplug.Client;
using Buttplug.Client.Connectors.WebsocketConnector;
using Buttplug.Core;
using Buttplug.Core.Messages;
using MelonLoader;

namespace DeppartPrototypeHentaiPlayMod
{
    public struct ButtPlugAdditionalScalar
    {
        public bool Enable;
        public ActuatorType ActuatorType;
        public uint Index;
        public double Scalar;
    }

    public class ButtPlugReporter : BaseReporter
    {
        private readonly MelonPreferences_Entry<double> _buttPlugActiveVibrateScalar;
        private readonly MelonPreferences_Entry<ButtPlugAdditionalScalar[]> _buttPlugAdditionalScalarList;
        private readonly ButtplugClient _buttplugClient;
        private readonly MelonPreferences_Entry<int> _buttPlugShotVibrateDuration;
        private readonly MelonPreferences_Entry<double> _buttPlugShotVibrateScalar;
        private readonly MelonPreferences_Entry<uint[]> _buttPlugVibrateCmdIndexList;

        private readonly Dictionary<ButtplugClientDevice, Mutex> _deviceMutexMap =
            new Dictionary<ButtplugClientDevice, Mutex>();


        private double _baseVibrateScalar;

        public ButtPlugReporter
        (
            HentaiPlayMod melonMod,
            MelonPreferences_Entry<double> buttPlugActiveVibrateScalar,
            MelonPreferences_Entry<string> buttPlugServerUrlEntry,
            MelonPreferences_Entry<double> buttPlugShotVibrateScalar,
            MelonPreferences_Entry<uint[]> buttPlugVibrateCmdIndexList,
            MelonPreferences_Entry<int> buttPlugShotVibrateDuration,
            MelonPreferences_Entry<ButtPlugAdditionalScalar[]> buttPlugAdditionalScalarList
        ) : base(melonMod)
        {
            _buttPlugActiveVibrateScalar = buttPlugActiveVibrateScalar;
            _buttPlugShotVibrateScalar = buttPlugShotVibrateScalar;
            _buttPlugShotVibrateDuration = buttPlugShotVibrateDuration;
            _buttPlugAdditionalScalarList = buttPlugAdditionalScalarList;
            _buttPlugVibrateCmdIndexList = buttPlugVibrateCmdIndexList;
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
            var connector = new ButtplugWebsocketConnector(new Uri(buttPlugServerUrlEntry.Value));
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
                            if (_buttPlugVibrateCmdIndexList.Value.Length == 0)
                                device.VibrateAsync(scalar);
                            else
                                device.VibrateAsync(
                                    _buttPlugVibrateCmdIndexList.Value.Select(index => (index, scalar)));
                            foreach (var cmdInfo in _buttPlugAdditionalScalarList.Value)
                                if (cmdInfo.Enable)
                                    device.ScalarAsync(
                                        new ScalarCmd.ScalarSubcommand(
                                            cmdInfo.Index,
                                            scalar.Equals(_baseVibrateScalar) && _baseVibrateScalar == 0
                                                ? 0
                                                : cmdInfo.Scalar,
                                            cmdInfo.ActuatorType
                                        )
                                    );
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
                SendCommand(new[] { _buttPlugActiveVibrateScalar.Value, _baseVibrateScalar }, 5000);
            }
            else
            {
                _baseVibrateScalar = _buttPlugActiveVibrateScalar.Value;
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
            SendCommand(
                new[] { _buttPlugShotVibrateScalar.Value, _baseVibrateScalar },
                _buttPlugShotVibrateDuration.Value
            );
        }
    }
}