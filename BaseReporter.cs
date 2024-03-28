namespace DeppartPrototypeHentaiPlayMod
{
    public class BaseReporter : IEventReporter
    {
        protected readonly HentaiPlayMod MelonMod;

        public BaseReporter(HentaiPlayMod melonMod)
        {
            MelonMod = melonMod;
        }

        public virtual void ReportActivateEvent(string eventName)
        {
            MelonMod.LoggerInstance.Msg($"ActivateEvent: {eventName}");
        }

        public virtual void ReportDeactivateEvent(string eventName)
        {
            MelonMod.LoggerInstance.Msg($"DeactivateEvent: {eventName}");
        }

        public virtual void ReportGameEnterEvent()
        {
            MelonMod.LoggerInstance.Msg("GameEnterEvent");
        }

        public virtual void ReportGameExitEvent()
        {
            MelonMod.LoggerInstance.Msg("GameExitEvent");
        }
    }
}