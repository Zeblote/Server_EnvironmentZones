// Checks prerequisites, executes all required scripts and initializes the server side.
// -------------------------------------------------------------------

if(forceRequiredAddOn("Tool_NewDuplicator") == $Error::AddOn_NotFound)
{
	error("ERROR: Tool_NewDuplicator is required for Server_EnvironmentZones.");
	schedule(1000, 0, messageAll, 'MsgError', "\c0ERROR: Tool_NewDuplicator is required for Server_EnvironmentZones.");
	return;
}

if(ndCompareVersion($ND::Version, "1.6.1") == 2)
{
	error("ERROR: Tool_NewDuplicator must be up-to-date for Server_EnvironmentZones.");
	schedule(1000, 0, messageAll, 'MsgError', "\c0ERROR: Tool_NewDuplicator must be up to date for Server_EnvironmentZones.");
	return;
}

if(!isFunction(NetObject, setNetFlag))
{
	error("ERROR: SelectiveGhosting.dll is required for Server_EnvironmentZones.");
	schedule(1000, 0, messageAll, 'MsgError', "\c0ERROR: SelectiveGhosting.dll is required for Server_EnvironmentZones.");
	return;
}

exec("./scripts/zone.cs");
exec("./scripts/support.cs");
exec("./scripts/manager.cs");
exec("./scripts/commands.cs");
exec("./scripts/environment.cs");

activatePackage(EnvironmentZones);
if(!isObject(EnvironmentZoneGroup))
	new ScriptGroup(EnvironmentZoneGroup);
