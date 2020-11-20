using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace PluginTools
{
	public static partial class Tools
	{
		public static object GetField(string field, object obj)
		{
			BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic;
			return GetField(field, obj, bf);
		}

		public static object GetField(string field, object obj, BindingFlags bf)
		{
			if (obj == null) return null;
			FieldInfo fi = obj.GetType().GetField(field, bf);
			if (fi == null) return null;
			return fi.GetValue(obj);
		}

		public static Control GetControl(string control)
		{
			return GetControl(control, KeePass.Program.MainForm);
		}

		public static Control GetControl(string control, Control form)
		{
			if (form == null) return null;
			if (string.IsNullOrEmpty(control)) return null;
			Control[] cntrls = form.Controls.Find(control, true);
			if (cntrls.Length == 0) return null;
			return cntrls[0];
		}

		public static ToolStripMenuItem FindToolStripMenuItem(ToolStripItemCollection tsic, string key, bool searchAllChildren)
		{
			if (tsic == null) return null;
			ToolStripItem[] tsi = FindToolStripMenuItems(tsic, key, searchAllChildren);
			if (tsi.Length > 0) return tsi[0] as ToolStripMenuItem;
			return null;
		}

		public static ToolStripItem[] FindToolStripMenuItems(ToolStripItemCollection tsic, string key, bool searchAllChildren)
		{
			if (tsic == null) return new ToolStripItem[] { };
			ToolStripItem[] tsi = tsic.Find(key, searchAllChildren);
			if (!MonoWorkaroundRequired || !searchAllChildren) return tsi;

			//Mono does not support 'searchAllChildren' for ToolStripItemCollection
			//Iterate over all items and search for given item
			List<ToolStripItem> lItems = new List<ToolStripItem>(tsi);
			foreach (var item in tsic)
			{
				ToolStripMenuItem tsmi = item as ToolStripMenuItem;
				if (tsmi == null) continue;
				lItems.AddRange(FindToolStripMenuItems(tsmi.DropDownItems, key, searchAllChildren));
			}
			return lItems.ToArray();
		}
	}
}