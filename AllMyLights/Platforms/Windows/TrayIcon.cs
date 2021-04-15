#if Windows
using System;
using System.Windows.Forms;
using System.Drawing;
using AllMyLights.Connectors.Sinks;

namespace AllMyLights.Platforms.Windows
{
    public class TrayIcon : ISink
    {
        private static TrayIcon Instance;

        private NotifyIcon NotifyIcon;

        private readonly ContextMenuStrip Menu = new ContextMenuStrip();
        private readonly ToolStripLabel TrayColorLabel = new ToolStripLabel();

        private bool Minimized { get; }

        public string Id { get; } = "my-lovely-windows-trayicon";

        private TrayIcon(bool minimized)
        {
            Minimized = minimized;
            Instance = this;

            Init();
        }

        public void Consume(object message)
        {
            if (Menu.InvokeRequired)
            {
                Menu.Invoke((MethodInvoker)delegate () { TrayColorLabel.Text = message.ToString(); });
                return;
            }

            TrayColorLabel.Text = message.ToString();
        }

        private void ShowMinimizeHint()
        {
            NotifyIcon.ShowBalloonTip(
                1000,
                "Minimize to tray",
                "You can minimize this application to tray by double clicking the tray icon.",
                ToolTipIcon.Info
            );
        }

        private void Init()
        {
            var exitButton = new ToolStripButton("Exit");

            Menu.Items.Add(TrayColorLabel);
            Menu.Items.Add(exitButton);

            NotifyIcon = new NotifyIcon()
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                Text = "AllMyLights",
                ContextMenuStrip = Menu,
                Visible = true
            };

            bool showConsole = !Minimized;
            NotifyIcon.DoubleClick += (sender, e) =>
            {
                showConsole = !showConsole;
                ConsoleWindow.Show(showConsole);
            };

            exitButton.Click += (sender, e) =>
            {
                Environment.Exit(0);
            };

            if (!Minimized)
            {
                ShowMinimizeHint();
            }
        }

        public static TrayIcon GetInstance(bool minimized) => Instance == null ? new TrayIcon(minimized) : Instance;

        public void Show(bool show)
        {
            NotifyIcon.Visible = show;
        }

        public object GetInfo()
        {
            return null;
        }
    }
}
#endif