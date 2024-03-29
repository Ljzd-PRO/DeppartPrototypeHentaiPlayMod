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
        private readonly int _reportInGameInterval;
        private readonly string _reportUrl;
        private bool _reportInGameStarted;
        private bool _stopReportingInGame;

        public HttpReporter(HentaiPlayMod melonMod, string reportUrl, int reportInGameInterval) : base(melonMod)
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
            _reportInGameInterval = reportInGameInterval;
        }

        private void SendRequest(Dictionary<string, string> query)
        {
            new Thread(() =>
            {
                query["t"] = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
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

        private void KeepReportingInGame()
        {
            if (_reportInGameStarted)
                return;
            var query = new Dictionary<string, string>
            {
                { "event_name", EventEnum.InGame.ToString() }
            };
            new Thread(() =>
            {
                while (!_stopReportingInGame)
                {
                    query["t"] = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
                    try
                    {
                        _httpClient.GetAsync(Utils.BuildRequestUri(_reportUrl, query)).Wait();
                    }
                    catch (Exception e)
                    {
                        MelonMod.LoggerInstance.Error($"{nameof(HttpReporter)}: Report failed", e);
                    }

                    Thread.Sleep(_reportInGameInterval);
                }
            }).Start();
            _reportInGameStarted = true;
        }

        private void StopReportingInGame()
        {
            _stopReportingInGame = true;
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
            KeepReportingInGame();
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
            StopReportingInGame();
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