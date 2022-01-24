using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using spotify.companion.Enums;
using System;

namespace spotify.companion.Model
{

    internal class InAppNotification : Response
    {
        private DispatcherTimer dispatcherTimer;
        private int timerValue = 0;
        private int timerDuration;

        private string _icon;
        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        private bool _isOpen;
        public bool IsOpen
        {
            get => _isOpen;
            set => SetProperty(ref _isOpen, value);
        }

        private InfoBarSeverity _severity;
        public InfoBarSeverity Severity
        {
            get => _severity;
            set => SetProperty(ref _severity, value);
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private bool _autoDismiss;
        public bool AutoDismiss
        {
            get => _autoDismiss;
            set => SetProperty(ref _autoDismiss, value);
        }

        public InAppNotification(ResponseType responseType, object payload, string title, bool autoDismiss = true)
            : base(responseType, payload)
        {
            IsOpen = false;
            Icon = GetFontIcon(responseType);
            Message = payload != null ? payload.ToString() : "";
            Title = title;
            Severity = GetSeverity(responseType);
            AutoDismiss = autoDismiss;
        }

        private static InfoBarSeverity GetSeverity(ResponseType responseType)
        {
            return responseType switch
            {
                ResponseType.Success => InfoBarSeverity.Success,
                ResponseType.Error => InfoBarSeverity.Error,
                ResponseType.Warning => InfoBarSeverity.Warning,
                ResponseType.Cancelled => InfoBarSeverity.Warning,
                _ => InfoBarSeverity.Informational,
            };
        }

        private static string GetFontIcon(ResponseType responseType)
        {
            return responseType switch
            {
                ResponseType.Success => "&#xF13E;",
                ResponseType.Error => "&#xE783;",
                ResponseType.Warning => "&#xE7BA;",
                ResponseType.Cancelled => "&#xE711;",
                _ => "&#xE946;",
            };
        }

        public void Hide()
        {
            IsOpen = false;
            ResetTimer();
        }

        /// <summary>
        /// /Show notification.
        /// </summary>
        /// <param name="duration">Sets the notification duration in seconds.</param>
        public void Show(int duration = 3)
        {
            timerDuration = duration;
            IsOpen = true;
            
            if (AutoDismiss) StartTimer();
        }

        private void StartTimer()
        {
            ResetTimer();
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, timerDuration);
            dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            timerValue++;
            if (timerValue >= timerDuration) Hide();
        }

        private void ResetTimer()
        {
            timerValue = 0;
            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();
                dispatcherTimer.Tick -= DispatcherTimer_Tick;
            }
        }
    }
}
