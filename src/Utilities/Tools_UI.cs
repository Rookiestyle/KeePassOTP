namespace PluginTools
{
	public static partial class Tools
	{
		public static bool PreserveEntriesShown
		{
			get { return KeePass.Program.Config.CustomConfig.GetBool("Rookiestyle.PreserveEntriesShown", true); }
			set { KeePass.Program.Config.CustomConfig.SetBool("Rookiestyle.PreserveEntriesShown", value); }
		}

		public static void RefreshEntriesList(bool bSetModified)
		{
			UpdateUI(false, null, false, null, false, null, bSetModified);
		}

		public static void UpdateUI(bool bRecreateTabBar, KeePass.UI.PwDocument dsSelect,
			bool bUpdateGroupList, KeePassLib.PwGroup pgSelect, bool bUpdateEntryList,
			KeePassLib.PwGroup pgEntrySource, bool bSetModified)
		{
			//Update entries that are currently shown
			if (PreserveEntriesShown && !bUpdateEntryList && !bUpdateEntryList)
			{
				KeePass.Program.MainForm.RefreshEntriesList();
				//bUpdateGroupList = false;
				//bUpdateEntryList = false;
			}

			//Update UI to update all other parts of the UI
			KeePass.Program.MainForm.UpdateUI(bRecreateTabBar, dsSelect,
				bUpdateGroupList, bUpdateGroupList ? pgSelect : null,
				bUpdateEntryList, bUpdateEntryList ? pgEntrySource : null, bSetModified);
		}
	}
}