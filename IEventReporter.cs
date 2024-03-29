namespace DeppartPrototypeHentaiPlayMod
{
    public interface IEventReporter
    {
        void ReportActivateEvent(string eventName);
        void ReportDeactivateEvent(string eventName);
        void ReportGameEnterEvent();
        void ReportGameExitEvent();
        void ReportShot();
    }
}