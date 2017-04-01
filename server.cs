if(forceRequiredAddOn("Tool_NewDuplicator") == $Error::AddOn_NotFound)
{
	error("ERROR: Tool_NewDuplicator is required for Server_EnvironmentZones.");
	messageAll('', "\c0ERROR: Tool_NewDuplicator is required for Server_EnvironmentZones.");
	return;
}

if(ndCompareVersion($ND::Version, "1.6.1") == 2)
{
	error("ERROR: Tool_NewDuplicator must be up-to-date for Server_EnvironmentZones.");
	messageAll('', "\c0ERROR: Tool_NewDuplicator must be up-to-date for Server_EnvironmentZones.");
	return;
}

if(!isFunction(NetObject, setNetFlag))
{
	error("ERROR: SelectiveGhosting.dll is required for Server_EnvironmentZones.");
	messageAll('', "\c0ERROR: SelectiveGhosting.dll is required for Server_EnvironmentZones.");
	return;
}

exec("./environment.cs");
exec("./zone.cs");
exec("./manager.cs");
