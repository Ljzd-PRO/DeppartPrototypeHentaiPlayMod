using MelonLoader;

namespace DeppartPrototypeHentaiPlayMod
{
    public class BaseReporter : IEventReporter
    {
        private readonly MelonMod _melonMod;
        public BaseReporter(MelonMod melonMod)
        {
            _melonMod = melonMod;
        }

        public void ReportActivateEvent(string eventName)
        {
            _melonMod.LoggerInstance.Msg($"ActivateEvent: {eventName}");
        }

        public void ReportDeactivateEvent(string eventName)
        {
            _melonMod.LoggerInstance.Msg($"DeactivateEvent: {eventName}");
        }

        public void ReportGameEnterEvent()
        {
            _melonMod.LoggerInstance.Msg("GameEnterEvent");
        }

        public void ReportGameExitEvent()
        {
            _melonMod.LoggerInstance.Msg("GameExitEvent");
        }
    }
}