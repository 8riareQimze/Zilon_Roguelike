# ��������� � Resources
# ������ �������� ��� ����� ������. � ������ ����� ���� �� ������ ������������ � ������������ ����� ������.

$path='.\Icons\props'
$schemePath='.\Schemes\Props\Equipments'
$time = Get-Date -Date "2019-05-07 00:00:00Z"

$newIcons=get-childitem "$path" -recurse | Where-Object {$_.LastWriteTime -gt $time}

foreach($file in $newIcons)
{
	$fileName=[System.IO.Path]::GetFileNameWithoutExtension($file)
	copy-item -Path "$schemePath\short-sword.json" -Destination "$schemePath\$fileName.json"
	
	(Get-Content "$schemePath\$fileName.json" -Encoding UTF8).replace('�������� ���', "$fileName").replace('Short sword', "$fileName") | Set-Content "$schemePath\$fileName.json"

}