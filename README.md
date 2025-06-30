[Unit]                                                                                                       
Description=Open Authorization Server
After=network.target

[Service]
WorkingDirectory=${WORKING_PATH}
ExecStart=/opt/dotnet/9.0.4/dotnet ${WORKING_PATH}Auth.Presentation.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=authorization
User=${SERVICE_USER}
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=${URL}

# 資源限制
CPUQuota=${CPU_QUOTA}
MemoryMax=${MEMORY_MAX}

[Install]
WantedBy=multi-user.target