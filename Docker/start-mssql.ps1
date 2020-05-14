docker.exe run --name sharparch-mssql -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=Password12!' -p 2433:1433 -v sqlvolume:/var/opt/mssql -d mcr.microsoft.com/mssql/server:2019-CU4-ubuntu-16.04


# docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<YourStrong!Passw0rd>" -p 1433:1433 -v <host directory>/data:/var/opt/mssql/data -v <host directory>/log:/var/opt/mssql/log -v <host directory>/secrets:/var/opt/mssql/secrets
# -d mcr.microsoft.com/mssql/server:2019-GA-ubuntu-16.04
