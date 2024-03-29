using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading;

namespace DeppartPrototypeHentaiPlayMod
{
    public class HttpReporter : BaseReporter
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _reportUrl;

        public HttpReporter(HentaiPlayMod melonMod, string reportUrl) : base(melonMod)
        {
            try
            {
                Assembly.Load("Mono.HttpUtility");
            }
            catch (Exception)
            {
                MelonMod.LoggerInstance.Error("Mono.HttpUtility is required");
                throw;
            }

            _reportUrl = reportUrl;
        }

        private void SendRequest(Dictionary<string, string> query)
        {
            new Thread(() =>
            {
                try
                {
                    _httpClient.GetAsync(Utils.BuildRequestUri(_reportUrl, query)).Wait();
                }
                catch (Exception e)
                {
                    MelonMod.LoggerInstance.Error($"{nameof(HttpReporter)}: Report failed", e);
                }
            }).Start();
        }

        public override void ReportActivateEvent(string eventName)
        {
            base.ReportActivateEvent(eventName);
            SendRequest(
                new Dictionary<string, string>
                {
                    { "status", "activate" },
                    { "event_name", eventName }
                }
            );
        }

        public override void ReportDeactivateEvent(string eventName)
        {
            base.ReportDeactivateEvent(eventName);
            SendRequest(
                new Dictionary<string, string>
                {
                    { "status", "deactivate" },
                    { "event_name", eventName }
                }
            );
        }

        public override void ReportGameEnterEvent()
        {
            base.ReportGameEnterEvent();
            SendRequest(
                new Dictionary<string, string>
                {
                    { "event_name", EventEnum.GameEnter.ToString() }
                }
            );
        }

        public override void ReportGameExitEvent()
        {
            base.ReportGameExitEvent();
            SendRequest(
                new Dictionary<string, string>
                {
                    { "event_name", EventEnum.GameExit.ToString() }
                }
            );
        }

        public override void ReportShot()
        {
            base.ReportShot();
            SendRequest(
                new Dictionary<string, string>
                {
                    { "event_name", EventEnum.Shot.ToString() }
                }
            );
        }
    }
}