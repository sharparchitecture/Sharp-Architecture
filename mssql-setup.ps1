# Enable names pipes and set alias to TardisBank
# reference: https://www.appveyor.com/docs/services-databases/#enabling-tcpip-named-pipes-and-setting-instance-alias

[reflection.assembly]::LoadWithPartialName("Microsoft.SqlServer.Smo") | Out-Null
[reflection.assembly]::LoadWithPartialName("Microsoft.SqlServer.SqlWmiManagement") | Out-Null

$serverName = $env:COMPUTERNAME
$instanceName = 'SQL2019'
$smo = 'Microsoft.SqlServer.Management.Smo.'
$wmi = new-object ($smo + 'Wmi.ManagedComputer')

# Enable TCP/IP
$uri = "ManagedComputer[@Name='$serverName']/ServerInstance[@Name='$instanceName']/ServerProtocol[@Name='Tcp']"
$Tcp = $wmi.GetSmoObject($uri)
$Tcp.IsEnabled = $true
$Tcp.alter()

# Disable named pipes
$uri = "ManagedComputer[@Name='$serverName']/ ServerInstance[@Name='$instanceName']/ServerProtocol[@Name='Np']"
$Np = $wmi.GetSmoObject($uri)
$Np.IsEnabled = $true
$Np.Alter()

# Set Alias (64 bit)
New-Item HKLM:\SOFTWARE\Microsoft\MSSQLServer\Client -Name ConnectTo | Out-Null
Set-ItemProperty -Path HKLM:\SOFTWARE\Microsoft\MSSQLServer\Client\ConnectTo -Name 'TardisBank' -Value "DBMSSOCN,$serverName\$instanceName" | Out-Null

# Set Alias (32 bit)
New-Item HKLM:\SOFTWARE\WOW6432Node\Microsoft\MSSQLServer\Client -Name ConnectTo | Out-Null
Set-ItemProperty -Path HKLM:\SOFTWARE\WOW6432Node\Microsoft\MSSQLServer\Client\ConnectTo -Name 'TardisBank' -Value "DBMSSOCN,$serverName\$instanceName" | Out-Null

$configPath=".\Samples\TardisBank\Src\Suteki.TardisBank.WebApi\NHibernate.config"

((Get-Content -path $configPath -Raw) -replace 'Data Source=localhost,2433',"Data Source=$serverName\$instanceName") | Set-Content -Path $configPath

# Start services
Set-Service SQLBrowser -StartupType Manual
Start-Service SQLBrowser
Start-Service "MSSQL`$$instanceName"

sqlcmd -S TardisBank -U sa -P Password12! -Q "CREATE DATABASE TardisBank"

