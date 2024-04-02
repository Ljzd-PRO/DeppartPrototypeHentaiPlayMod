using System;
using System.Collections.Generic;
using System.Linq;
using DeppartPrototypeHentaiPlayMod;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(HentaiPlayMod), "HentaiPlay", "0.1.0", "Ljzd-PRO")]
[assembly: MelonGame("N4bA", "DEPPART prototype")]
[assembly: MelonOptionalDependencies("Mono.HttpUtility")]

namespace DeppartPrototypeHentaiPlayMod
{
    public class HentaiPlayMod : MelonMod
    {
        private readonly Dictionary<string, bool> _events = new Dictionary<string, bool>
        {
            { EventEnum.BulbBroken.ToString(), false },
            { EventEnum.ZombieRun.ToString(), false },
            { EventEnum.EnterLevel1.ToString(), false },
            { EventEnum.Level1Zombie.ToString(), false },
            { EventEnum.EndZombie.ToString(), false },
            { EventEnum.PlayerDied.ToString(), false }
        };

        private MelonPreferences_Entry<Uri> _buttPlugServerUrlEntry;
        private MelonPreferences_Entry<bool> _disableEventLogEntry;

        private IEventReporter _eventReporter;
        private MelonPreferences_Entry<string> _eventReporterTypeEntry;
        private MelonPreferences_Entry<string> _httpReporterUrlEntry;
        private MelonPreferences_Entry<int> _httpReportInGameInterval;
        private MelonPreferences_Category _preferencesCategory;

        public HentaiPlayMod()
        {
            _eventReporter = new BaseReporter(this);
        }

        public override void OnInitializeMelon()
        {
            _preferencesCategory = MelonPreferences.CreateCategory("HentaiPlay");
            _eventReporterTypeEntry = _preferencesCategory.CreateEntry
            (
                "EventReporterType",
                nameof(ButtPlugReporter),
                description: "Type of reporter that report events in game"
            );
            _httpReporterUrlEntry = _preferencesCategory.CreateEntry
            (
                "HttpReporterUrl",
                "http://127.0.0.1:7788/report",
                description: "Report URL for HttpReporter"
            );
            _httpReportInGameInterval = _preferencesCategory.CreateEntry
            (
                "HttpReportInGameInterval",
                3000,
                description: "Time interval for HttpReporter to reporting InGame events"
            );
            _disableEventLogEntry = _preferencesCategory.CreateEntry
            (
                "DisableEventLog",
                false,
                description: "Not to report events to Console"
            );
            _buttPlugServerUrlEntry = _preferencesCategory.CreateEntry
            (
                "ButtPlugServerUrl",
                new Uri("ws://localhost:12345"),
                description: "Websocket URL of ButtPlug server (Intiface Central)"
            );
        }

        public override void OnLateInitializeMelon()
        {
            SetupEventReporter();
        }

        public override void OnLateUpdate()
        {
            ReportBulbBroken();
            ReportZombieRun();
            ReportEnterLevel1();
            ReportLevel1Zombie();
            ReportEndZombie();
            ReportPlayerDied();
        }

        public override void OnUpdate()
        {
            ReportShot();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            _eventReporter.ReportGameEnterEvent();
        }

        public override void OnApplicationQuit()
        {
            _eventReporter.ReportGameExitEvent();
        }

        private void SetupEventReporter()
        {
            var eventReporterType = _eventReporterTypeEntry.Value;
            LoggerInstance.Msg($"Using reporter: {eventReporterType}");
            switch (eventReporterType)
            {
                case nameof(BaseReporter):
                    _eventReporter = new BaseReporter(this);
                    break;
                case nameof(HttpReporter):
                    _eventReporter = new HttpReporter(
                        this,
                        _httpReporterUrlEntry.Value,
                        _httpReportInGameInterval.Value
                    );
                    break;
                case nameof(ButtPlugReporter):
                    _eventReporter = new ButtPlugReporter(this, _buttPlugServerUrlEntry.Value);
                    break;
            }

            if (_eventReporter is BaseReporter baseReporter) baseReporter.DisableEventLog = _disableEventLogEntry.Value;
        }

        private void UpdateEventStatus(string eventName, bool isActivate)
        {
            if (isActivate && !_events[eventName])
            {
                _events[eventName] = true;
                _eventReporter.ReportActivateEvent(eventName);
            }
            else if (!isActivate && _events[eventName])
            {
                _events[eventName] = false;
                _eventReporter.ReportDeactivateEvent(eventName);
            }
        }

        private void ReportBulbBroken()
        {
            var gameObject = GameObject.Find("/2etaj/shtyki/lampLopnyla");
            if (gameObject == null)
            {
                UpdateEventStatus(EventEnum.BulbBroken.ToString(), false);
                return;
            }

            UpdateEventStatus(EventEnum.BulbBroken.ToString(), gameObject.activeSelf);
        }

        private void ReportZombieRun()
        {
            var gameObject = GameObject.Find("/z/GameObject");
            if (gameObject == null)
            {
                UpdateEventStatus(EventEnum.ZombieRun.ToString(), false);
                return;
            }

            UpdateEventStatus(EventEnum.ZombieRun.ToString(), gameObject.activeSelf);
        }

        private void ReportEnterLevel1()
        {
            var gameObject = GameObject.Find("/lvl1");
            if (gameObject == null)
            {
                UpdateEventStatus(EventEnum.EnterLevel1.ToString(), false);
                return;
            }

            UpdateEventStatus
            (
                EventEnum.EnterLevel1.ToString(),
                gameObject.activeSelf && _events[EventEnum.BulbBroken.ToString()] &&
                _events[EventEnum.ZombieRun.ToString()]
            );
        }

        private void ReportLevel1Zombie()
        {
            var gameObject = GameObject.Find("/lvl1/z");
            if (gameObject == null)
            {
                UpdateEventStatus(EventEnum.Level1Zombie.ToString(), false);
                return;
            }

            var level1ZombieExists = gameObject.GetComponentsInChildren<Transform>()
                .FirstOrDefault(child =>
                    child.name.StartsWith("Ch10_nonPBR") && child.gameObject.activeSelf &&
                    child.GetComponent<Animator>().enabled) != null;
            UpdateEventStatus(EventEnum.Level1Zombie.ToString(), level1ZombieExists);
        }

        private void ReportEndZombie()
        {
            var gameObject = GameObject.Find("/end/Ch10_nonPBR (11)");
            if (gameObject == null)
            {
                UpdateEventStatus(EventEnum.EndZombie.ToString(), false);
                return;
            }

            UpdateEventStatus(EventEnum.EndZombie.ToString(), gameObject.activeSelf);
        }

        private void ReportPlayerDied()
        {
            var gameObject = GameObject.Find("/DIE");
            if (gameObject == null)
            {
                UpdateEventStatus(EventEnum.PlayerDied.ToString(), false);
                return;
            }

            UpdateEventStatus(EventEnum.PlayerDied.ToString(), gameObject.activeSelf);
        }

        private void ReportShot()
        {
            var gameObject = GameObject.Find("/First Person Controller/First Person Camera/Armpist");
            if (gameObject == null)
                return;
            var pistolObject = gameObject.GetComponent<pistol>();
            if (
                gameObject.activeSelf &&
                !(pistolObject.YBRAL > 0) && !(pistolObject.pl.walking == 2 || pistolObject.stene.vstene) &&
                Input.GetButtonDown("SHOOT") && !pistolObject.cantshoot && pistolObject.ammo > 0)
                _eventReporter.ReportShot();
        }
    }
}