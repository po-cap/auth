#!/bin/bash

# 設定變數
SERVICE_NAME="auth"
SERVER_USER="sammy"
SERVER_IP="orange"

VERSION=$(cat version.txt)
WORKING_PATH="/opt/${SERVICE_NAME}/${VERSION}"
SCRIPT_PATH="/opt/${SERVICE_NAME}/bin"
SERVICE_FILE="/etc/systemd/system/${SERVICE_NAME}.service"


# 打包 API
dotnet publish ./src/Auth.Presentation/Auth.Presentation.csproj -c Release -o bin
sleep 1
tput clear

## 這段改掉
## 建立工作目錄
#ssh $SERVER_USER@$SERVER_IP "
#if [ ! -d ${WORKING_PATH} ]; then
#    sudo mkdir -p ${WORKING_PATH}
#fi
#"

## 這段改掉
## 建立腳本目錄
#ssh $SERVER_USER@$SERVER_IP "
#if [ ! -d ${SCRIPT_PATH} ]; then
#    sudo mkdir -p ${SCRIPT_PATH}
#fi
#"

## 這段改掉
## 檔案案傳到服務器上
#scp -r ./publish/* "root@${SERVER_IP}:${WORKING_PATH}" 
#scp ./build/install.sh "root@${SERVER_IP}:${SCRIPT_PATH}"
#scp ./version.txt "root@${SERVER_IP}:${SCRIPT_PATH}"


## 把剛剛 publish 的內容在本地刪除
#rm -rf ./publish
#sleep 1
#tput clear

## 執行安裝腳本
#echo "執行安裝腳本"
#ssh $SERVER_USER@$SERVER_IP "bash ${SCRIPT_PATH}/install.sh"