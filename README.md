[Unit]
Description=Example .NET Web API App running on Linux

[Service]
WorkingDirectory=/opt/backstage/1.0.0/
ExecStart=/opt/dotnet/9.0.4/dotnet /opt/backstage/1.0.0/Backstage.Presentation.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=dotnet-example
User=api
Environment=ASPNETCORE_ENVIRONMENT=Production


[Install]
WantedBy=multi-user.target
