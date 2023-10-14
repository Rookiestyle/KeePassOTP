using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PluginTools
{
  public static class PTHotKeyManager
  {
    [DllImport("user32")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private static bool m_bIsUnix = true;
    public static event EventHandler<HotKeyEventArgs> HotKeyPressed;
    private static MessageWindow m_msgWindow = null;

    static PTHotKeyManager()
    {
      m_bIsUnix = KeePassLib.Native.NativeLib.IsUnix();
      if (m_bIsUnix) return;
      m_msgWindow = new MessageWindow();
    }

    public static int RegisterHotKey(Keys keys)
    {
      if (m_bIsUnix) return 0;
      KeyModifiers modifiers = ModifiersToNativeModifiers(keys);
      //global hotkeys can be registered only once
      //simply use the hotkey itself as id
      int id = (int)modifiers + (int)(keys & Keys.KeyCode);
      if (RegisterHotKey(m_msgWindow.Handle, id, (uint)modifiers, (uint)(keys & Keys.KeyCode)))
        return id;
      else
        return 0;
    }

    public static bool UnregisterHotKey(int id)
    {
      if (m_bIsUnix) return false;
      return UnregisterHotKey(m_msgWindow.Handle, id);
    }

    private static KeyModifiers ModifiersToNativeModifiers(Keys keys)
    {
      KeyModifiers modifiers = KeyModifiers.NoRepeat;
      if ((keys & Keys.Shift) != Keys.None)
        modifiers |= KeyModifiers.Shift;
      if ((keys & Keys.Alt) != Keys.None)
        modifiers |= KeyModifiers.Alt;
      if ((keys & Keys.Control) != Keys.None)
        modifiers |= KeyModifiers.Control;
      return modifiers;
    }

    internal static void OnHotKeyPressed(HotKeyEventArgs e)
    {
      if (HotKeyPressed != null) HotKeyPressed(null, e);
    }

    private class MessageWindow : NativeWindow, IDisposable
    {
      private const int WM_HOTKEY = 0x312;

      public MessageWindow()
      {
        CreateHandle(new CreateParams());
      }

      public void Dispose()
      {
        DestroyHandle();
      }

      protected override void WndProc(ref Message m)
      {
        if (m.Msg == WM_HOTKEY)
        {
          HotKeyEventArgs e = new HotKeyEventArgs(m.LParam, m.WParam);
          PTHotKeyManager.OnHotKeyPressed(e);
        }
        base.WndProc(ref m);
      }
    }
  }

  public class HotKeyEventArgs : EventArgs
  {
    public readonly Keys Key;
    public readonly int ID;

    public HotKeyEventArgs(IntPtr Hotkey, IntPtr id)
    {
      uint param = (uint)Hotkey.ToInt64();
      Key = (Keys)((param & 0xffff0000) >> 16);
      KeyModifiers m = (KeyModifiers)(param & 0x0000ffff);
      Key |= NativeModifiersToModifiers(m);
      ID = (int)id;
    }

    private static Keys NativeModifiersToModifiers(KeyModifiers keys)
    {
      Keys modifiers = Keys.None;
      if ((keys & KeyModifiers.Shift) == KeyModifiers.Shift)
        modifiers |= Keys.Shift;
      if ((keys & KeyModifiers.Alt) == KeyModifiers.Alt)
        modifiers |= Keys.Alt;
      if ((keys & KeyModifiers.Control) == KeyModifiers.Control)
        modifiers |= Keys.Control;
      return modifiers;
    }
  }

  internal enum KeyModifiers
  {
    Alt = 1,
    Control = 2,
    Shift = 4,
    Windows = 8,
    NoRepeat = 0x4000
  }
}
