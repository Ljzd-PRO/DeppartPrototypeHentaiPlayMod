using System.Collections.Generic;
using System.Linq;
using DeppartPrototypeHentaiPlayMod;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(HentaiPlayMod), "HentaiPlay", "0.1.0", "Ljzd-PRO")]

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

        public override void OnLateUpdate()
        {
            ReportBulbBroken();
            ReportZombieRun();
            ReportEnterLevel1();
            ReportLevel1Zombie();
            ReportEndZombie();
            ReportPlayerDied();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            ReportGameEnterEvent();
        }
        
        public override void OnApplicationQuit()
        {
            ReportGameExitEvent();
        }

        private void ReportActivateEvent(string eventName)
        {
            LoggerInstance.Msg($"ActivateEvent: {eventName}");
        }

        private void ReportDeactivateEvent(string eventName)
        {
            LoggerInstance.Msg($"DeactivateEvent: {eventName}");
        }

        private void ReportGameEnterEvent()
        {
            LoggerInstance.Msg("GameEnterEvent");
        }

        private void ReportGameExitEvent()
        {
            LoggerInstance.Msg("GameExitEvent");
        }

        private void UpdateEventStatus(string eventName, bool isActivate)
        {
            if (isActivate && !_events[eventName])
            {
                _events[eventName] = true;
                ReportActivateEvent(eventName);
            }
            else if (!isActivate && _events[eventName])
            {
                _events[eventName] = false;
                ReportDeactivateEvent(eventName);
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

            UpdateEventStatus(EventEnum.BulbBroken.ToString(), gameObject.gameObject.activeSelf);
        }

        private void ReportZombieRun()
        {
            var gameObject = GameObject.Find("/z/GameObject");
            if (gameObject == null)
            {
                UpdateEventStatus(EventEnum.ZombieRun.ToString(), false);
                return;
            }

            UpdateEventStatus(EventEnum.ZombieRun.ToString(), gameObject.gameObject.activeSelf);
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
                gameObject.gameObject.activeSelf && _events[EventEnum.BulbBroken.ToString()] &&
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

            UpdateEventStatus(EventEnum.EndZombie.ToString(), gameObject.gameObject.activeSelf);
        }
        
        private void ReportPlayerDied()
        {
            var gameObject = GameObject.Find("/DIE");
            if (gameObject == null)
            {
                UpdateEventStatus(EventEnum.PlayerDied.ToString(), false);
                return;
            }

            UpdateEventStatus(EventEnum.PlayerDied.ToString(), gameObject.gameObject.activeSelf);
        }
    }
}