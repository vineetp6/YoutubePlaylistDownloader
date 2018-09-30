﻿using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YoutubeExplode;
using YoutubeExplode.Models;

namespace YoutubePlaylistDownloader
{
    static class GlobalConsts
    {
        //The const variables are variables that can be accessed from all over the solution.
        #region Const Variables
        public static Skeleton Current;
        public static AppTheme Theme;
        public static Accent Accent;
        public static Brush ErrorBrush;
        public static readonly WebClient WebClient;
        public static string Language;
        public static readonly string TempFolderPath;
        public static string SaveDirectory;
        public static readonly string CurrentDir;
        private static readonly string ConfigFilePath;
        private static readonly string ErrorFilePath;
        public const double VERSION = 1.304;
        public static bool UpdateOnExit;
        public static string UpdateSetupLocation;
        public static bool OptionExpanderIsExpanded;
        public static bool UpdateFinishedDownloading;
        public static bool UpdateLater;
        public static DownloadUpdate UpdateControl;
        public static readonly string ChannelSubscriptionsFilePath;
        public static readonly YoutubeClient YoutubeClient;
        private static bool checkForSubscriptionUpdates;
        public static bool CheckForProgramUpdates;
        public static TimeSpan SubscriptionsUpdateDelay;

        public static bool CheckForSubscriptionUpdates
        {
            get => checkForSubscriptionUpdates;
            set
            {
                checkForSubscriptionUpdates = value;
                if (checkForSubscriptionUpdates)
                    SubscriptionManager.UpdateAllSubscriptions();
                else
                    SubscriptionManager.CancelAll();
            }

        }
        public static AppTheme Opposite { get { return Theme.Name == "BaseLight" ? ThemeManager.GetAppTheme("BaseDark") : ThemeManager.GetAppTheme("BaseLight"); } }

        #endregion

        static GlobalConsts()
        {
            CurrentDir = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString();
            string appDataPath = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\Youtube Playlist Downloader\\");
            ConfigFilePath = string.Concat(appDataPath, "Settings.json");
            ErrorFilePath = string.Concat(appDataPath, "Errors.txt");
            ChannelSubscriptionsFilePath = string.Concat(appDataPath, "Subscriptions.ypds");

            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            ErrorBrush = Brushes.Crimson;
            Language = "English";
            SaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            TempFolderPath = string.Concat(Path.GetTempPath(), "YoutubePlaylistDownloader\\");
            UpdateOnExit = false;
            UpdateLater = false;
            UpdateSetupLocation = string.Empty;
            WebClient = new WebClient
            {
                CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore)
            };
            YoutubeClient = new YoutubeClient();
            SubscriptionsUpdateDelay = TimeSpan.FromMinutes(1);
            checkForSubscriptionUpdates = false;
        }

        //The const methods are used mainly for saving/loading consts, and handling page\menu management.
        #region Const Methods

        #region Buttons
        public static void HideSubscriptionsButton()
        {
            Current.SubscriptionsButton.Visibility = Visibility.Collapsed;
        }
        public static void HideHelpButton()
        {
            Current.HelpButton.Visibility = Visibility.Collapsed;
        }
        public static void HideHomeButton()
        {
            Current.HomeButton.Visibility = Visibility.Collapsed;
        }
        public static void HideAboutButton()
        {
            Current.AboutButton.Visibility = Visibility.Collapsed;
        }
        public static void HideSettingsButton()
        {
            Current.SettingsButton.Visibility = Visibility.Collapsed;
        }
        public static void ShowSettingsButton()
        {
            Current.SettingsButton.Visibility = Visibility.Visible;
        }
        public static void ShowHelpButton()
        {
            Current.HelpButton.Visibility = Visibility.Visible;
        }
        public static void ShowAboutButton()
        {
            Current.AboutButton.Visibility = Visibility.Visible;
        }
        public static void ShowHomeButton()
        {
            Current.HomeButton.Visibility = Visibility.Visible;
        }
        public static void ShowSubscriptionsButton()
        {
            Current.SubscriptionsButton.Visibility = Visibility.Visible;
        }
        #endregion

        public static async Task ShowMessage(string title, string message)
        {
            if (Current.DefaultFlyout.IsOpen)
                Current.DefaultFlyout.IsOpen = false;
            await Current.ShowMessage(title, message).ConfigureAwait(false);
        }
        public static async Task<MessageDialogResult> ShowYesNoDialog(string title, string message)
        {
            if (Current.DefaultFlyout.IsOpen)
                Current.DefaultFlyout.IsOpen = false;
            return await Current.ShowYesNoDialog(title, message).ConfigureAwait(false);
        }
        public static void LoadPage(UserControl page) => Current.CurrentPage.Content = page;
        public static void SaveConsts()
        {
            try
            {
                var settings = new Objects.Settings(Theme.Name, Accent.Name, Language, SaveDirectory, OptionExpanderIsExpanded, CheckForSubscriptionUpdates, CheckForProgramUpdates, SubscriptionsUpdateDelay);
                File.WriteAllText(ConfigFilePath, Newtonsoft.Json.JsonConvert.SerializeObject(settings));
                SubscriptionManager.SaveSubscriptions();
            }
            catch { }
        }
        public static void RestoreDefualts()
        {
            Theme = ThemeManager.GetAppTheme("BaseDark");
            Accent = ThemeManager.GetAccent("Red");
            Language = "English";
            OptionExpanderIsExpanded = true;
            checkForSubscriptionUpdates = false;
            CheckForProgramUpdates = true;
            SubscriptionsUpdateDelay = TimeSpan.FromMinutes(1);

            SaveConsts();
        }
        public static void LoadConsts()
        {

            if (!File.Exists(ConfigFilePath))
            {
                RestoreDefualts();
                return;
            }

            try
            {
                var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Objects.Settings>(File.ReadAllText(ConfigFilePath));
                Theme = ThemeManager.GetAppTheme(settings.Theme);
                Accent = ThemeManager.GetAccent(settings.Accent);
                Language = settings.Language;
                SaveDirectory = settings.SaveDirectory;
                OptionExpanderIsExpanded = settings.OptionExpanderIsExpanded;
                CheckForSubscriptionUpdates = settings.CheckForSubscriptionUpdates;
                CheckForProgramUpdates = settings.CheckForProgramUpdates;
                SubscriptionsUpdateDelay = settings.SubscriptionsDelay;
            }
            catch
            {
                RestoreDefualts();
            }
            UpdateTheme();
            UpdateLanguage();

        }
        public static void CreateTempFolder()
        {
            if (!Directory.Exists(Path.GetTempPath() + "YoutubePlaylistDownloader"))
                Directory.CreateDirectory(Path.GetTempPath() + "YoutubePlaylistDownloader");
            
        }
        public static void CleanTempFolder()
        {
            if (Directory.Exists(Path.GetTempPath() + "YoutubePlaylistDownloader"))
            {
                DirectoryInfo di = new DirectoryInfo(Path.GetTempPath() + "YoutubePlaylistDownloader");

                foreach (FileInfo file in di.GetFiles())
                    try { file.Delete(); } catch { };

                foreach (DirectoryInfo dir in di.GetDirectories())
                    try { dir.Delete(true); } catch { };
            }
        }
        private static void UpdateTheme()
        {
            ThemeManager.ChangeAppStyle(Application.Current, Accent, Opposite);
            ThemeManager.ChangeAppTheme(Application.Current, Theme.Name);
        }
        private static void UpdateLanguage()
        {
            ResourceDictionary toRemove = Application.Current.Resources.MergedDictionaries.First(x => x.Source.OriginalString.Contains("English"));
            ResourceDictionary r = new ResourceDictionary()
            {
                Source = new Uri($"/Languages/{Language}.xaml", UriKind.Relative)
            };
            Application.Current.Resources.MergedDictionaries.Add(r);
            Application.Current.Resources.MergedDictionaries.Remove(toRemove);
        }
        public static void ChangeLanguage(string nLang)
        {
            ResourceDictionary toRemove = Application.Current.Resources.MergedDictionaries.First(x => x.Source.OriginalString.Contains(Language));
            ResourceDictionary r = new ResourceDictionary()
            {
                Source = new Uri($"/Languages/{nLang}.xaml", UriKind.Relative)
            };
            Application.Current.Resources.MergedDictionaries.Add(r);
            Application.Current.Resources.MergedDictionaries.Remove(toRemove);
            Language = nLang;
        }
        public static async Task Log(string message, object sender)
        {
            using (StreamWriter sw = new StreamWriter(ErrorFilePath, true))
            {
                await sw.WriteLineAsync($"[{DateTime.Now.ToUniversalTime()}], [{sender}]:\n\n{message}\n\n");
            }
        }
        public static string CleanFileName(string filename)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = string.Format(@"[{0}]+", invalidChars);

            var reservedWords = new[]
            {
                "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
                "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
                "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
            };

            var sanitisedNamePart = Regex.Replace(filename, invalidReStr, "_");
            foreach (var reservedWord in reservedWords)
            {
                var reservedWordPattern = string.Format("^{0}\\.", reservedWord);
                sanitisedNamePart = Regex.Replace(sanitisedNamePart, reservedWordPattern, "_reservedWord_.", RegexOptions.IgnoreCase);
            }

            return sanitisedNamePart;
        }
        public static async Task TagFile(Video video, int vIndex, string file, Playlist playlist = null)
        {
            var genre = video.Title.Split('[', ']').ElementAtOrDefault(1);


            if (genre == null)
                genre = string.Empty;

            else if (genre.Length >= video.Title.Length)
                genre = string.Empty;


            var title = video.Title;

            if (!string.IsNullOrWhiteSpace(genre))
            {
                title = video.Title.Replace($"[{genre}]", string.Empty);
                var rm = title.Split('[', ']', '【', '】').ElementAtOrDefault(1);
                if (!string.IsNullOrWhiteSpace(rm))
                    title = title.Replace($"[{rm}]", string.Empty);
            }
            title = title.TrimStart(' ', '-', '[', ']');

            var t = TagLib.File.Create(file);

            t.Tag.Album = playlist?.Title;
            t.Tag.Track = (uint)vIndex;
            t.Tag.Year = (uint)video.UploadDate.Year;
            t.Tag.DateTagged = video.UploadDate.UtcDateTime;
            t.Tag.AlbumArtists = new[] { playlist?.Author };
            var lowerGenre = genre.ToLower();
            if (new[] { "download", "out now", "mostercat", "video", "lyric", "release", "ncs" }.Any(x => lowerGenre.Contains(x)))
                genre = string.Empty;
            else
                t.Tag.Genres = genre.Split('/', '\\');
            
            try
            {
                TagLib.Id3v2.Tag.DefaultVersion = 3;
                TagLib.Id3v2.Tag.ForceDefaultVersion = true;
                var frame = TagLib.Id3v2.PopularimeterFrame.Get((TagLib.Id3v2.Tag)t.GetTag(TagLib.TagTypes.Id3v2, true), "WindowsUser", true);
                frame.Rating = Convert.ToByte((video.Statistics.LikeCount * 255) / (video.Statistics.LikeCount + video.Statistics.DislikeCount));
            }
            catch
            {

            }

            var index = title.LastIndexOf('-');
            if (index > 0)
            {
                var vTitle = title.Substring(index + 1).Trim(' ', '-');
                if (string.IsNullOrWhiteSpace(vTitle))
                {
                    index = title.IndexOf('-');
                    if (index > 0)
                        vTitle = title.Substring(index + 1).Trim(' ', '-');
                }
                t.Tag.Title = vTitle;
                t.Tag.Performers = title.Substring(0, index - 1).Trim().Split(new string[] { "&", "feat.", "feat", "ft.", " ft ", "Feat.", " x ", " X " }, StringSplitOptions.RemoveEmptyEntries);
            }

            try
            {
                var picLoc = $"{TempFolderPath}{CleanFileName(video.Title)}.jpg";
                using (var wb = new WebClient())
                    File.WriteAllBytes(picLoc, await wb.DownloadDataTaskAsync($"https://img.youtube.com/vi/{video.Id}/0.jpg").ConfigureAwait(false));

                t.Tag.Pictures = new TagLib.IPicture[] { new TagLib.Picture(picLoc) };
            }
            catch { }

            t.Save();
        }
        public static void LoadFlyoutPage(UserControl page)
        {
            Current.DefaultFlyoutUserControl.Content = page;
            Current.DefaultFlyout.IsOpen = true;
        }
        public static void CloseFlyout()
        {
            Current.DefaultFlyout.IsOpen = false;
            Current.DefaultFlyoutUserControl.Content = null;
        }
        public static double GetOffset()
        {
            return Current.ActualHeight - 120;
        }
        #endregion

    }
}
