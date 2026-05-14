using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace OP_Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // Регистрация кодировок для корректной работы батников
            try { Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); } catch { }
            InitializeComponent();
        }

        // --- ПРИВАТНОСТЬ (С объяснением для пользователя) ---
        private void DisableTelemetry_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы хотите отключить отправку системных отчетов (телеметрию)?\n\n" +
                "• Это уменьшит фоновую нагрузку на систему.\n" +
                "• Microsoft перестанет собирать данные об использовании ПК.\n" +
                "• Это абсолютно безопасно и не является вирусом.",
                "Настройка приватности",
                MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                RunAsBatch(@"sc stop DiagTrack & sc config DiagTrack start= disabled & reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection"" /v AllowTelemetry /t REG_DWORD /d 0 /f");
                MessageBox.Show("Приватность оптимизирована!");
            }
        }

        private void EnableTelemetry_Click(object sender, RoutedEventArgs e) =>
            RunAsBatch(@"sc config DiagTrack start= auto & sc start DiagTrack & reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection"" /v AllowTelemetry /f");

        // --- СЕТЬ (DNS) ---
        private void SetFastDNS_Click(object sender, RoutedEventArgs e) =>
            RunAsBatch(@"netsh interface ip set dns name=""Ethernet"" source=static addr=1.1.1.1 & netsh interface ip add dns name=""Ethernet"" addr=1.0.0.1 index=2 & netsh interface ip set dns name=""Wi-Fi"" source=static addr=1.1.1.1 & netsh interface ip add dns name=""Wi-Fi"" addr=1.0.0.1 index=2");

        private void SetAutoDNS_Click(object sender, RoutedEventArgs e) =>
            RunAsBatch(@"netsh interface ip set dns name=""Ethernet"" source=dhcp & netsh interface ip set dns name=""Wi-Fi"" source=dhcp");

        // --- ИКОНКИ И ЗАПУСК ---
        private void ResetIconCache_Click(object sender, RoutedEventArgs e) =>
            RunAsBatch(@"taskkill /f /im explorer.exe & timeout /t 3 /nobreak >nul & del /a /f /q %localappdata%\IconCache.db & del /a /f /q %localappdata%\Microsoft\Windows\Explorer\iconcache* & del /a /f /q %localappdata%\Microsoft\Windows\Explorer\thumbcache* & start explorer.exe");

        private void RemoveArrows_Click(object sender, RoutedEventArgs e) =>
            RunAsBatch(@"reg add ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons"" /v 29 /t REG_SZ /d ""%windir%\System32\shell32.dll,50"" /f & taskkill /f /im explorer.exe & start explorer.exe");

        private void RestoreArrows_Click(object sender, RoutedEventArgs e) =>
            RunAsBatch(@"reg delete ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons"" /v 29 /f & taskkill /f /im explorer.exe & start explorer.exe");

        private void RemoveStartupDelay_Click(object sender, RoutedEventArgs e) =>
            RunAsBatch(@"reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize"" /v StartupDelayInMSec /t REG_DWORD /d 0 /f");

        // --- ИНТЕРФЕЙС ---
        private void MenuClassic_Click(object sender, RoutedEventArgs e) =>
            RunAsBatch(@"reg add ""HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32"" /f /ve & taskkill /f /im explorer.exe & start explorer.exe");

        private void MenuDefault_Click(object sender, RoutedEventArgs e) =>
            RunAsBatch(@"reg delete ""HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}"" /f & taskkill /f /im explorer.exe & start explorer.exe");

        // --- СИСТЕМА ---
        private void OptimizeSpeed_Click(object sender, RoutedEventArgs e) =>
            RunAsBatch(@"reg add ""HKCU\Control Panel\Desktop"" /v AutoEndTasks /t REG_SZ /d 1 /f & reg add ""HKCU\Control Panel\Desktop"" /v WaitToKillAppTimeout /t REG_SZ /d 2000 /f");

        private void CleanTemp_Click(object sender, RoutedEventArgs e) =>
            RunAsBatch(@"del /s /f /q %temp%\*.* & del /s /f /q C:\Windows\Temp\*.*");

        private void InstantKill_Click(object sender, RoutedEventArgs e) =>
            RunAsBatch("taskkill /f /fi \"status eq not responding\"");

        // --- ЯДРО ВЫПОЛНЕНИЯ ---
        private void RunAsBatch(string commands)
        {
            string batchPath = Path.Combine(Path.GetTempPath(), "op_ultra_final.bat");
            try
            {
                File.WriteAllText(batchPath, "@echo off\nchcp 866 >nul\n" + commands, Encoding.GetEncoding(866));
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = batchPath,
                    Verb = "runas",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (Process p = Process.Start(psi)) p?.WaitForExit();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
            finally { if (File.Exists(batchPath)) File.Delete(batchPath); }
        }
    }
}