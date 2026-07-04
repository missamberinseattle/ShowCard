using ShowCard.Controls;
using ShowCard.Enums;
using ShowCard.Models;
using ShowCard.Services;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace ShowCard.Forms;

public partial class CardViewForm : Form
{
    private readonly IAppStateService _stateService;
    private readonly ILogService _log;

    private readonly CardFlipControl _suspectCard;
    private readonly CardFlipControl _weaponCard;
    private readonly CardFlipControl _locationCard;

    private BlackoutBackgroundForm? _blackoutForm;

    [DllImport("kernel32.dll")]
    private static extern uint SetThreadExecutionState(uint esFlags);
    private const uint ES_CONTINUOUS = 0x80000000;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;
    private const uint ES_SYSTEM_REQUIRED = 0x00000001;

    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(
    IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    private const int LWA_ALPHA = 0x2;

    private const byte fadeStep = 10;
    private const byte minAlpha = 10;
    private const byte maxAlpha = 245;

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

        Resize += (_, __) =>
        {
            LayoutCards();

            if (_blackoutForm != null)
                _blackoutForm.Bounds = this.Bounds;
        };

        Shown += (_, __) =>
        {
            SetLayeredWindowAttributes(Handle, 0, 255, LWA_ALPHA);
            CreateBlackoutBackground();
            ApplyWallpaper();
        };

        Load += (_, __) =>
        {
            LayoutCards();
            ShowBacks();
        };
    }

    private void CreateBlackoutBackground()
    {
        // Create only once
        if (_blackoutForm != null)
            return;

        _blackoutForm = new BlackoutBackgroundForm(Bounds);

        // Show behind this form
        _blackoutForm.Show();
        _blackoutForm.SendToBack();

        // Ensure CardViewForm stays above it
        this.BringToFront();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            const int WS_EX_LAYERED = 0x80000;
            var cp = base.CreateParams;
            cp.ExStyle |= WS_EX_LAYERED;
            return cp;
        }
    }

    public LightState LightState { get; private set; } = LightState.Lit;

    public void FadeOut()
    {
        if (LightState == LightState.Dark)
            return;

        LightState = LightState.Dark;

        byte alpha = 255;
        var timer = new Timer { Interval = 15 };

        timer.Tick += (s, e) =>
        {
            alpha -= fadeStep;
            _log.Info($"Fading out... Current alpha: {alpha}");

            if (alpha <= minAlpha)
            {
                alpha = 0;
                SetLayeredWindowAttributes(Handle, 0, alpha, LWA_ALPHA);
                timer.Stop();
                timer.Dispose();
                return;
            }

            SetLayeredWindowAttributes(Handle, 0, alpha, LWA_ALPHA);
        };

        timer.Start();
    }


    public void FadeIn()
    {
        if (LightState == LightState.Lit)
            return;

        LightState = LightState.Lit;

        byte alpha = 0;
        var timer = new Timer { Interval = 15 };

        timer.Tick += (s, e) =>
        {
            alpha += fadeStep;
            _log.Info($"Fading in... Current alpha: {alpha}");

            if (alpha >= maxAlpha)
            {
                alpha = 255;
                SetLayeredWindowAttributes(Handle, 0, alpha, LWA_ALPHA);
                timer.Stop();
                timer.Dispose();
                return;
            }

            SetLayeredWindowAttributes(Handle, 0, alpha, LWA_ALPHA);
        };

        timer.Start();
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

        if (_blackoutForm != null)
        {
            _blackoutForm.Close();
            _blackoutForm = null;
        }
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

    public void RevealSuspect() => _suspectCard.Flip(CardDisplay.Revealed);
    public void RevealWeapon() => _weaponCard.Flip(CardDisplay.Revealed);
    public void RevealLocation() => _locationCard.Flip(CardDisplay.Revealed);

    public void HideSuspect() => _suspectCard.Flip(CardDisplay.Hidden);
    public void HideWeapon() => _weaponCard.Flip(CardDisplay.Hidden);
    public void HideLocation() => _locationCard.Flip(CardDisplay.Hidden);

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
