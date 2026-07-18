using ShowCard.Enums;
using ShowCard.Models;
using ShowCard.Services;
using System.Diagnostics;

namespace ShowCard.Forms;

public class CardManagerForm : Form
{
    private readonly IAppStateService _stateService;
    private readonly ILogService _log;
    private readonly ICardRunService _runService;
    private readonly IAttractModeService _attract;
    private readonly CardViewForm _cardView;

    // Card entry controls
    private ComboBox _cardGroupCombo;
    private TextBox _cardTitleText;
    private TextBox _cardFacePathText;
    private TextBox _cardBackPathText;
    private Button _browseFaceButton;
    private Button _browseBackButton;
    private Button _addCardButton;

    // Current State View
    private Label _nextPerformerLabel;

    // Performer controls
    private TextBox _performerNameText;
    private NumericUpDown _performerOrderNumeric;
    private ComboBox _suspectCombo;
    private ComboBox _weaponCombo;
    private ComboBox _locationCombo;
    private Button _addPerformerButton;
    private ListBox _performerList;

    // Control buttons
    private Button _shuffleAssignButton;
    private Button _nextPerformerButton;
    private Button _revealSuspectButton;
    private Button _revealWeaponButton;
    private Button _revealLocationButton;
    private Button _revealAllButton;
    private Button _hideSuspectButton;
    private Button _hideWeaponButton;
    private Button _hideLocationButton;
    private Button _hideAllButton;
    private Button _startAttractButton;
    private Button _stopAttractButton;
    private Button _setWallpaperButton;
    private CheckBox _autoFlipCheckBox;
    private NumericUpDown _delayNumeric;
    private Button _exportAppStateButton;
    private Button _importAppStateButton;
    private Button _fadeButton;

    // Card List Controls
    private ListView _suspectListView;
    private ListView _weaponListView;
    private ListView _locationListView;


    // Log
    private TextBox _logTextBox;

    private Performer? _currentPerformer;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public CardManagerForm(
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        IAppStateService stateService,
        ILogService log,
        ICardRunService runService,
        IAttractModeService attract,
        CardViewForm cardView)
    {
        _stateService = stateService;
        _log = log;
        _runService = runService;
        _attract = attract;
        _cardView = cardView;

        Text = "ShowCard - Card Manager";
        StartPosition = FormStartPosition.Manual;
        Bounds = Screen.AllScreens[0].WorkingArea;

        InitializeControls();
        WireEvents();
        BindData();

        _log.LogMessage += msg =>
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AppendLog(msg)));
            }
            else
            {
                AppendLog(msg);
            }
        };
    }

    private void InitializeControls()
    {
        // Layout is minimal; adjust as needed.
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 10,
            AutoSize = true
        };
        Controls.Add(panel);

        _cardGroupCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        _cardGroupCombo.Items.AddRange(new[] { "Suspect", "Weapon", "Location" });
        _cardGroupCombo.SelectedIndex = 0;

        _cardTitleText = new TextBox();
        _cardFacePathText = new TextBox();
        _cardBackPathText = new TextBox();
        _browseFaceButton = new Button { Text = "Face..." };
        _browseBackButton = new Button { Text = "Back..." };
        _addCardButton = new Button { Text = "Add Card" };

        _performerNameText = new TextBox();
        _performerOrderNumeric = new NumericUpDown { Minimum = 0, Maximum = 999, Value = 1 };
        _suspectCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        _weaponCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        _locationCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        _addPerformerButton = new Button { Text = "Add Performer" };
        _performerList = new ListBox() { Height = 300, Width = 300 };


        _nextPerformerLabel = new Label
        {
            Text = "Next Performer: None",
            AutoSize = true,
            Font = new Font(
                "Segoe UI",          // Font family
                20f,                 // Font size
                FontStyle.Bold       // Style (Bold, Italic, Regular, etc.)
            ),
            ForeColor = Color.Red,   // Text color
            BackColor = Color.Transparent     // Optional
        };

        const int bottomButtonRowWidth = 120;

        _shuffleAssignButton = new Button { Text = "Shuffle & Assign", Width = bottomButtonRowWidth };
        _nextPerformerButton = new Button { Text = "Set Next Performer", Width = bottomButtonRowWidth };
        _fadeButton = new Button { Text = "Fade Out", Width = bottomButtonRowWidth };
        _revealSuspectButton = new Button { Text = "Reveal Suspect", Width = bottomButtonRowWidth };
        _revealWeaponButton = new Button { Text = "Reveal Weapon", Width = bottomButtonRowWidth };
        _revealLocationButton = new Button { Text = "Reveal Location", Width = bottomButtonRowWidth };
        _revealAllButton = new Button { Text = "Reveal All", Width = bottomButtonRowWidth };
        _hideSuspectButton = new Button { Text = "Hide Suspect", Width = bottomButtonRowWidth };
        _hideWeaponButton = new Button { Text = "Hide Weapon", Width = bottomButtonRowWidth };
        _hideLocationButton = new Button { Text = "Hide Location", Width = bottomButtonRowWidth };
        _hideAllButton = new Button { Text = "Hide All", Width = bottomButtonRowWidth };
        _startAttractButton = new Button { Text = "Start Attract", Width = bottomButtonRowWidth };
        _stopAttractButton = new Button { Text = "Stop Attract", Width = bottomButtonRowWidth };
        _setWallpaperButton = new Button { Text = "Set Wallpaper", Width = bottomButtonRowWidth };
        _autoFlipCheckBox = new CheckBox { Text = "Auto flip 3-card set", Width = bottomButtonRowWidth };
        _delayNumeric = new NumericUpDown { Minimum = 100, Maximum = 10000, Value = _stateService.State.RevealDelayMs, Increment = 100 };

        _exportAppStateButton = new Button { Text = "Export State", Width = bottomButtonRowWidth };
        _importAppStateButton = new Button { Text = "Import State", Width = bottomButtonRowWidth };

        _logTextBox = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };

        // Add to panel (rough layout)
        panel.Controls.Add(new Label { Text = "Card Group" }, 0, 0);
        panel.Controls.Add(_cardGroupCombo, 1, 0);
        panel.Controls.Add(new Label { Text = "Title" }, 0, 1);
        panel.Controls.Add(_cardTitleText, 1, 1);
        panel.Controls.Add(new Label { Text = "Face Path" }, 0, 2);
        panel.Controls.Add(_cardFacePathText, 1, 2);
        panel.Controls.Add(_browseFaceButton, 2, 2);
        panel.Controls.Add(new Label { Text = "Back Path" }, 0, 3);
        panel.Controls.Add(_cardBackPathText, 1, 3);
        panel.Controls.Add(_browseBackButton, 2, 3);
        panel.Controls.Add(_addCardButton, 1, 4);

        panel.Controls.Add(new Label { Text = "Performer Name" }, 0, 5);
        panel.Controls.Add(_performerNameText, 1, 5);
        panel.Controls.Add(new Label { Text = "Run Order" }, 0, 6);
        panel.Controls.Add(_performerOrderNumeric, 1, 6);
        panel.Controls.Add(new Label { Text = "Suspect" }, 0, 7);
        panel.Controls.Add(_suspectCombo, 1, 7);
        panel.Controls.Add(new Label { Text = "Weapon" }, 0, 8);
        panel.Controls.Add(_weaponCombo, 1, 8);
        panel.Controls.Add(new Label { Text = "Location" }, 0, 9);
        panel.Controls.Add(_locationCombo, 1, 9);
        panel.Controls.Add(_addPerformerButton, 2, 9);

        panel.Controls.Add(_performerList, 3, 0);
        panel.SetRowSpan(_performerList, 10);

        CreateUIPanel("ShowStatus", DockStyle.Bottom, new Control[] {
            _nextPerformerLabel
        });

        CreateUIPanel("AttractMode", DockStyle.Bottom, new Control[] {
            _startAttractButton,
            _stopAttractButton
        });

        CreateUIPanel("NextUp", DockStyle.Bottom, new Control[] {
            _nextPerformerButton
        });

        CreateUIPanel("CardViewControls", DockStyle.Bottom, new Control[] {
            _fadeButton
        });

        CreateUIPanel("RevealCards", DockStyle.Bottom, new Control[] {
            _revealAllButton,
            _revealSuspectButton,
            _revealWeaponButton,
            _revealLocationButton
        });

        CreateUIPanel("HideCards", DockStyle.Bottom, new Control[] {
            _hideAllButton,
            _hideSuspectButton,
            _hideWeaponButton,
            _hideLocationButton
        });

        CreateUIPanel("Setup", DockStyle.Bottom, new Control[] {
            _shuffleAssignButton,
            _setWallpaperButton,
            new Label{ Text="Delay (ms)"},
            _delayNumeric,
            _autoFlipCheckBox
        });

        CreateUIPanel("StateManagement", DockStyle.Bottom, new Control[] {
            _exportAppStateButton,
            _importAppStateButton
        });

        var logPanel = new Panel { Dock = DockStyle.Bottom, Height = 120 };
        logPanel.Controls.Add(_logTextBox);
        Controls.Add(logPanel);

        var cardListPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 350,
            ColumnCount = 1,
            RowCount = 3,
            AutoSize = true
        };

        _suspectListView = CreateCardListView("Suspects");
        _weaponListView = CreateCardListView("Weapons");
        _locationListView = CreateCardListView("Locations");

        cardListPanel.Controls.Add(_suspectListView);
        cardListPanel.Controls.Add(_weaponListView);
        cardListPanel.Controls.Add(_locationListView);

        Controls.Add(cardListPanel);

    }

    private FlowLayoutPanel CreateUIPanel(string name, DockStyle bottom, Control[] controls)
    {
        var panel = new FlowLayoutPanel { Name = name, Dock = bottom, AutoSize = true };
        panel.Controls.AddRange(controls);
        Controls.Add(panel);

        return panel;
    }

    private void WireEvents()
    {
        // The (_, __) => lambda is a shorthand for ignoring the sender and event args
        _browseFaceButton.Click += (_, __) => BrowseForPath(_cardFacePathText);
        _browseBackButton.Click += (_, __) => BrowseForPath(_cardBackPathText);

        _addCardButton.Click += (_, __) => AddCard();
        _addPerformerButton.Click += (_, __) => AddPerformer();

        _shuffleAssignButton.Click += (_, __) => ShuffleAssign();
        _nextPerformerButton.Click += (_, __) => SetNextPerformer();

        _importAppStateButton.Click += (_, __) =>
        {
            using var ofd = new OpenFileDialog
            {
                RestoreDirectory = true,

                InitialDirectory = Path.Combine(AppStateService.GetDropboxPath(), "Show Card States"),
                Filter = "JSON Files|*.json|All Files|*.*"
            };
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                _stateService.Load(ofd.FileName);
                BindData();
                _log.Info($"State imported from {ofd.FileName}.");
            }
        };

        _exportAppStateButton.Click += (_, __) =>
        {
            using var sfd = new SaveFileDialog
            {
                InitialDirectory = Path.Combine(AppStateService.GetDropboxPath(), "Show Card States"),
                Filter = "JSON Files|*.json|All Files|*.*",
                FileName = $"ShowCardState_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };
            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                _stateService.Save(sfd.FileName);
                _log.Info($"State exported to {sfd.FileName}.");
            }
        };

        _revealSuspectButton.Click += (_, __) => { EnsureLit(); _cardView.RevealSuspect(); _log.Info("Reveal suspect."); };
        _revealWeaponButton.Click += (_, __) => { EnsureLit(); _cardView.RevealWeapon(); _log.Info("Reveal weapon."); };
        _revealLocationButton.Click += (_, __) => { EnsureLit(); _cardView.RevealLocation(); _log.Info("Reveal location."); };
        _revealAllButton.Click += (_, __) => RevealAll();
        _hideSuspectButton.Click += (_, __) => { _cardView.HideSuspect(); _log.Info("Hide suspect."); };
        _hideWeaponButton.Click += (_, __) => { _cardView.HideWeapon(); _log.Info("Hide weapon."); };
        _hideLocationButton.Click += (_, __) => { _cardView.HideLocation(); _log.Info("Hide location."); };
        _hideAllButton.Click += (_, __) => HideAll();

        _startAttractButton.Click += (_, __) => StartAttract();
        _stopAttractButton.Click += (_, __) => StopAttract();

        _fadeButton.Click += (_, __) => ToggleFade();

        _setWallpaperButton.Click += (_, __) => SetWallpaper();

        _delayNumeric.ValueChanged += (_, __) =>
        {
            _stateService.State.RevealDelayMs = (int)_delayNumeric.Value;
            _stateService.Save();
        };

        _performerList.DoubleClick += (_, __) => SetNextPerformerFromPerformerList();

        FormClosing += (_, __) => _stateService.Save();
    }

    private void _performerList_DoubleClick(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private void BindData()
    {
        RefreshCardCombos();
        RefreshCardLists();
        _performerList.DataSource = _stateService.State.Performers;
        _performerList.SelectedIndex = -1;
    }

    private void RefreshCardCombos()
    {
        _suspectCombo.DataSource = null;
        _weaponCombo.DataSource = null;
        _locationCombo.DataSource = null;

        _suspectCombo.DataSource = _stateService.State.Suspects.ToList();
        _weaponCombo.DataSource = _stateService.State.Weapons.ToList();
        _locationCombo.DataSource = _stateService.State.Locations.ToList();
    }

    private void BrowseForPath(TextBox target)
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All Files|*.*"
        };
        if (ofd.ShowDialog(this) == DialogResult.OK)
        {
            target.Text = ofd.FileName;
            var parsed = ParseCardFileName(ofd.FileName);

            if (parsed.Type != "Card")
            {
                _cardTitleText.Text = parsed.Title;
                for (var xx = 0; xx < _cardGroupCombo.Items.Count; xx++)
                {
                    if (_cardGroupCombo.Items[xx]!.ToString() == parsed.Type)
                    {
                        _cardGroupCombo.SelectedIndex = xx;
                        break;
                    }
                }
            }
        }
    }

    private void AddCard()
    {
        var title = _cardTitleText.Text.Trim();
        if (string.IsNullOrEmpty(title))
        {
            MessageBox.Show(this, "Card title cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var card = new Card
        {
            Title = title,
            FaceImagePath = _cardFacePathText.Text.Trim(),
            BackImagePath = string.IsNullOrWhiteSpace(_cardBackPathText.Text)
                ? _stateService.State.LastBackImagePath
                : _cardBackPathText.Text.Trim()
        };

        if (!string.IsNullOrWhiteSpace(card.BackImagePath))
            _stateService.State.LastBackImagePath = card.BackImagePath;

        switch (_cardGroupCombo.SelectedItem?.ToString())
        {
            case "Suspect":
                _stateService.State.Suspects.Add(card);
                _performerNameText.Text = title; // Pre-fill performer name with suspect title for convenience
                break;
            case "Weapon":
                _stateService.State.Weapons.Add(card);
                break;
            case "Location":
                _stateService.State.Locations.Add(card);
                break;
        }

        _stateService.Save();
        RefreshCardCombos();
        RefreshCardLists();

        _log.Info($"Added card '{card.Title}' to {_cardGroupCombo.SelectedItem}.");

        _cardTitleText.Clear();
        _cardFacePathText.Clear();
        // back path left as last used
    }

    private void AddPerformer()
    {
        var name = _performerNameText.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            MessageBox.Show(this, "Performer name cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var performer = new Performer
        {
            Name = name,
            RunOrder = (int)_performerOrderNumeric.Value,
            Suspect = _suspectCombo.SelectedItem as Card,
            Weapon = _weaponCombo.SelectedItem as Card,
            Location = _locationCombo.SelectedItem as Card
        };

        _stateService.State.Performers.Add(performer);
        _stateService.State.Performers = _stateService.State.Performers
            .OrderBy(p => p.RunOrder).ToList();
        _performerList.DataSource = null;
        _performerList.DataSource = _stateService.State.Performers;

        _stateService.Save();
        _log.Info($"Added performer '{performer.Name}' with run order {performer.RunOrder}.");

        _performerOrderNumeric.Value = ((int)_performerOrderNumeric.Value) + 1;
    }

    private void ShuffleAssign()
    {
        _runService.ShuffleAndAssign(
            _stateService.State.Performers,
            _stateService.State.Suspects,
            _stateService.State.Weapons,
            _stateService.State.Locations);

        _stateService.Save();
        _performerList.DataSource = _stateService.State.Performers.ToList(); // Refresh list

        _log.Info("Shuffled suspects and assigned weapons/locations.");
    }

    public void SetNextPerformerFromPerformerList()
    {
        Performer selectedPerformer = (Performer)_performerList.SelectedItem!;
        SetNextPerformer(selectedPerformer.RunOrder);
    }

    public void SetNextPerformer(int runThisActNext = -1)
    {
        int currentOrder; ;

        if (runThisActNext == -1)
        {
            currentOrder = _currentPerformer?.RunOrder ?? -1;
        }
        else
        {
            currentOrder = runThisActNext - 1;
        }

        var next = _runService.GetNextPerformer(_stateService.State.Performers, currentOrder)
                       ?? _stateService.State.Performers.OrderBy(p => p.RunOrder).FirstOrDefault();

        if (next == null)
        {
            Debugger.Break();
        }

        if (runThisActNext != -1 && next!.RunOrder != runThisActNext)
        {
            throw new InvalidOperationException($"Expected to set performer with run order {runThisActNext}, but got {next.RunOrder}.");
        }

        _currentPerformer = next;
        _performerList.SelectedItem = next;
        _cardView.SetCards(next!.Suspect, next.Weapon, next.Location);
        _cardView.ShowBacks();
        _log.Info($"Next performer set: {next.Name}.");
        _nextPerformerLabel.Text = $"Next Performer: {next.Name}";

        if (_autoFlipCheckBox.Checked)
        {
            RevealAll();
        }
    }

    private void ToggleFade()
    {
        if (_cardView.LightState == LightState.Lit)
        {
            _cardView.FadeOut();
            _fadeButton.Text = "Fade In";
            _log.Info("Fade out triggered.");
        }
        else
        {
            _cardView.FadeIn();
            _fadeButton.Text = "Fade Out";
            _log.Info("Fade in triggered.");
        }
    }

    private void RevealAll()
    {
        if (_currentPerformer == null)
        {
            MessageBox.Show("Next performer is not set.", "Nothing to show", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _log.Warn("Next performer not set.");
            return;
        }

        EnsureLit();
        int delay = _stateService.State.RevealDelayMs;
        _cardView.RevealAllSequential(delay);
        _log.Info("Reveal all cards sequence.");
    }

    private void EnsureLit()
    {
        if (_cardView.LightState == LightState.Dark)
        {
            _cardView.FadeIn();
            _fadeButton.Text = "Fade Out";
        }
    }

    private void HideAll()
    {
        int delay = _stateService.State.RevealDelayMs;
        _cardView.HideAllSequential(delay);
        _log.Info("Hide all cards sequence.");
    }

    private void StopAttract()
    {
        if (MessageBox.Show(this, "Stop attract mode?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
        {
            return;
        }

        _attract.Stop();
        _log.Info("Attract mode stopped.");
    }

    private void StartAttract()
    {
        if (_performerList.Items.Count == 0)
        {
            MessageBox.Show(this, "No performers available for attract mode.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (MessageBox.Show(this, "Start attract mode? This will continuously cycle through performers and reveal cards.", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
        {
            return;
        }

        EnsureLit();

        _attract.Start(
            _stateService.State,
            showSequence: (suspect, weapon, location) =>
            {
                _cardView.SetCards(suspect, weapon, location);
                _cardView.RevealAllSequential(_stateService.State.RevealDelayMs);
            },
            hideSequence: () =>
            {
                _cardView.HideAllSequential(_stateService.State.RevealDelayMs);
            });
    }

    private void SetWallpaper()
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All Files|*.*"
        };
        if (ofd.ShowDialog(this) == DialogResult.OK)
        {
            _stateService.State.WallpaperPath = ofd.FileName;
            _stateService.Save();
            _cardView.Invoke(new Action(() => _cardView.Refresh()));
            _log.Info($"Wallpaper set: {ofd.FileName}");
        }
    }

    private void AppendLog(string message)
    {
        if (!_logTextBox.IsDisposed)
        {
            _logTextBox.AppendText(message + Environment.NewLine);
        }
    }

    private (string Type, string Title) ParseCardFileName(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return ("Unknown", "Unknown");
        }

        var file = Path.GetFileNameWithoutExtension(filePath);

        // Pattern: Type-Name[-Index]
        // Examples:
        //   Weapon-Knife
        //   Suspect-MissVioletDeVille-02
        //   Location-BoxOffice-03

        var parts = file.Split('-');
        if (parts.Length < 2)
            return ("Unknown", file);

        string type = parts[0];
        string namePart = string.Join("-", parts.Skip(1));

        // Remove trailing index if present
        if (int.TryParse(namePart.Split('-').Last(), out _))
        {
            namePart = string.Join("-", namePart.Split('-').Reverse().Skip(1).Reverse());
        }

        // Convert CamelCase or PascalCase into spaced words
        string title = System.Text.RegularExpressions.Regex
            .Replace(namePart, "([a-z])([A-Z])", "$1 $2");

        return (type, title);
    }

    private ListView CreateCardListView(string header)
    {
        var lv = new ListView
        {
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Height = Height / 4,
            Width = 470,
            Dock = DockStyle.Top
        };

        lv.Columns.Add(header, 270);
        lv.Columns.Add("File", 200);

        return lv;
    }

    private void RefreshCardLists()
    {
        _suspectListView.Items.Clear();
        _weaponListView.Items.Clear();
        _locationListView.Items.Clear();

        foreach (var card in _stateService.State.Suspects)
            AddCardToList(_suspectListView, card);

        foreach (var card in _stateService.State.Weapons)
            AddCardToList(_weaponListView, card);

        foreach (var card in _stateService.State.Locations)
            AddCardToList(_locationListView, card);
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        // 
        // CardManagerForm
        // 
        ClientSize = new Size(978, 842);
        Name = "CardManagerForm";
        ResumeLayout(false);

    }

    private void AddCardToList(ListView lv, Card card)
    {
        var parsed = ParseCardFileName(card.FaceImagePath);
        var item = new ListViewItem(card.Title);
        item.SubItems.Add(Path.GetFileNameWithoutExtension(card.FaceImagePath));
        // item.SubItems.Add(Path.GetFileName(card.FaceImagePath));
        lv.Items.Add(item);
    }

}
