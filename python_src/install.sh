#!/bin/bash
set -e

echo "Install prerequest packages"
#sudo dpkg -i python3-lib2to3_3.6.5-3_all.deb
#sudo dpkg -i python3-distutils_3.6.5-3_all.deb
#sudo dpkg -i python-pip-whl*
sudo dpkg -i python*

echo "Install bot service"
sudo -H dpkg -i dozor-telegram-bot*.deb

echo
echo "Check database connection in /var/lib/dozor-net/telegram-bot/db_config.json file" 
echo "You need to configure telegram-token for bot in Dozor interface and start dozor-telegram-bot service" 
