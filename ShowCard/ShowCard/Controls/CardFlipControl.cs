using ShowCard.Services;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

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

    public CardFlipControl(ILogService log)
    {
        _log = log;
        SizeMode = PictureBoxSizeMode.Zoom;
        Click += (s, e) => Flip();
    }

    public void ShowBack()
    {
        IsFrontVisible = false;
        Image = BackImage;
        _log.Info("Card flipped to back.");
    }

    public void ShowFront()
    {
        IsFrontVisible = true;
        Image = FrontImage;
        _log.Info($"Card flipped to front::{Text}");
    }

    public void Flip()
    {
        // TODO: replace with true 3D Viewport3D flip via WPF host.
        FlipStarted?.Invoke();
        if (IsFrontVisible)
            ShowBack();
        else
            ShowFront();
        FlipCompleted?.Invoke();
    }
}
