datablock TriggerData(EnvironmentZoneTrigger)
{
	tickPeriodMS = 120;
};

function EnvironmentZoneTrigger::onEnterTrigger(%this, %trigger, %obj)
{
	if(%obj.getClassName() $= "Player"
		|| %obj.getClassName() $= "AiPlayer"
		|| %obj.getClassName() $= "WheeledVehicle"
		|| %obj.getClassName() $= "FlyingWheelVehicle")
	{
		%trigger.zone.recursiveEnterZone(%obj);
	}
}

function EnvironmentZoneTrigger::onLeaveTrigger(%this, %trigger, %obj)
{
	if(%obj.getClassName() $= "Player"
		|| %obj.getClassName() $= "AiPlayer"
		|| %obj.getClassName() $= "WheeledVehicle"
		|| %obj.getClassName() $= "FlyingWheelVehicle")
	{
		%trigger.zone.recursiveLeaveZone(%obj);
	}
}

function EnvironmentZone::recursiveEnterZone(%this, %obj)
{
	if(%obj.getClassName() $= "Player" && isObject(%obj.client))
		%obj.client.setEnvironment(%this.environment);

	for(%i = 0; %i < %obj.getMountedObjectCount(); %i++)
		%this.recursiveEnterZone(%obj.getMountedObject(%i));
}

function EnvironmentZone::recursiveLeaveZone(%this, %obj)
{
	if(%this.persistent)
		return;

	if(%obj.getClassName() $= "Player" && isObject(%obj.client))
		%obj.client.setEnvironment($DefaultEnvironment);

	for(%i = 0; %i < %obj.getMountedObjectCount(); %i++)
		%this.recursiveLeaveZone(%obj.getMountedObject(%i));
}

function EnvironmentZone(%name)
{
	//Create zone object
	EnvironmentZoneGroup.add(
		%this = new ScriptObject(EnvironmentZone)
		{
			zoneName = %name;
		}
	);

	//Create zone trigger
	%this.trigger = new Trigger()
	{
		datablock = EnvironmentZoneTrigger;
		polyhedron = "-0.5 -0.5 -0.5 1 0 0 0 1 0 0 0 1";
		zone = %this;
	};

	//Create visualization box
	if($ShowEnvironmentZones)
	{
		%this.editBox = ND_SelectionBox();
		%this.editBox.setDisabledMode();
		%this.updateShapeName();
	}

	%this.environment = Environment();
	%this.environment.copyFrom($DefaultEnvironment);

	return %this;
}

function EnvironmentZone::onRemove(%this)
{
	%this.trigger.delete();

	if(isObject(%this.editBox))
		%this.editBox.delete();

	%this.environment.delete();
}

function EnvironmentZone::setSize(%this, %point1, %point2)
{
	if(getWordCount(%point1) == 6)
	{
		%point2 = getWords(%point1, 3, 5);
		%point1 = getWords(%point1, 0, 2);
	}

	%this.point1 = %point1;
	%this.point2 = %point2;

	if(isObject(%this.editBox))
		%this.editBox.setSize(%point1, %point2);

	%size = vectorSub(%point2, %point1);
	%center = vectorAdd(%point1, vectorScale(%size, 0.5));

	%this.trigger.setTransform(%center @ " 1 0 0 0");
	%this.trigger.setScale(%size);
}

function EnvironmentZone::startEdit(%this, %client)
{
	if(%this.editClient)
	{
		echo("ERROR: Environment zone already has an editing client");
		return false;
	}

	if(!$ShowEnvironmentZones)
		showEnvironmentZones(true);

	%this.editClient = %client;
	%client.envEditZone = %this;

	%this.editBox.setSizeAligned(%this.point1, %this.point2, %client.getControlObject());
	%this.editBox.setNormalMode();
	%this.updateShapeName();
}

function EnvironmentZone::stopEdit(%this)
{
	if(!%this.editClient)
	{
		echo("ERROR: Environment zone does not have an editing client");
		return false;
	}

	%this.editClient.envEditZone = 0;
	%this.editClient = 0;

	%this.editBox.setDisabledMode();
	%this.setSize(%this.editBox.getWorldBox());
	%this.updateShapeName();
}

function EnvironmentZone::updateShapeName(%this)
{
	%this.editBox.shapeName.setShapeNameColor(%this.editBox.borderColor);

	if(isObject(%this.editClient))
		%editor = %this.editClient.name @ " editing ";

	if(%this.persistent)
		%persistent = "Persistent ";

	%this.editBox.shapeName.setShapeName(%editor @ %persistent @ "Env Zone \"" @ %this.zoneName @ "\"");
}

package EnvironmentZones
{
	//Shift Brick
	function serverCmdShiftBrick(%client, %x, %y, %z)
	{
		if(!isObject(%client.envEditZone))
			return parent::serverCmdShiftBrick(%client, %x, %y, %z);

		//Move the corner
		switch(getAngleIDFromPlayer(%client.getControlObject()))
		{
			case 0: %newX =  %x; %newY =  %y;
			case 1: %newX = -%y; %newY =  %x;
			case 2: %newX = -%x; %newY = -%y;
			case 3: %newX =  %y; %newY = -%x;
		}

		%newX = mFloor(%newX) / 2;
		%newY = mFloor(%newY) / 2;
		%z    = mFloor(%z   ) / 5;

		%client.envEditZone.editBox.shiftCorner(%newX SPC %newY SPC %z, 100000);
	}

	//Super Shift Brick
	function serverCmdSuperShiftBrick(%client, %x, %y, %z)
	{
		if(!isObject(%client.envEditZone))
			return parent::serverCmdSuperShiftBrick(%client, %x, %y, %z);

		serverCmdShiftBrick(%client, %x * 8, %y * 8, %z * 20);
	}

	//Rotate Brick
	function serverCmdRotateBrick(%client, %direction)
	{
		if(!isObject(%client.envEditZone))
			return parent::serverCmdRotateBrick(%client, %direction);

		%client.envEditZone.editBox.switchCorner();
	}
};

function showEnvironmentZones(%bool)
{
	$ShowEnvironmentZones = %bool;
	%count = EnvironmentZoneGroup.getCount();

	for(%i = 0; %i < %count; %i++)
	{
		%zone = EnvironmentZoneGroup.getObject(%i);

		if(%bool && !isObject(%zone.editBox))
		{
			%zone.editBox = ND_SelectionBox("Env Zone \"" @ %zone.zoneName @ "\"");
			%zone.editBox.setDisabledMode();
			%zone.editBox.setSize(%zone.point1, %zone.point2);
			%zone.updateShapeName();
		}
		else if(!%bool && isObject(%zone.editBox))
		{
			if(isObject(%zone.editClient))
				%zone.stopEdit();
			
			%zone.editBox.delete();
		}
	}
}

if(!isObject(EnvironmentZoneGroup))
	new ScriptGroup(EnvironmentZoneGroup);
