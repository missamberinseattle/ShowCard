using ShowCard.Models;
using ShowCard.Services;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace ShowCard.Controls;

public class CardFlipControl : PictureBox
{
    private readonly ILogService _log;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image? FrontImage { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image? BackImage { get; set; }

    public bool IsFrontVisible { get; private set; }

    public event Action? FlipStarted;
    public event Action? FlipCompleted;

    private Timer _flipTimer;
    private int _flipStep;
    private const int TotalSteps = 20;
    private bool _isFlipping;

    private double _currentScale = 1.0;

    public CardFlipControl(ILogService log)
    {
        _log = log;

        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.SupportsTransparentBackColor, true);

        BackColor = Color.Transparent;
        SizeMode = PictureBoxSizeMode.Zoom;

        Click += (s, e) => Flip();

        _flipTimer = new Timer();
        _flipTimer.Interval = 15;
        _flipTimer.Tick += FlipTick;
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // Suppress background painting to avoid white flash
    }

    public void ShowBack()
    {
        IsFrontVisible = false;
        Image = BackImage;
        _log.Info($"{Text} flipped to back.");
        Invalidate();
    }

    public void ShowFront()
    {
        IsFrontVisible = true;
        Image = FrontImage;
        _log.Info($"{Text} flipped to front::{Text}");
        Invalidate();
    }

    public void Flip(CardDisplay revealed = CardDisplay.NotSet)
    {
        if (_isFlipping)
        {
            return;
        }

        if (revealed == CardDisplay.Revealed && IsFrontVisible || 
            revealed == CardDisplay.Hidden && !IsFrontVisible)
        {
            return;
        }

        _isFlipping = true;
        _flipStep = 0;
        FlipStarted?.Invoke();
        _flipTimer.Start();
    }

    private void FlipTick(object? sender, EventArgs e)
    {
        _flipStep++;

        if (_flipStep <= TotalSteps / 2)
        {
            double scale = 1.0 - (_flipStep / (double)(TotalSteps / 2));
            _currentScale = Math.Max(0.0, scale);
        }
        else
        {
            if (_flipStep == (TotalSteps / 2) + 1)
            {
                if (IsFrontVisible)
                    ShowBack();
                else
                    ShowFront();
            }

            double scale = (_flipStep - (TotalSteps / 2)) / (double)(TotalSteps / 2);
            _currentScale = Math.Min(1.0, scale);
        }

        Invalidate();

        if (_flipStep >= TotalSteps)
        {
            _flipTimer.Stop();
            _isFlipping = false;
            FlipCompleted?.Invoke();
        }
    }

    protected override void OnPaint(PaintEventArgs pe)
    {
        if (Image == null)
            return;

        var g = pe.Graphics;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        int scaledWidth = (int)(Width * _currentScale);
        int x = (Width - scaledWidth) / 2;

        g.DrawImage(Image, new Rectangle(x, 0, scaledWidth, Height));
    }


}
