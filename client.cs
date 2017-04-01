// Checks whether we can install the updater. There isn't any other client code here.
// -------------------------------------------------------------------

if(!$Pref::EZ::DisableUpdater
	&& !$SupportUpdaterMigration
	&& !isFile("Add-Ons/Support_Updater.zip"))
{
	exec("./updater/tcpclient.cs");
	exec("./updater/install.cs");
}
