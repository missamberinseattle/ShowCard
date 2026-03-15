using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ShowCard.Controls;
using ShowCard.Models;
using ShowCard.Services;
using Timer = System.Windows.Forms.Timer;

namespace ShowCard.Forms;

public partial class CardViewForm : Form
{
    private readonly IAppStateService _stateService;
    private readonly ILogService _log;

    private readonly CardFlipControl _suspectCard;
    private readonly CardFlipControl _weaponCard;
    private readonly CardFlipControl _locationCard;

    [DllImport("kernel32.dll")]
    private static extern uint SetThreadExecutionState(uint esFlags);
    private const uint ES_CONTINUOUS = 0x80000000;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;
    private const uint ES_SYSTEM_REQUIRED = 0x00000001;

    public CardViewForm(IAppStateService stateService, ILogService log)
    {
        _stateService = stateService;
        _log = log;

        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;

        var screens = Screen.AllScreens;
        var targetScreen = screens.Length > 1 ? screens[1] : screens[0];
        Bounds = targetScreen.Bounds;

        _suspectCard = new CardFlipControl(_log);
        _weaponCard = new CardFlipControl(_log);
        _locationCard = new CardFlipControl(_log);

        Controls.AddRange(new Control[] { _suspectCard, _weaponCard, _locationCard });

        Resize += (_, __) => LayoutCards();
        Shown += (_, __) => ApplyWallpaper();

        Load += (_, __) =>
        {
            LayoutCards();
            ShowBacks();
        };
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        PreventSleep(true);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        PreventSleep(false);
        base.OnFormClosing(e);
    }

    private void PreventSleep(bool enable)
    {
        if (enable)
            SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED);
        else
            SetThreadExecutionState(ES_CONTINUOUS);
    }

    private void LayoutCards()
    {
        int cardWidth = Width / 5;
        int cardHeight = (int)(cardWidth * 1.5);
        int centerY = Height / 2 - cardHeight / 2;
        int gutter = cardWidth / 4;

        _suspectCard.Bounds = new Rectangle(Width / 2 - cardWidth * 3 / 2 - gutter, centerY, cardWidth, cardHeight);
        _weaponCard.Bounds = new Rectangle(Width / 2 - cardWidth / 2, centerY, cardWidth, cardHeight);
        _locationCard.Bounds = new Rectangle(Width / 2 + cardWidth / 2 + gutter, centerY, cardWidth, cardHeight);

        // Apply rounded corners (10px radius)
        int radius = 10;
        _suspectCard.Region = CreateRoundedRegion(_suspectCard.ClientRectangle, radius);
        _weaponCard.Region = CreateRoundedRegion(_weaponCard.ClientRectangle, radius);
        _locationCard.Region = CreateRoundedRegion(_locationCard.ClientRectangle, radius);

        if (_stateService.State.LastBackImagePath != null)
        {
            _suspectCard.BackImage = Image.FromFile(_stateService.State.LastBackImagePath);
            _weaponCard.BackImage = Image.FromFile(_stateService.State.LastBackImagePath);
            _locationCard.BackImage = Image.FromFile(_stateService.State.LastBackImagePath);
        }
    }

    private void ApplyWallpaper()
    {
        var path = _stateService.State.WallpaperPath;

        if (string.IsNullOrWhiteSpace(path))
        {
            _log.Info("No wallpaper path set.");
            return;
        }

        if (!File.Exists(path))
        {
            _log.Info($"Wallpaper file not found: {path}");
            return;
        }

        try
        {
            BackgroundImage = Image.FromFile(path);
            BackgroundImageLayout = ImageLayout.Stretch;
            _log.Info($"Wallpaper applied: {path}");
        }
        catch (Exception ex)
        {
            _log.Info($"Failed to load wallpaper: {ex.Message}");
        }
    }

    public void SetCards(Card? suspect, Card? weapon, Card? location)
    {
        SetCardImages(_suspectCard, suspect);
        SetCardImages(_weaponCard, weapon);
        SetCardImages(_locationCard, location);
        ShowBacks();
    }

    private void SetCardImages(CardFlipControl control, Card? card)
    {
        if (card == null)
        {
            control.FrontImage = null;
            control.BackImage = null;
            control.Image = null;
            _log.Warn("Card is null, clearing images.");

            return;
        }

        control.Text = card.Title;

        if (File.Exists(card.FaceImagePath))
        {
            control.FrontImage = Image.FromFile(card.FaceImagePath);
        }
        else
        {
            _log.Warn($"Face image not found for card '{card.Title}': {card.FaceImagePath}");
            control.FrontImage = null;
        }

        if (File.Exists(card.BackImagePath))
        {
            control.BackImage = Image.FromFile(card.BackImagePath);
        }
        else
        {
            _log.Warn($"Back image not found for card '{card.Title}': {card.BackImagePath}");
            control.BackImage = null;
        }
    }

    public void ShowBacks()
    {
        _suspectCard.ShowBack();
        _weaponCard.ShowBack();
        _locationCard.ShowBack();
    }

    public void RevealSuspect() => _suspectCard.Flip();
    public void RevealWeapon() => _weaponCard.Flip();
    public void RevealLocation() => _locationCard.Flip();

    public void HideSuspect() => _suspectCard.Flip();
    public void HideWeapon() => _weaponCard.Flip();
    public void HideLocation() => _locationCard.Flip();

    public void RevealAllSequential(int delayMs)
    {
        var timer = new Timer { Interval = delayMs };
        int step = 0;
        timer.Tick += (s, e) =>
        {
            step++;
            switch (step)
            {
                case 1: RevealSuspect(); break;
                case 2: RevealWeapon(); break;
                case 3: RevealLocation(); break;
                default:
                    timer.Stop();
                    timer.Dispose();
                    break;
            }
        };
        timer.Start();
    }

    public void HideAllSequential(int delayMs)
    {
        var timer = new Timer { Interval = delayMs };
        int step = 0;
        timer.Tick += (s, e) =>
        {
            step++;
            switch (step)
            {
                case 1: HideSuspect(); break;
                case 2: HideWeapon(); break;
                case 3: HideLocation(); break;
                default:
                    timer.Stop();
                    timer.Dispose();
                    break;
            }
        };
        timer.Start();
    }

    private Region CreateRoundedRegion(Rectangle bounds, int radius)
    {
        int diameter = radius * 2;
        var path = new System.Drawing.Drawing2D.GraphicsPath();

        path.StartFigure();
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return new Region(path);
    }

}
