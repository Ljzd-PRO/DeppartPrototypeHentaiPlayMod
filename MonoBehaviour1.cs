using System.Collections.Generic;
using System.Linq;
using DeppartPrototypeHentaiPlayMod;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(HentaiPlayMod), "HentaiPlay", "1.0.0", "Ljzd-PRO")]

namespace DeppartPrototypeHentaiPlayMod
{
    public class HentaiPlayMod : MelonMod
    {
        private readonly Dictionary<string, bool> _events = new Dictionary<string, bool>()
        {
            {EventEnum.Level1Zombie.ToString(), false}
        };
        public override void OnUpdate()
        {
            ReportLevel1Zombie();
        }

        private void ReportActivateEvent(string eventName)
        {
            LoggerInstance.Msg($"ActivateEvent: {eventName}");
        }
        
        private void ReportDeactivateEvent(string eventName)
        {
            LoggerInstance.Msg($"DeactivateEvent: {eventName}");
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

        private void ReportLevel1Zombie()
        {
            var gameObject = GameObject.Find("lvl1/z");
            var level1ZombieExists = gameObject.GetComponentsInChildren<Transform>()
                .FirstOrDefault(child => child.name.StartsWith("Ch10_nonPBR") && child.gameObject.activeSelf) != null;
            UpdateEventStatus(EventEnum.Level1Zombie.ToString(), level1ZombieExists);
        }

        
    }
}