# ���������� � ����� � ��������� �����������. ��������, � Debug\bin
Set-PSDebug -Trace 1
For ($i=0; $i -le 100; $i++)
{
  echo "======= $i ========="
  $outPath "[���� � ����� � �������������]\map-"+$i+".bmp"
  echo $outPath
  .\Zilon.Core.MassSectorGenerator.exe -out=$outPath
  echo "-------------------"
}