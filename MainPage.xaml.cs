using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MusicLibrary.Model;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MusicLibrary
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Song> songs;
        private List<MenuItem> menuItems;
        private MediaPlayer player;
        private PlaylistManager playlistManager;
        private ObservableCollection<Playlist> playlists;
        private AppBarButton currentPlayControl;
        private AppBarButton currentPauseControl;
        private AppBarButton currentStopControl;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            player = new MediaPlayer();
            songs = new ObservableCollection<Song>();
            playlistManager = new PlaylistManager();
            playlists = new ObservableCollection<Playlist>(playlistManager.Playlists.Values);
            SongManager.GetAllSongs(songs);
            menuItems = new List<MenuItem>();

            //Load Pane
            menuItems.Add(new MenuItem
            {
                IconFile = "Assets/Icons/adele.png",
                Category = SongCategory.Adele
            });
            menuItems.Add(new MenuItem
            {
                IconFile = "Assets/Icons/Justin.png",
                Category = SongCategory.Justin
            });
            menuItems.Add(new MenuItem
            {
                IconFile = "Assets/Icons/sia.png",
                Category = SongCategory.Sia
            });
            menuItems.Add(new MenuItem
            {
                IconFile = "Assets/Icons/taylor.png",
                Category = SongCategory.Taylor
            });

        }

        //Start mark rating 
        private void RatingChanged(RatingControl sender, object args)
        {

        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;

        }

        private void MenuItemsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var menuItem = (MenuItem)e.ClickedItem;
            CategoryTextBlock.Text = menuItem.Category.ToString();
            SongManager.GetSongsByCategory(songs, menuItem.Category);
            BackButton.Visibility = Visibility.Visible;
        }

        private void SongGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var song = (Song)e.ClickedItem;
            MyMediaElement.Source = new Uri(this.BaseUri, song.AudioFile);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SongManager.GetAllSongs(songs);
            CategoryTextBlock.Text = "All Songs";
            MenuItemsListView.SelectedItem = null;
            BackButton.Visibility = Visibility.Collapsed;
        }

        //Switch to Local Music Page(Page2)
        private void HyperLinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LocalMusic), yourName.Text);
        }

        private void FavoriteList_Click(object sender, ItemClickEventArgs e)
        {
            Playlist playlist = (Playlist)e.ClickedItem;
            songs.Clear();
            SongManager.GetSongsByPlaylist(playlist.Name, songs);
            CategoryTextBlock.Text = "Play List";
            MenuItemsListView.SelectedItem = null;
            BackButton.Visibility = Visibility.Visible;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Song clickedOnSong = ((Button)sender).DataContext as Song;
            if (clickedOnSong.IsPaused)
            {
                clickedOnSong.IsPaused = false;
            }
            else
            {
                Windows.Storage.StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync(@"Assets");
                Windows.Storage.StorageFile audioFile = await folder.GetFileAsync(clickedOnSong.AudioFile);
                player.AutoPlay = false;
                player.Source = MediaSource.CreateFromStorageFile(audioFile);
                
                if (currentPlayControl != null)
                {
                    currentPlayControl.Visibility = Visibility.Visible;
                }

                if (currentPauseControl != null)
                {
                    currentPauseControl.Visibility = Visibility.Collapsed;
                }

                if (currentStopControl != null)
                {
                    currentStopControl.Visibility = Visibility.Collapsed;
                }
            }

            var buttons = ((StackPanel)((AppBarButton)sender).Parent).Children;
            foreach (var button in buttons)
            {
                if (((AppBarButton)button).Name == "Play")
                {
                    ((AppBarButton)button).Visibility = Visibility.Collapsed;
                    this.currentPlayControl = (AppBarButton)button;
                }
                else
                {
                    ((AppBarButton)button).Visibility = Visibility.Visible;
                    if (((AppBarButton)button).Name == "Pause")
                    {
                        this.currentPauseControl = (AppBarButton)button;
                    }
                    else
                    {
                        this.currentStopControl = (AppBarButton)button;
                    }
                }
            }

            clickedOnSong.IsPlaying = true;
            player.Play();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            Song clickedOnSong = ((Button)sender).DataContext as Song;
            clickedOnSong.IsPaused = true;

            var buttons = ((StackPanel)((AppBarButton)sender).Parent).Children;
            foreach (var button in buttons)
            {
                if (((AppBarButton)button).Name == "Pause")
                {
                    ((AppBarButton)button).Visibility = Visibility.Collapsed;
                }
                else
                {
                    ((AppBarButton)button).Visibility = Visibility.Visible;
                }
            }

            player.Pause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Song clickedOnSong = ((Button)sender).DataContext as Song;
            player.Source = null;

            var buttons = ((StackPanel)((AppBarButton)sender).Parent).Children;
            foreach (var button in buttons)
            {
                if (((AppBarButton)button).Name == "Play")
                {
                    ((AppBarButton)button).Visibility = Visibility.Visible;
                }
                else
                {
                    ((AppBarButton)button).Visibility = Visibility.Collapsed;
                }
            }
        }

        private void AddToPlaylist_Click(object sender, ItemClickEventArgs e)
        {
            Playlist playlist = (Playlist)e.ClickedItem;
            var allSongs = new List<Song>(songs);
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            foreach (var song in allSongs)
            {
                if (song.SelectedForPlaylist)
                {
                    localSettings.Values[playlist.Name + song.Name] = song.SelectedForPlaylist;
                }

                song.SelectedForPlaylist = false;
            }
        }
    }
}