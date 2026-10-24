using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using KeePass.UI;

namespace RookieUI
{
  public class CollapsibleGroupBox : GroupBox
  {
    private int _expandedHeight;
    public int CollapsedSize = 20;
    private int HeaderHeight
    {
      get { return Font.Height + CollapsedSize; }
    }
    private List<Control> lControlsVisible = new List<Control>();

    private string _sText = string.Empty;
    public string PlainText
    {
      get { return _sText; }
      set { _sText = value; }
    }

    private bool _collapsed = false;
    public bool Collapsed
    {
      get { return _collapsed; }
      set
      {
        _collapsed = value;
        UpdateState();
      }
    }

    private bool IsRightToLeftEffective
    {
      get
      {
        return RtlTranslateAlignment(ContentAlignment.MiddleLeft) == ContentAlignment.MiddleRight;
      }
    }

    public override string Text
    {
      get
      {
        string s = string.Empty;
        if (!Collapsed) s = "▼";
        else s = IsRightToLeftEffective ? "◀" : "▶";
        return s + " " + _sText;
      }
      set
      {
        _sText = value;
      }
    }

    public CollapsibleGroupBox()
    {
      DoubleBuffered = true;
      Resize += (s, e) =>
      {
        if (!_collapsed) _expandedHeight = Height;
      };
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
      base.OnControlAdded(e);
      if (!Collapsed || !e.Control.Visible) return;
      lControlsVisible.Add(e.Control);
      e.Control.Visible = false;
    }

    private bool _ControlCreated = false;
    protected override void OnCreateControl()
    {
      base.OnCreateControl();
      _ControlCreated = true;
      UpdateState();
    }

    protected override void OnClick(EventArgs e)
    {
      base.OnClick(e);

      //Clickable header
      var mouse = PointToClient(MousePosition);
      if (mouse.Y <= HeaderHeight)
      {
        Collapsed = !Collapsed;
      }
    }

    private void UpdateState()
    {
      if (!_ControlCreated) return;
      if (Collapsed)
      {
        _expandedHeight = Height;
        Height = HeaderHeight;

        lControlsVisible.Clear();

        foreach (Control c in Controls)
        {
          if (c.Visible) lControlsVisible.Add(c);
          c.Visible = false;
        }
      }
      else
      {
        Height = _expandedHeight;

        foreach (Control c in Controls)
        {
          if (lControlsVisible.Contains(c))
          {
            lControlsVisible.Remove(c);
            c.Visible = true;
          }
        }
      }

      Invalidate();
    }
  }
}
