
#$x = @(Select-String -Path packages.config -Pattern id\s*=\s*"(.*?)"\s -AllMatches | %{$_.Matches} | %{" add package " + $_.Groups[1].Value})

$x = @("--info", "add package )

&"dotnet.exe" $x
