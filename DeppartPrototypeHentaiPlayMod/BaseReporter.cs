namespace DeppartPrototypeHentaiPlayMod
{
    public class BaseReporter : IEventReporter
    {
        protected readonly HentaiPlayMod MelonMod;
        public bool DisableEventLog = false;

        public BaseReporter(HentaiPlayMod melonMod)
        {
            MelonMod = melonMod;
        }

        public virtual void ReportActivateEvent(string eventName)
        {
            if (!DisableEventLog)
                MelonMod.LoggerInstance.Msg($"ActivateEvent: {eventName}");
        }

        public virtual void ReportDeactivateEvent(string eventName)
        {
            if (!DisableEventLog)
                MelonMod.LoggerInstance.Msg($"DeactivateEvent: {eventName}");
        }

        public virtual void ReportGameEnterEvent()
        {
            if (!DisableEventLog)
                MelonMod.LoggerInstance.Msg($"Event: {EventEnum.GameEnter.ToString()}");
        }

        public virtual void ReportGameExitEvent()
        {
            if (!DisableEventLog)
                MelonMod.LoggerInstance.Msg($"Event: {EventEnum.GameExit.ToString()}");
        }

        public virtual void ReportShot()
        {
            if (!DisableEventLog)
                MelonMod.LoggerInstance.Msg($"Event: {EventEnum.Shot.ToString()}");
        }
    }
}