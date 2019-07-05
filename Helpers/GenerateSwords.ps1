# ��������� � Resources
# ������ �������� ��� ����� ������. � ������ ����� ���� �� ������ ������������ � ������������ ����� ������.

$schemePath='..\Zilon.Client\Assets\Resources\Schemes\Props\Equipments'
$newPropFilePath='.\new-props.txt'
$protoScheme='short-sword'
$outDropFile='drop.txt'

foreach($propSid in Get-Content $newPropFilePath) {
    copy-item -Path "$schemePath\$protoScheme.json" -Destination "$schemePath\$propSid.json"
	(Get-Content "$schemePath\$propSid.json" -Encoding UTF8).replace('�������� ���', "$propSid").replace('Short sword', "$propSid") | Set-Content "$schemePath\$propSid.json"
	Add-Content -Path ".\$outDropFile" -Value "{ `"SchemeSid`": `"$propSid`", `"Weight`": 10},"
}