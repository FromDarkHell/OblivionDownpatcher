using System;
using System.IO;
using AdonisUI.Controls;
using System.Windows.Input;
using OblivionDownpatcher.NoCD;
using OblivionDownpatcher.Downpatcher;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog; // This is done to avoid naming conficts, its real ugly :)

namespace OblivionDownpatcher {
    /// <summary>
    /// 
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        Oblivion gameInstance;

        public MainWindow() {
            InitializeComponent();
            try {
                gameInstance = new Oblivion(Properties.Settings.Default.gamePath, "Oblivion");
                GamePathBox.Text = Properties.Settings.Default.gamePath;
                foreach(string p in gameInstance.getPatches()) {
                    PatchBox.Items.Add(p);
                }
                PatchBox.SelectedIndex = 0;
            }
            catch(Exception ex) {
                var messageBox = new MessageBoxModel {
                    Text = ("Exception: " + ex.Message),
                    Caption = "Info",
                    Icon = MessageBoxImage.Warning,
                    IsSoundEnabled = true,
                    Buttons = new [] { MessageBoxButtons.Ok() }
                };
                MessageBox.Show(messageBox);
            }
        }

        public void UpdateVersionLabel() { 
            string patch = gameInstance.getCurrentPatch();
            VersionLabel.Content = patch;
            ApplyCDFix.IsEnabled = NoCDPatcher.CanApplyCDPatch(Path.Combine(gameInstance.gameDir.FullName, "Oblivion.exe"));
        }

        #region Events

        private void GamePath_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            string lastCorrectDir = Properties.Settings.Default.gamePath;
            try {
                gameInstance.setCurrentFilePath(GamePathBox.Text);
                UpdateVersionLabel();
                Properties.Settings.Default.gamePath = GamePathBox.Text;
                Properties.Settings.Default.Save();
            }
            catch(Exception) {
                VersionLabel.Content = "Unknown";

                Properties.Settings.Default.gamePath = lastCorrectDir;
                Properties.Settings.Default.Save();
                try { 
                    gameInstance.setCurrentFilePath(GamePathBox.Text);
                    UpdateVersionLabel();
                }
                catch(Exception) {
                    Properties.Settings.Default.gamePath = @"C:\";
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void ApplyCDFix_Click(object sender, System.Windows.RoutedEventArgs e) {
            Mouse.OverrideCursor = Cursors.Wait;

            NoCDPatcher.ApplyCDPatch(Path.Combine(gameInstance.gameDir.FullName, "Oblivion.exe"));
            gameInstance.setCurrentFilePath(GamePathBox.Text);
            UpdateVersionLabel();

            Mouse.OverrideCursor = null;
        }
        
        private void PatchButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            Mouse.OverrideCursor = Cursors.Wait;

            gameInstance.setCurrentPatch(((string)PatchBox.SelectedItem));
            UpdateVersionLabel();

            Mouse.OverrideCursor = null;

            string patch = gameInstance.getCurrentPatch();
            bool askForNoCD = (NoCDPatcher.CanApplyCDPatch(Path.Combine(gameInstance.gameDir.FullName, "Oblivion.exe")) && !patch.EndsWith(" (No CD)"));
            if (askForNoCD) {
                var messageBox = new MessageBoxModel {
                    Text = "Patching complete!\nDo you want to apply the No CD edit?",
                    Caption = "Info",
                    Icon = MessageBoxImage.Question,
                    IsSoundEnabled = true,
                    Buttons = MessageBoxButtons.YesNo()
                };
                var result = MessageBox.Show(messageBox);
                if(result == MessageBoxResult.Yes) // Click the button if the user said yes
                    ApplyCDFix_Click(null, new System.Windows.RoutedEventArgs());
            }
            else {
                var messageBox = new MessageBoxModel {
                    Text = "Patching complete!",
                    Caption = "Info",
                    Icon = MessageBoxImage.Information,
                    IsSoundEnabled = true,
                    Buttons = new[] { MessageBoxButtons.Ok() }
                };
                MessageBox.Show(messageBox);
            }

            gameInstance.ApplyINIFix(); // Modify the INI to write to the patched directory.
        }

        private void Info_Click(object sender, System.Windows.RoutedEventArgs e) {
            var messageBox = new MessageBoxModel {
                Text = "Made by: FromDarkHell\nSee https://fromdarkhell.github.io/Oblivion for more info",
                Caption = "Info",
                Icon = MessageBoxImage.Information,
                Buttons = new[] { MessageBoxButtons.Ok() }
            };
            MessageBox.Show(messageBox);

        }

        private void GamePathBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                dialog.ShowNewFolderButton = false;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK) {
                    GamePathBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void HelpButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            string hashLocation = "";
            var patch = (string)PatchBox.SelectedItem;
            switch (patch) {
                case "1.1.425 (Beta)":
                    hashLocation = "#Version_1.1.425_.28Beta.29";
                    break;
                case "1.1.511":
                    hashLocation = "#Version_1.1.511";
                    break;
            }
            System.Diagnostics.Process.Start("https://en.uesp.net/wiki/Oblivion:Patch" + hashLocation);
        }

        #endregion
    }
}
