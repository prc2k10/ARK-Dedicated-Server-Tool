﻿using QueryMaster;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ARK_Server_Manager.Lib
{
    public class ServerRuntime : DependencyObject, IDisposable
    {
        public enum ServerStatus
        {
            Unknown,
            Stopping,
            Stopped,
            Initializing,
            Running,
            Updating,
            Uninstalled
        }

        public enum SteamStatus
        {
            Unknown,
            NeedPublicIP,
            Unavailable,
            WaitingForPublication,
            Available
        }

        #region Model Properties

        public static readonly DependencyProperty SteamProperty = DependencyProperty.Register(nameof(Steam), typeof(SteamStatus), typeof(ServerRuntime), new PropertyMetadata(SteamStatus.Unknown));
        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(nameof(Status), typeof(ServerStatus), typeof(ServerRuntime), new PropertyMetadata(ServerStatus.Unknown));
        public static readonly DependencyProperty MaxPlayersProperty = DependencyProperty.Register(nameof(MaxPlayers), typeof(int), typeof(ServerRuntime), new PropertyMetadata(0));
        public static readonly DependencyProperty PlayersProperty = DependencyProperty.Register(nameof(Players), typeof(int), typeof(ServerRuntime), new PropertyMetadata(0));
        public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(nameof(Version), typeof(Version), typeof(ServerRuntime), new PropertyMetadata(new Version()));
        public static readonly DependencyProperty ProfileSnapshotProperty = DependencyProperty.Register(nameof(ProfileSnapshot), typeof(RuntimeProfileSnapshot), typeof(ServerRuntime), new PropertyMetadata(null));

        public SteamStatus Steam
        {
            get { return (SteamStatus)GetValue(SteamProperty); }
            protected set { SetValue(SteamProperty, value); }
        }

        public ServerStatus Status
        {
            get { return (ServerStatus)GetValue(StatusProperty); }
            protected set { SetValue(StatusProperty, value); }
        }

        public int MaxPlayers
        {
            get { return (int)GetValue(MaxPlayersProperty); }
            protected set { SetValue(MaxPlayersProperty, value); }
        }

        public int Players
        {
            get { return (int)GetValue(PlayersProperty); }
            protected set { SetValue(PlayersProperty, value); }
        }

        public Version Version
        {
            get { return (Version)GetValue(VersionProperty); }
            protected set { SetValue(VersionProperty, value); }
        }

        public RuntimeProfileSnapshot ProfileSnapshot
        {
            get { return (RuntimeProfileSnapshot)GetValue(ProfileSnapshotProperty); }
            set { SetValue(ProfileSnapshotProperty, value); }
        }

        #endregion

        public struct RuntimeProfileSnapshot
        {
            public string ServerName;
            public string InstallDirectory;
            public int QueryPort;
            public int ServerConnectionPort;
            public string ServerIP;
            public string LastInstalledVersion;
            public string ProfileName;            
            public bool RCONEnabled;
            public int RCONPort;
            public string ServerArgs;
            public string AdminPassword;
            public bool UseRawSockets;
            public bool SotFServer;
        };

        private IAsyncDisposable updateRegistration;
        private Process serverProcess;

        public ServerRuntime()
        {
        }

        private void RegisterForUpdates()
        {
            if (this.updateRegistration == null)
            {
                IPEndPoint localServerQueryEndPoint;
                IPEndPoint steamServerQueryEndPoint;
                GetServerEndpoints(out localServerQueryEndPoint, out steamServerQueryEndPoint);
                this.updateRegistration = ServerStatusWatcher.Instance.RegisterForUpdates(this.ProfileSnapshot.InstallDirectory, localServerQueryEndPoint, steamServerQueryEndPoint, ProcessStatusUpdate);
            }
        }

        private void UnregisterForUpdates()
        {
            if (this.updateRegistration != null)
            {
                this.updateRegistration.DisposeAsync().DoNotWait();
                this.updateRegistration = null;
            }
        }
        
        public Task AttachToProfile(ServerProfile profile)
        {
            AttachToProfileCore(profile);
            GetProfilePropertyChanges(profile);
            return TaskUtils.FinishedTask;
        }

        private void AttachToProfileCore(ServerProfile profile)
        {
            UnregisterForUpdates();

            this.ProfileSnapshot = new RuntimeProfileSnapshot
            {
                InstallDirectory = profile.InstallDirectory,
                QueryPort = profile.ServerPort,
                ServerConnectionPort = profile.ServerConnectionPort,
                ServerIP = String.IsNullOrWhiteSpace(profile.ServerIP) ? IPAddress.Loopback.ToString() : profile.ServerIP,
                LastInstalledVersion = profile.LastInstalledVersion,
                ProfileName = profile.ProfileName,
                RCONEnabled = profile.RCONEnabled,
                RCONPort = profile.RCONPort,
                ServerName = profile.ServerName,
                ServerArgs = profile.GetServerArgs(),
                AdminPassword = profile.AdminPassword,
                UseRawSockets = profile.UseRawSockets,
                SotFServer = profile.SOTF_Enabled,
            };

            Version lastInstalled;
            if (Version.TryParse(profile.LastInstalledVersion, out lastInstalled))
            {
                this.Version = lastInstalled;
            }

            RegisterForUpdates();
        }

        List<PropertyChangeNotifier> profileNotifiers = new List<PropertyChangeNotifier>();

        private void GetProfilePropertyChanges(ServerProfile profile)
        {
            foreach(var notifier in profileNotifiers)
            {
                notifier.Dispose();
            }

            profileNotifiers.Clear();
            profileNotifiers.AddRange(PropertyChangeNotifier.GetNotifiers(
                profile,
                new[] { 
                    ServerProfile.InstallDirectoryProperty,
                    ServerProfile.ServerPortProperty,
                    ServerProfile.ServerConnectionPortProperty,
                    ServerProfile.ServerIPProperty
                },
                (s, p) =>
                {
                    if (Status == ServerStatus.Stopped || Status == ServerStatus.Uninstalled || Status == ServerStatus.Unknown) { AttachToProfileCore(profile); }
                }));

        }

        public string GetServerExe()
        {
            return Path.Combine(this.ProfileSnapshot.InstallDirectory, Config.Default.ServerBinaryRelativePath, Config.Default.ServerExe);
        }

        public Task StartAsync()
        {
            if(!System.Environment.Is64BitOperatingSystem)
            {
                MessageBox.Show("ARK: Survival Evolved(tm) Server requires a 64-bit operating system to run.  Your operating system is 32-bit and therefore the Ark Server Manager cannot start the server.  You may still load and save profiles and settings files for use on other machines.", "64-bit OS Required", MessageBoxButton.OK, MessageBoxImage.Error);
                return TaskUtils.FinishedTask;
            }

            switch(this.Status)
            {
                case ServerStatus.Running:
                case ServerStatus.Initializing:
                case ServerStatus.Stopping:
                    Debug.WriteLine("Server {0} already running.", this.ProfileSnapshot.ProfileName);
                    return TaskUtils.FinishedTask;
            }

            UnregisterForUpdates();
            this.Status = ServerStatus.Initializing;
            
            var serverExe = GetServerExe();
            var serverArgs = this.ProfileSnapshot.ServerArgs;

            if (Config.Default.ManageFirewallAutomatically)
            {
                var ports = new List<int>() { this.ProfileSnapshot.QueryPort, this.ProfileSnapshot.ServerConnectionPort };
                if (this.ProfileSnapshot.RCONEnabled)
                {
                    ports.Add(this.ProfileSnapshot.RCONPort);
                }

                if(this.ProfileSnapshot.UseRawSockets)
                {
                    ports.Add(this.ProfileSnapshot.ServerConnectionPort + 1);
                }

                if (!FirewallUtils.EnsurePortsOpen(serverExe, ports.ToArray(), "ARK Server: " + this.ProfileSnapshot.ServerName))
                {
                    var result = MessageBox.Show("Failed to automatically set firewall rules.  If you are running custom firewall software, you may need to set your firewall rules manually.  You may turn off automatic firewall management in Settings.\r\n\r\nWould you like to continue running the server anyway?", "Automatic Firewall Management Error", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.No)
                    {
                        return TaskUtils.FinishedTask;
                    }
                }
            }

            var startInfo = new ProcessStartInfo();
            Process process;
            try
            {
                process = Process.Start(serverExe, serverArgs);
                process.EnableRaisingEvents = true;
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                throw new FileNotFoundException(String.Format("Unable to find {0} at {1}.  Server Install Directory: {2}", Config.Default.ServerExe, serverExe, this.ProfileSnapshot.InstallDirectory), serverExe, ex);
            }
            finally
            {
                RegisterForUpdates();
            }
            
            return TaskUtils.FinishedTask;            
        }

        // Delegate type to be used as the Handler Routine for SCCH
        delegate Boolean ConsoleCtrlDelegate(CtrlTypes CtrlType);

        // Enumerated type for the control messages sent to the handler routine
        enum CtrlTypes : uint
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GenerateConsoleCtrlEvent(CtrlTypes dwCtrlEvent, uint dwProcessGroupId);

        public static void SendStop(Process proc)
        {
            //This does not require the console window to be visible.
            if (AttachConsole((uint)proc.Id))
            {
                // Disable Ctrl-C handling for our program
                SetConsoleCtrlHandler(null, true);
                GenerateConsoleCtrlEvent(CtrlTypes.CTRL_C_EVENT, 0);

                // Must wait here. If we don't and re-enable Ctrl-C
                // handling below too fast, we might terminate ourselves.
                //proc.WaitForExit(2000);

                FreeConsole();

                //Re-enable Ctrl-C handling or any subsequently started
                //programs will inherit the disabled state.
                SetConsoleCtrlHandler(null, false);
            }
        }

        public async Task StopAsync()
        {
            switch(this.Status)
            {
                case ServerStatus.Running:
                case ServerStatus.Initializing:
                    try
                    {
                        UnregisterForUpdates();

                        var ts = new TaskCompletionSource<bool>();
                        EventHandler handler = (s, e) => ts.TrySetResult(true);
                        var process = this.serverProcess;
                        if (process != null)
                        {
                            try
                            {
                                this.Status = ServerStatus.Stopping;
                                this.Steam = SteamStatus.Unavailable;
                                process.Exited += handler;
                                if (AttachConsole((uint)process.Id))
                                {
                                    // Disable Ctrl-C handling for our program
                                    SetConsoleCtrlHandler(null, true);
                                    GenerateConsoleCtrlEvent(CtrlTypes.CTRL_C_EVENT, 0);
                                    await ts.Task;
                                    FreeConsole();
                                    SetConsoleCtrlHandler(null, false);
                                }
                                else
                                {
                                    process.Kill();
                                }
                            }
                            finally
                            {
                                process.Exited -= handler;
                            }
                        }                    
                    }
                    catch(InvalidOperationException)
                    {                    
                    }
                    finally
                    {
                        this.Status = ServerStatus.Stopped;
                        this.Steam = SteamStatus.Unavailable;
                    }
                    break;
            }            
        }

        public async Task<bool> UpgradeAsync(CancellationToken cancellationToken, bool validate)
        {
            if (!System.Environment.Is64BitOperatingSystem)
            {
                var result = MessageBox.Show("ARK: Survival Evolved(tm) Server requires a 64-bit operating system to run.  Your operating system is 32-bit and therefore the Ark Server Manager will be unable to start the server, but you may still install it or load and save profiles and settings files for use on other machines.\r\n\r\nDo you wish to continue?", "64-bit OS Required", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    return false;
                }
            }

            string serverExe = GetServerExe();
            try
            {
                await StopAsync();
                this.Status = ServerStatus.Updating;

                // Run the SteamCMD to install the server
                var steamCmdPath = Updater.GetSteamCMDPath();
                var steamCmdInstallServerArgsFormat = this.ProfileSnapshot.SotFServer ? Config.Default.SteamCmdInstallServerArgsFormat_SotF : Config.Default.SteamCmdInstallServerArgsFormat;
                //DataReceivedEventHandler dataReceived = (s, e) => Console.WriteLine(e.Data);
                var success = await ServerUpdater.UpgradeServerAsync(validate, this.ProfileSnapshot.InstallDirectory, steamCmdPath, steamCmdInstallServerArgsFormat, null /* dataReceived*/, cancellationToken);
                if (success)
                    this.Version = GetServerVersion();

                return success;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
            finally
            {
                this.Status = ServerStatus.Stopped;
            }
        }       

        public void Dispose()
        {
            this.updateRegistration.DisposeAsync().DoNotWait();
        }

        /// <summary>
        /// Returns the server version number from the version file.
        /// </summary>
        /// <returns>A version object containing the server version.</returns>
        private Version GetServerVersion()
        {
            var versionFile = Path.Combine(this.ProfileSnapshot.InstallDirectory, Config.Default.VersionFile);

            if (!string.IsNullOrWhiteSpace(versionFile) && File.Exists(versionFile))
            {
                var fileValue = File.ReadAllText(versionFile);

                if (!string.IsNullOrWhiteSpace(fileValue))
                {
                    string versionString = fileValue.ToString();
                    if (versionString.IndexOf('.') == -1)
                        versionString = versionString + ".0";

                    Version version;
                    if (Version.TryParse(versionString, out version))
                        return version;
                }
            }

            return new Version(0, 0);
        }

        private void GetServerEndpoints(out IPEndPoint localServerQueryEndPoint, out IPEndPoint steamServerQueryEndPoint)
        {
            //
            // Get the local endpoint for querying the local network
            //

            IPAddress localServerIpAddress;
            if (!String.IsNullOrWhiteSpace(this.ProfileSnapshot.ServerIP) && IPAddress.TryParse(this.ProfileSnapshot.ServerIP, out localServerIpAddress))
            {
                // Use the explicit Server IP
                localServerQueryEndPoint = new IPEndPoint(localServerIpAddress, Convert.ToUInt16(this.ProfileSnapshot.QueryPort));
            }
            else
            {
                // No Server IP specified, use Loopback
                localServerQueryEndPoint = new IPEndPoint(IPAddress.Loopback, Convert.ToUInt16(this.ProfileSnapshot.QueryPort));
            }

            //
            // Get the public endpoint for querying Steam
            //
            steamServerQueryEndPoint = null;
            if (!String.IsNullOrWhiteSpace(Config.Default.MachinePublicIP))
            {
                IPAddress steamServerIpAddress;
                if (IPAddress.TryParse(Config.Default.MachinePublicIP, out steamServerIpAddress))
                {
                    // Use the Public IP explicitly specified
                    steamServerQueryEndPoint = new IPEndPoint(steamServerIpAddress, Convert.ToUInt16(this.ProfileSnapshot.QueryPort));
                }
                else
                {
                    // Resolve the IP from the DNS name provided
                    try
                    {
                        var addresses = Dns.GetHostAddresses(Config.Default.MachinePublicIP);
                        if (addresses.Length > 0)
                        {
                            steamServerQueryEndPoint = new IPEndPoint(addresses[0], Convert.ToUInt16(this.ProfileSnapshot.QueryPort));
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Failed to resolve DNS address {0}: {1}\r\n{2}", Config.Default.MachinePublicIP, ex.Message, ex.StackTrace);
                    }
                }
            }
        }

        private void ProcessStatusUpdate(IAsyncDisposable registration, ServerStatusWatcher.ServerStatusUpdate update)
        {
            if(!Object.ReferenceEquals(registration, this.updateRegistration))
            {
                return;
            }

            TaskUtils.RunOnUIThreadAsync(() =>
            {
                switch (update.Status)
                {
                    case ServerStatusWatcher.ServerStatus.NotInstalled:
                        this.Status = ServerStatus.Uninstalled;
                        this.Steam = SteamStatus.Unavailable;
                        break;

                    case ServerStatusWatcher.ServerStatus.Initializing:
                        this.Status = ServerStatus.Initializing;
                        this.Steam = SteamStatus.Unavailable;
                        break;

                    case ServerStatusWatcher.ServerStatus.Stopped:
                        this.Status = ServerStatus.Stopped;
                        this.Steam = SteamStatus.Unavailable;
                        break;

                    case ServerStatusWatcher.ServerStatus.Running:
                        this.Status = ServerStatus.Running;
                        this.Steam = SteamStatus.WaitingForPublication;
                        break;

                    case ServerStatusWatcher.ServerStatus.Published:
                        this.Status = ServerStatus.Running;
                        this.Steam = SteamStatus.Available;
                        break;
                }

                if (update.ServerInfo != null)
                {
                    var match = Regex.Match(update.ServerInfo.Name, @"\(v([0-9]+\.[0-9]*)\)");
                    if (match.Success && match.Groups.Count >= 2)
                    {
                        var serverVersion = match.Groups[1].Value;
                        Version temp;
                        if (!String.IsNullOrWhiteSpace(serverVersion) && Version.TryParse(serverVersion, out temp))
                        {
                            this.Version = temp;
                        }
                    }

                    // set the player count using the players list, as this should only contain the current valid players.
                    this.Players = update.Players.Count;
                    this.MaxPlayers = update.ServerInfo.MaxPlayers;
                }

                this.serverProcess = update.Process;
            }).DoNotWait();
        }
    }
}
