#!/bin/bash

# 設定變數
SERVICE_NAME="auth"
SERVER_USER="sammy"
SERVER_IP="orange"
URL="http://*:7020"
CPU_QUOTA="50%"
MEMORY_MAX="1G"
SERVICE_USER="www-data"

SCRIPT_PATH="/opt/${SERVICE_NAME}/bin"
WORKING_PATH="/opt/${SERVICE_NAME}/${VERSION}/"
VERSION=$(sudo cat "${SCRIPT_PATH}/version.txt")
SERVICE_FILE="/etc/systemd/system/${SERVICE_NAME}.service"


# 建立用戶（跑服務的用戶，並變更執行檔權限）
if ! id ${SERVICE_USER} &>/dev/null; then
    sudo useradd --system --no-create-home --shell /usr/sbin/nologin ${SERVICE_USER}
fi
sudo chown -R ${SERVICE_USER}:${SERVICE_USER} ${WORKING_PATH}

# 建立服務
if ! test -f ${SERVICE_FILE}; then
    sudo touch ${SERVICE_FILE}
fi

sudo tee ${SERVICE_FILE} >/dev/null <<EOF
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
EOF


## 重新加載 systemd 以識別新服務
sudo systemctl daemon-reload 
sudo systemctl enable "${SERVICE_NAME}"
sudo systemctl start "${SERVICE_NAME}"
sudo systemctl status "${SERVICE_NAME}"

