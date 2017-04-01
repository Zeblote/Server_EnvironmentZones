// Installs a copy of Support_Updater to handle automatic updates.
// Based on http://forum.blockland.us/index.php?topic=255382.0
// -------------------------------------------------------------------

function ezInstallUpdaterPrompt()
{
	%message = "<just:left>Environment Zones might get new features or bug fixes in the future."
	@ " To make this much easier for you, an automatic updater is available! (Support_Updater by Greek2Me)"
	NL "\nJust click yes below to install it in the background. Click no to be reminded later.";

	messageBoxYesNo("Environment Zones | Automatic Updates", %message, "ezInstallUpdater();");
}

function ezInstallUpdater()
{
	%url = "http://mods.greek2me.us/storage/Support_Updater.zip";
	%downloadPath = "Add-Ons/Support_Updater.zip";
	%className = "EZ_InstallUpdaterTCP";

	connectToURL(%url, "GET", %downloadPath, %className);
	messageBoxOK("Environment Zones | Downloading Updater", "Trying to download the updater...");
}

function EZ_InstallUpdaterTCP::onDone(%this, %error)
{
	if(%error)
		messageBoxOK("Environment Zones | Error :(", "Error downloading the updater:" NL %error NL "You'll be prompted again at a later time.");
	else
	{
		messageBoxOK("Environment Zones | Updater Installed", "The updater has been installed.\n\nHave fun!");

		discoverFile("Add-ons/Support_Updater.zip");
		exec("Add-ons/Support_Updater/client.cs");
	}
}

schedule(1000, 0, "ezInstallUpdaterPrompt");
$SupportUpdaterMigration = true;
