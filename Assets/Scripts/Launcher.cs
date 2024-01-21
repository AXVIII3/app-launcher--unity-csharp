using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Unity.Services.Authentication;
using Unity.Services.RemoteConfig;
using UnityEngine.Networking;
using Unity.Services.Core;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace AXVIII3.Proxy.Launcher
{
    // Attributes as required by Remote Config
    public struct userAttributes {}
    public struct appAttributes {}


    // An enum containing all the possible states the launcher can be in
    enum LauncherStates
    {
        notReady,
        ready, // Everything is ok and the game can be played
        failed, // Something went wrong or there is some error
        downloadingGame, // Downloading the game files for the first time
        downloadingUpdate // Downloading the game files for an update
    }


    public class Launcher : MonoBehaviour
    {

        // Inspector Variables ------------------------------------------------------------------------------------------------
        [Header("UI References")]
        [Tooltip("Slider doubling as a progress bar to show download progress")] [SerializeField] private Slider progressBar;
        [Tooltip("Text element to show version number")] [SerializeField] private TMP_Text versionText;
        [Tooltip("Text element to show the launcher version")] [SerializeField] private TMP_Text launcherVersionText;
        [Tooltip("Text element to show the current stage of download")] [SerializeField] private TMP_Text downloadInfoText;
        [Tooltip("Text element in the Play button to show state of launcher")] [SerializeField] private TMP_Text playButtonText;
        [Tooltip("Background image for the launcher UI")] [SerializeField] private RawImage bgImage;
        [Tooltip("Image element for logo")] [SerializeField] private RawImage logoImage;
        [Tooltip("Text element to show some text under logo")] [SerializeField] private TMP_Text subTitle;
        [Tooltip("The Patch Notes screen")] [SerializeField] private GameObject changelogScreen;
        [Tooltip("Text element in Patch Notes Screen to show the patch notes")] [SerializeField] private TMP_Text changelog;

        [Header("Messages")]
        [Tooltip("Message to show in the downloadInfoText UI element during Ready state of the launcher")] [SerializeField] private string playMessage = "Ready to launch game!";
        [Tooltip("Message to show in the downloadInfoText UI element during Not Ready state of the launcher")] [SerializeField] private string notReadyMessage = "Setting some things up!";
        [Tooltip("Message to show in the downloadInfoText UI element during Failed state of the launcher")] [SerializeField] private string retryMessage = "And error has occurred!";
        [Tooltip("Message to show in the downloadInfoText UI element during DownloadingGame state of the launcher")] [SerializeField] private string downloadingGameMessage = "Downloading Game!";
        [Tooltip("Message to show in the downloadInfoText UI element during DownloadingUpdate state of the launcher")] [SerializeField] private string updatingGameMessage = "Updating Game!";

        [Header("Remote Config")]
        [Tooltip("Unity Project Environment ID")] [SerializeField] private string environmentID = "abcdefg-abcd-abcd-abcd-abcdefghijkl";
        [Tooltip("Remote Config Variable to kill the application")] [SerializeField] private string killVariable = "kill";
        [Tooltip("Remote Config Variable which contains latest application version")] [SerializeField] private string versionVariable = "version";
        [Tooltip("Remote Config Variable which contains test to show in the subtitle text box")] [SerializeField] private string subtitleVariable = "subtitle";
        [Tooltip("Remote Config Variable which contains the patch notes text")] [SerializeField] private string patchNotesVariable = "patch_notes";
        [Tooltip("Remote Config Variable which contains link to the background image")] [SerializeField] private string backgroundImageVariable = "background_image";
        [Tooltip("Remote Config Variable which contains link to the logo image")] [SerializeField] private string logoImageVariable = "logo_image";
        [Tooltip("Remote Config Variable which contains link to the Windows 64 zip version of the app")] [SerializeField] private string winAppLinkVariable = "win_64_link";
        [Tooltip("Remote Config Variable which contains link to the Windows 32/86 zip version of the app")] [SerializeField] private string winAppLink86Variable = "win_86_link";
        [Tooltip("Remote Config Variable which contains link to the linux zip version of the app")] [SerializeField] private string linuxAppLinkVariable = "linux_link";
        [Tooltip("Remote Config Variable which contains link to the mac zip version of the app")] [SerializeField] private string macAppLinkVariable = "mac_link"; 
        [Tooltip("Remote Config Variable which contains the actual executable name for the windows version")] [SerializeField] private string gameExecutableNameWinVariable = "win_executable_name";
        [Tooltip("Remote Config Variable which contains the actual executable name for the mac version")] [SerializeField] private string gameExecutableNameMacVariable = "mac_executable_name";
        [Tooltip("Remote Config Variable which contains the actual executable name for the linux version")] [SerializeField] private string gameExecutableNameLinuxVariable = "linux_executable_name";
        [Tooltip("Remote Config Variable which contains the name of the windows 64 zip which eventually becomes the folder name after extraction")] [SerializeField] private string gameZipOrFolderNameWinVariable = "win_64_name";
        [Tooltip("Remote Config Variable which contains the name of the windows 32 zip which eventually becomes the folder name after extraction")] [SerializeField] private string gameZipOrFolderNameWin86Variable = "win_86_name";
        [Tooltip("Remote Config Variable which contains the name of the mac zip which eventually becomes the folder name after extraction")] [SerializeField] private string gameZipOrFolderNameMacVariable = "mac_name";
        [Tooltip("Remote Config Variable which contains the name of the linux zip which eventually becomes the folder name after extraction")] [SerializeField] private string gameZipOrFolderNameLinuxVariable = "linux_name";

        // Hidden Variables ~ Computed ---------------------------------------------------------------------------------------
        private string rootPath; // Current Directory
        private string dataPath; // Directory where the app is to be installed
        private string gameZipPath; // Path where the game zip will be installed
        private string gameExecutablePath; // Path where the game executable can be found

        [Header("Links")]
        private string appLink; // Automatically assigned depending on OS
        private string winAppLink; // Link to windows x64 zip
        private string winAppLink86; // Link to windows x86 zip
        private string linuxAppLink; // Link to linux zip
        private string macAppLink; // Link to mac zip

        [Header("Names")]
        private string gameExecutableName; // Automatically assigned depending on OS
        private string gameExecutableNameWin; // Game executable name if OS is windows
        private string gameExecutableNameMac; // Game executable name if OS is mac
        private string gameExecutableNameLinux; // Game executable name if OS is linux
        private string gameZipOrFolderName; // Automatically assigned depending on OS
        private string gameZipOrFolderNameWin; // The name of zip or folder for windows x64
        private string gameZipOrFolderNameWin86; // The name of zip or folder for windows x86
        private string gameZipOrFolderNameMac; // The name of zip or folder for mac 
        private string gameZipOrFolderNameLinux; // The name of zip or folder for linux   

        // State Variable ~ Computed ------------------------------------------------------------------------------------------
        private LauncherStates _state; // Helper variable for the auto variable State
        internal LauncherStates State // A variable to store the current state of the launcher
        {
            get => _state;
            set
            {
                _state = value;
                switch (_state)
                {
                    case LauncherStates.notReady:
                        playButtonText.text = "Loading";
                        downloadInfoText.text = notReadyMessage;
                        progressBar.gameObject.SetActive(false);
                        break;
                    case LauncherStates.ready:
                        playButtonText.text = "Play";
                        downloadInfoText.text = playMessage;
                        progressBar.gameObject.SetActive(false);
                        break;
                    case LauncherStates.failed:
                        playButtonText.text = "Retry!";
                        downloadInfoText.text = retryMessage;
                        progressBar.gameObject.SetActive(false);
                        break;
                    case LauncherStates.downloadingGame:
                        playButtonText.text = "Downloading";
                        downloadInfoText.text = downloadingGameMessage;
                        progressBar.gameObject.SetActive(false);
                        break;
                    case LauncherStates.downloadingUpdate:
                        playButtonText.text = "Updating";
                        downloadInfoText.text = updatingGameMessage;
                        progressBar.gameObject.SetActive(true);
                        break;
                    default: break;
                }
            }
        }

        // Called when the script instance is being loaded --------------------------------------------------------------------
        private void Awake ()
        {
            State = LauncherStates.notReady;
            Screen.SetResolution(900, 540, false);
            Initialize (); // Start all processes
        }

        // Initialization -----------------------------------------------------------------------------------------------------
        private async void Initialize() 
        {
            if (!Utilities.CheckForInternetConnection())
            {
                ShowExceptions.Singleton.ShowError ("Connection Error!", "Please make sure that you are connected to a stable internet connection and retry!");
                State = LauncherStates.notReady;
                return;
            }

            // Initializing Remote Config
            UnityEngine.Debug.Log ("INITIALIZING: Initializing Remote Config");
            await InitializeRemoteConfigAsync();
            RemoteConfigService.Instance.SetEnvironmentID (environmentID); // Setting the environment to pull the data from
            RemoteConfigService.Instance.FetchConfigs <userAttributes, appAttributes> (new userAttributes(), new appAttributes()); // Fetching configs with default settings

            RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;
        }

        // Initializing Unity Gaming Services handlers and Authentication required for Remote Config --------------------------
        private async Task InitializeRemoteConfigAsync()
        {
                // Initialize handlers for U.G.S.
                UnityEngine.Debug.Log ("INITIALIZING: Initializing UGS handlers");
                await UnityServices.InitializeAsync();

                // Remote Config requires authentication for managing environment information
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    UnityEngine.Debug.Log ("INITIALIZING: Initializing Authentication handlers");
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
        }

        // Remote Config Settings fetched ---------------------------------------------------------------------------------------
        private void ApplyRemoteSettings (ConfigResponse configResponse)
        {
            if (configResponse.status != ConfigRequestStatus.Success) 
            {
                Initialize (); // Re-Initialize if the Remote Config response status was not successful 
                State = LauncherStates.notReady;
                return;
            }

            UnityEngine.Debug.Log ("SUCCESS: Remote Config configurations fetched!");
            UnityEngine.Debug.Log ("INITIALIZING: Assigning remote variables!");

            if (RemoteConfigService.Instance.appConfig.GetBool(killVariable))
            {
                ShowExceptions.Singleton.ShowError ("Maintenance break", "Sorry, we are currently under maintenance!\nApologies for any inconvenience caused!");
                State = LauncherStates.notReady;
                return;
            }

            // Get latest remote variable values
            winAppLink = RemoteConfigService.Instance.appConfig.GetString(winAppLinkVariable);
            winAppLink86 = RemoteConfigService.Instance.appConfig.GetString(winAppLink86Variable);
            linuxAppLink = RemoteConfigService.Instance.appConfig.GetString(linuxAppLinkVariable);
            macAppLink = RemoteConfigService.Instance.appConfig.GetString(macAppLinkVariable);
            gameExecutableNameWin = RemoteConfigService.Instance.appConfig.GetString(gameExecutableNameWinVariable);
            gameExecutableNameMac = RemoteConfigService.Instance.appConfig.GetString(gameExecutableNameMacVariable);
            gameExecutableNameLinux = RemoteConfigService.Instance.appConfig.GetString(gameExecutableNameLinuxVariable);
            gameZipOrFolderNameWin = RemoteConfigService.Instance.appConfig.GetString(gameZipOrFolderNameWinVariable);
            gameZipOrFolderNameWin86 = RemoteConfigService.Instance.appConfig.GetString(gameZipOrFolderNameWin86Variable);
            gameZipOrFolderNameMac = RemoteConfigService.Instance.appConfig.GetString(gameZipOrFolderNameMacVariable);
            gameZipOrFolderNameLinux = RemoteConfigService.Instance.appConfig.GetString(gameZipOrFolderNameLinuxVariable);

            // Set up all the UI elements
            subTitle.text = RemoteConfigService.Instance.appConfig.GetString(subtitleVariable);
            changelog.text = RemoteConfigService.Instance.appConfig.GetString(patchNotesVariable);
            StartCoroutine(
                GetImages(
                    RemoteConfigService.Instance.appConfig.GetString(backgroundImageVariable),
                    RemoteConfigService.Instance.appConfig.GetString(logoImageVariable)
                )
            );

            // After Remote Config is setup, Initialize the paths / directory structure
            InitializePaths ();
        }

        // Gets the current directory and creates the directory structure ------------------------------------------------------
        private void InitializePaths ()
        {
            // Get all the required directory paths 
            rootPath = Directory.GetCurrentDirectory();
            dataPath = Path.Combine(rootPath, "Data");

            // Assign variables based on OS
#if UNITY_EDITOR
            gameExecutableName = gameExecutableNameWin;
            if (RuntimeInformation.OSArchitecture == Architecture.X64 || RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                appLink = winAppLink;
                gameZipPath = Path.Combine(dataPath, gameZipOrFolderNameWin + ".zip");
                gameExecutablePath = Path.Combine(dataPath, gameZipOrFolderNameWin, gameExecutableName);
                gameZipOrFolderName = gameZipOrFolderNameWin;
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.X86 || RuntimeInformation.OSArchitecture == Architecture.Arm)
            {
                appLink = winAppLink86;
                gameZipPath = Path.Combine(dataPath, gameZipOrFolderNameWin86 + ".zip");
                gameExecutablePath = Path.Combine(dataPath, gameZipOrFolderNameWin86, gameExecutableName);
                gameZipOrFolderName = gameZipOrFolderNameWin86;
            }
#elif UNITY_STANDALONE_WIN
            gameExecutableName = gameExecutableNameWin;
            if (RuntimeInformation.OSArchitecture == Architecture.X64 || RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                appLink = winAppLink;
                gameZipPath = Path.Combine(dataPath, gameZipOrFolderNameWin + ".zip");
                gameExecutablePath = Path.Combine(dataPath, gameZipOrFolderNameWin, gameExecutableName);
                gameZipOrFolderName = gameZipOrFolderNameWin;
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.X86 || RuntimeInformation.OSArchitecture == Architecture.Arm)
            {
                appLink = winAppLink86;
                gameZipPath = Path.Combine(dataPath, gameZipOrFolderNameWin86 + ".zip");
                gameExecutablePath = Path.Combine(dataPath, gameZipOrFolderNameWin86, gameExecutableName);
                gameZipOrFolderName = gameZipOrFolderNameWin86;
            }
#elif UNITY_STANDALONE_LINUX
                appLink = linuxAppLink;
                gameExecutableName = gameExecutableNameLinux;
                gameZipPath = Path.Combine(dataPath, gameZipOrFolderNameLinux + ".zip");
                gameExecutablePath = Path.Combine(dataPath, gameZipOrFolderNameLinux, gameExecutableName);
                gameZipOrFolderName = gameZipOrFolderNameLinux;
#elif UNITY_STANDALONE_MAC
                appLink = macAppLink;
                gameExecutableName = gameExecutableNameMac;
                gameZipPath = Path.Combine(dataPath, gameZipOrFolderNameMac + ".zip");
                gameExecutablePath = Path.Combine(dataPath, gameZipOrFolderNameMac, gameExecutableName);
                gameZipOrFolderName = gameZipOrFolderNameMac;
#endif

            // After all the paths have been initialized, check for updates
            CheckForUpdates ();
        }

        // A coroutine to get images from the links provided by remotes config -------------------------------------------------
        IEnumerator GetImages (string backgroundUrl, string logoUrl)
        {
            // Creating web requests to download and assign the background and logo
            UnityWebRequest request1 = UnityWebRequestTexture.GetTexture(backgroundUrl);
            UnityWebRequest request2 = UnityWebRequestTexture.GetTexture(logoUrl);

            // Sending the web requests
            yield return request1.SendWebRequest();
            yield return request2.SendWebRequest();

            if (request1.result != UnityWebRequest.Result.Success ) {}
            else bgImage.texture = ((DownloadHandlerTexture)request1.downloadHandler).texture;
            

            if (request2.result != UnityWebRequest.Result.Success ) {}
            else logoImage.texture = ((DownloadHandlerTexture)request2.downloadHandler).texture;
            

            request1.Dispose();
            request2.Dispose();
        }

        // Checks for game files' integrity and/or for version changes ----------------------------------------------------------- 
        private void CheckForUpdates () 
        {
            if (File.Exists (gameExecutablePath) && PlayerPrefs.HasKey ("Version"))
            {
                Version localVersion = new Version (PlayerPrefs.GetString ("Version"));
                versionText.text = localVersion.ToString ();
                
                try
                {
                    Version onlineVersion = new Version (RemoteConfigService.Instance.appConfig.GetString (versionVariable)); // Version of latest game files from server
                    
                    // If server versions and local version are different.. update
                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        InstallGameFiles (true, onlineVersion);
                    }
                    else
                    {
                        // If game executable does not exist... download
                        if (!File.Exists(gameExecutablePath))
                        {
                            InstallGameFiles (false, onlineVersion);
                        }
                        else State = LauncherStates.ready;
                    }
                }
                catch (Exception ex)
                {
                    ShowExceptions.Singleton.ShowError(ex.Message, ex.ToString());
                    State = LauncherStates.failed;
                }
                State = LauncherStates.ready; // Set launcher state to ready after installing game files or if all game files are up to date
                return;
            }

            // If either Game Executable is unavailable or the local Version data can not be found then install game files
            InstallGameFiles (false, Version.zero);
            return;
        }

        // Clean install game files
        private void InstallGameFiles (bool _isUpdate, Version _onlineVersion)
        {
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath); // Create Data directory if there isn't one
            if (File.Exists(gameZipPath)) File.Delete(gameZipPath); // If the game zip exists probably due to a faulty installation earlier, delete it
            if (Directory.Exists(Path.Combine(dataPath, gameZipOrFolderName))) Directory.Delete(Path.Combine(dataPath, gameZipOrFolderName), true); // If the application exists either because the new version is an update or the old installation was faulty, delete it
            if (PlayerPrefs.HasKey("Version")) PlayerPrefs.DeleteKey ("Version"); // Delete the current version data

            try
            {
                WebClient webClient = new WebClient(); // Web client to download game files
                if (_isUpdate)
                {
                    State = LauncherStates.downloadingUpdate;
                }
                else
                {
#if UNITY_STANDALONE_LINUX
                    if (PlayerPrefs.HasKey("LinuxFlag")) PlayerPrefs.DeleteKey("LinuxFlag");
#endif
                    State = LauncherStates.downloadingGame;
                    _onlineVersion = new Version(RemoteConfigService.Instance.appConfig.GetString(versionVariable));
                }

                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler (DownloadingGameCompletedCallback);
                webClient.DownloadProgressChanged += (s, e) =>
                {
                    if (!progressBar.gameObject.activeInHierarchy) progressBar.gameObject.SetActive(true);
                    progressBar.value = e.ProgressPercentage;
                    downloadInfoText.text = "[" + e.ProgressPercentage + "%] Downloaded " + (e.BytesReceived / 1000000) + " out of " + (e.TotalBytesToReceive / 1000000) + "mb";

                    if (e.ProgressPercentage == 100)
                    {
                        downloadInfoText.text = "Installing game files";
                        progressBar.gameObject.SetActive(false);
                    }
                };
                webClient.DownloadFileAsync(new Uri(appLink), gameZipPath, _onlineVersion);
                webClient.Dispose();
            }
            catch (Exception ex)
            {
                ShowExceptions.Singleton.ShowError(ex.Message, ex.ToString());
                State = LauncherStates.failed;
            }
        }

        // Extract the zip and delete downloaded zip after download is completed ------------------------------------------------
        private void DownloadingGameCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                ZipFile.ExtractToDirectory(gameZipPath, Path.Combine(dataPath, gameZipOrFolderName));
                File.Delete(gameZipPath);

                string onlineVersion = ((Version)e.UserState).ToString();
                PlayerPrefs.SetString("Version", onlineVersion);
                versionText.text = onlineVersion;

                State = LauncherStates.ready;
            }
            catch (Exception ex)
            {
                ShowExceptions.Singleton.ShowError(ex.Message, ex.ToString());
                State = LauncherStates.failed;
            }
        }

        // Do something when player clicks on play ----------------------------------------------------------------------------------
        private void OnPlayButtonClick()
        {
            if (State == LauncherStates.downloadingGame) return;
            if (State == LauncherStates.downloadingUpdate) return;
            if (State == LauncherStates.notReady) 
            {
                Initialize ();
                return;
            }


            if ((File.Exists(gameExecutablePath) && PlayerPrefs.HasKey("Version")) && State == LauncherStates.ready)
            {
#if UNITY_STANDALONE_LINUX
                // File signing for linux to mark the game as executable 
                // If the executable has already been signed then run the game
                if (PlayerPrefs.HasKey("LinuxFlag")) 
                {
                    RunGame();
                    return;
                }
                
                // If the executable has not been signed sign it and run the game
                ProcessStartInfo startInfo = new ProcessStartInfo() // Run bash commands to sign the file
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"chmod +x  " + gameExecutablePath + "\"",
                    CreateNoWindow = true
                };

                try
                {
                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    ShowExceptions.Singleton.ShowError(ex.Message, ex.ToString());
                    State = LauncherStates.failed;
                }

                PlayerPrefs.SetInt("LinuxFlag", 1); // Set the LinuxFlag to show that the file has been already signed

                Invoke("RunGame", 1f);
#else
                RunGame();
#endif
            }
            else if (State == LauncherStates.failed || !File.Exists(gameExecutablePath) || !PlayerPrefs.HasKey("Version"))
            {
                CheckForUpdates ();
            }
        }

        // Start the game ---------------------------------------------------------------------------------------------------------
        private void RunGame()
        {
            // Executable to start
            ProcessStartInfo startInfo = new ProcessStartInfo(gameExecutablePath);
            startInfo.WorkingDirectory = Path.Combine(rootPath, "Data"); // Working directory of the process

            try
            {
                Process.Start(startInfo); 
            }
            catch (Exception ex)
            {
                ShowExceptions.Singleton.ShowError(ex.Message, ex.ToString());
                State = LauncherStates.failed;
            }

            Application.Quit(); // Close the launcher when the game starts
        }

        // Toggle the Patch Notes screen --------------------------------------------------------------------------------------
        public void ToggleChangelog (bool active)
        {
            changelogScreen.SetActive(active);
        }

        // Copy text to clipboard --------------------------------------------------------------------------------------------
        public static void CopyToClipboard (GameObject textUI)
        {
            GUIUtility.systemCopyBuffer = textUI.GetComponent<TMP_Text>().text;
        }

        // This function is called when the MonoBehaviour will be destroyed --------------------------------------------------
        private void OnDestroy()
        {
            // Remove subscription from the fetch completed callback when the GameObject is destroyed
            RemoteConfigService.Instance.FetchCompleted -= ApplyRemoteSettings;
        }
    }


    // A struct separating all the elements of the version string -------------------------------------------------------------------
    struct Version
    {

        // Default values
        internal static Version zero = new Version(0, 0, 0);

        // TODO: Download the minor releases from the game itself... launcher should only be used for major releases
        private short major; // Major releases
        private short minor; // Minor releases 
        private short subMinor; // Patches ~ no need to download anything by the launcher

        // Version constructor taking each part of the version separately
        internal Version(short _major, short _minor, short _subMinor)
        {
            major = _major;
            minor = _minor;
            subMinor = _subMinor;
        }

        // Version constructor taking the whole version string and splitting it
        internal Version(string _version)
        {
            string[] _versionStrings = _version.Split('.');

            // If the string is not in the right format then just use default values
            if (_versionStrings.Length != 4)
            {
                major = 0;
                minor = 0;
                subMinor = 0;

                return;
            }
            
            // Try to set the variables to their respective values from the version string
            // If there are any errors, log them and set the values to their default
            try { major = short.Parse(_versionStrings[1]); }
            catch (Exception ex) 
            { 
                UnityEngine.Debug.Log("Error: " + ex.Message); 
                major = 0;
            }
            try { minor = short.Parse(_versionStrings[2]); }
            catch (Exception ex) 
            { 
                UnityEngine.Debug.Log("Error: " + ex.Message); 
                minor = 0;
            }
            try { subMinor = short.Parse(_versionStrings[3]); }
            catch (Exception ex) 
            { 
                UnityEngine.Debug.Log("Error: " + ex.Message); 
                subMinor = 0;
            }
        }

        // Function to check if two versions are the same or not
        // Ignores the sub-minor as it is unnecessary for the launcher
        internal bool IsDifferentThan(Version _otherVersion)
        {
            if (major != _otherVersion.major) return true;
            if (minor != _otherVersion.minor) return true;
            if (subMinor != _otherVersion.minor) return true;

            return false;
        }
        
        // Convert version object into a string
        public override string ToString()
        {
            return $"{major}.{minor}.{subMinor}";
        }

    }
}