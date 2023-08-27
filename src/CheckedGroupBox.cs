using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RookieUI
{
  public class CheckedGroupCheckEventArgs : EventArgs
  {
    public bool GroupChecked { get; private set; }
    public bool BeforeDefaultHandling { get; private set; }
    public bool SkipDefaultHandling;

    public CheckedGroupCheckEventArgs(bool bGroupChecked, bool bBeforeDefaultHandling)
    {
      GroupChecked = bGroupChecked;
      BeforeDefaultHandling = bBeforeDefaultHandling;
    }
  }

  public class CheckedGroupBox : GroupBox
  {
    #region Members
    public override string Text
    {
      get
      {
        return DesignMode ? base.Text : " ";
      }
      set
      {
        if (!DesignMode) m_cbCheck.Text = value;
        base.Text = DesignMode ? value : " ";
      }
    }

    private CheckBox m_cbCheck = new CheckBox();
    private Dictionary<Control, bool> m_dControls = new Dictionary<Control, bool>();
    private bool m_bProcessCheck = false;

    private Point m_pCheckboxOffset = new Point(6, 0);
    [Category("Layout")]
    public Point CheckboxOffset
    {
      get { return m_pCheckboxOffset; }
      set { m_pCheckboxOffset = value; OnAdjustChechBoxPosition(null, null); }
    }

    public event EventHandler<CheckedGroupCheckEventArgs> CheckedChanged;
    public bool Checked
    {
      get { return m_cbCheck.Checked; }
      set
      {
        m_cbCheck.Checked = value;
        OnCheckChanged();
      }
    }

    private bool m_bDisableIfUnchecked = true;
    [Category("Behavior")]
    [DefaultValue(true)]
    public bool DisableControlsIfUnchecked
    {
      get { return m_bDisableIfUnchecked; }
      set { m_bDisableIfUnchecked = value; if (!Checked) OnCheckChanged(); }
    }

    private System.ComponentModel.IContainer components = null;
    #endregion

    #region Init
    public CheckedGroupBox() : base()
    {
      InitializeComponent();
      OnAdjustChechBoxPosition(null, null);
    }

    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
      m_cbCheck.AutoSize = true;
      m_cbCheck.Checked = true;
      m_cbCheck.Parent = this;
      m_cbCheck.CheckedChanged += (o, e) => OnCheckChanged();
      RightToLeftChanged += OnAdjustChechBoxPosition;
      ClientSizeChanged += OnAdjustChechBoxPosition;
      TabStop = true;
      components.Add(m_cbCheck);
    }
    #endregion

    #region Eventhandlers
    private void OnAdjustChechBoxPosition(object sender, EventArgs e)
    {
      if (RightToLeft == RightToLeft.Yes)
      {
        m_cbCheck.Left = ClientSize.Width - m_cbCheck.Width - CheckboxOffset.X;
        m_cbCheck.Top = CheckboxOffset.Y;
      }
      else if (RightToLeft == RightToLeft.No)
      {
        m_cbCheck.Left = CheckboxOffset.X;
        m_cbCheck.Top = CheckboxOffset.Y;
      }
    }

    private void OnCheckChanged()
    {
      if (CheckedChanged != null)
      {
        CheckedGroupCheckEventArgs c = new CheckedGroupCheckEventArgs(Checked, true);
        CheckedChanged(this, c);
        if (c.SkipDefaultHandling) return;
      }
      HandleCheckChange();
      if (CheckedChanged != null)
      {
        CheckedGroupCheckEventArgs c = new CheckedGroupCheckEventArgs(Checked, false);
        CheckedChanged(this, c);
      }
    }
    #endregion

    #region Handle check/uncheck
    private void HandleCheckChange()
    {
      m_bProcessCheck = true;
      foreach (Control c in Controls)
      {
        if (c == m_cbCheck) continue;

        if (!m_dControls.ContainsKey(c))
        {
          m_dControls[c] = c.Enabled;
          c.EnabledChanged += MySubControlEnabledChanged;
          c.Disposed += MySubControlEnabledChanged;
        }

        if (DisableControlsIfUnchecked)
        {
          if (!m_cbCheck.Checked) c.Enabled = false;
          if (m_cbCheck.Checked && m_dControls[c] && !c.Enabled) c.Enabled = true;
        }
        else
        {
          if (!c.Enabled) c.Enabled = m_dControls[c];
        }
      }
      m_bProcessCheck = false;
    }

    private void MySubControlEnabledChanged(object sender, EventArgs e)
    {
      Control c = sender as Control;
      if ((c == null) || c.Disposing || c.IsDisposed)
      {
        m_dControls.Remove(c);
        RemoveControlsEventHandlers(c);
        return;
      }
      if (m_bProcessCheck) return;
      m_dControls[c] = c.Enabled;
    }

    private void RemoveControlsEventHandlers()
    {
      foreach (KeyValuePair<Control, bool> c in m_dControls)
        RemoveControlsEventHandlers(c.Key);
      m_dControls.Clear();
    }

    private void RemoveControlsEventHandlers(Control c)
    {
      if (c == null) return;
      c.EnabledChanged -= MySubControlEnabledChanged;
      c.Disposed -= MySubControlEnabledChanged;
    }
    #endregion

    #region Cleanup
    protected override void Dispose(bool disposing)
    {
      RightToLeftChanged -= OnAdjustChechBoxPosition;
      ClientSizeChanged -= OnAdjustChechBoxPosition;

      RemoveControlsEventHandlers();
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }
    #endregion
  }
}
