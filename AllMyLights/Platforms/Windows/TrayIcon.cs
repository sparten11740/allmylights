#if Windows
using System;
using System.Windows.Forms;
using System.Drawing;


namespace AllMyLights.Platforms.Windows
{
    public class TrayIcon
    {
        private static TrayIcon Instance;

        private static NotifyIcon NotifyIcon;
        private static ColorSubject ColorSubject;
        private static readonly ToolStripLabel TrayColorLabel = new ToolStripLabel();

        private bool Minimized;

        private TrayIcon(ColorSubject colorSubject, bool minimized)
        {
            ColorSubject = colorSubject;
            Minimized = minimized;
            Instance = this;

            Init();
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
            var menu = new ContextMenuStrip();
            var exitButton = new ToolStripButton("Exit");

            menu.Items.Add(TrayColorLabel);
            menu.Items.Add(exitButton);

            NotifyIcon = new NotifyIcon()
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                Text = "AllMyLights",
                ContextMenuStrip = menu,
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

            ColorSubject.Updates().Subscribe((color) =>
            {
                TrayColorLabel.Text = color.ToString();
            });

            if (!Minimized)
            {
                ShowMinimizeHint();
            }
        }

        public static TrayIcon GetInstance(ColorSubject colorSubject, bool minimized) => Instance == null ? new TrayIcon(colorSubject, minimized) : Instance;

        public void Show(bool show)
        {
            NotifyIcon.Visible = show;
        }
    }
}
#endif