#!/bin/bash

echo
read -p "Do you want to change Telegram bot token? (y/N) " answer

if [ "$answer" == 'Y' ] || [ "$answer" == "y" ] 
then
    echo "Input Telegram bot token (looks like: '786850143:AAFclwIYW2CvwCV_WOKaFegAEe6bf4OY0MM')."
    read -p "Bot token: " token
    if [[ $token =~ [a-zA-Z0-9:_]+ ]]
    then
        echo -n "Write token to app.py..."
        awk -v sq="'" -v outString=$token -F':' '{if (match($1,"'token'")) print "        "sq"token"sq": "sq outString sq ","; else print $0;}' app.py > /tmp/awk.tmp; mv /tmp/awk.tmp app.py
        if [ "$?" == 0 ]; then echo -e "\e[32mOK\e[39m"; else echo -e "\e[91mFAIL\e[39m";exit -1;fi
    else
        echo Incorrect input
        exit -1;
    fi
fi

read -p "Do you want to change proxy settings? (y/N) " answer

if [ "$answer" == 'Y' ] || [ "$answer" == "y" ] 
then

    echo "Step 1/3. Input proxy address and port (example: http://proxy.net:8888 or socks5://proxy.net)"
    read -p "Proxy address: " proxy
    if [[ $proxy =~ [a-zA-Z]+:\/\/[a-zA-Z.]+:[0-9][0-9][0-9][0-9] ]]
    then
        echo Address is correct
    else
        if [[ $proxy =~ [a-zA-Z0-9]+:\/\/[a-zA-Z.]+ ]]
        then
            echo Address is correct, but port is not given
        else
            echo Address is not correct!
            exit -1
        fi
    fi

    echo "Step 2/3. Input proxy login (or press ENTER to leave it empty)"
    read -p "Proxy login: " proxy_login

    echo "Step 2/3. Input proxy password (or press ENTER to leave it empty)"
    read -p "Proxy password: " proxy_password
    
    if [ "$proxy_login" == "" ] && [ "$proxy_password" == "" ]
    then
        delim=""
    else
        delim=","
    fi
    
    echo -n "Write proxy settings to NotificationBot.py..."
    awk -v sq="'" -v outString=$proxy -v delim=$delim -F':' '{if (match($1,"'proxy_url'")) print "        "sq"proxy_url"sq": "sq outString sq delim; else print $0;}' UnconfiguredBot.py > /tmp/awk.tmp; mv /tmp/awk.tmp NotificationBot.py
    
    if [ "$proxy_login" == "" ] && [ "$proxy_password" == "" ]
    then
        # Убрать из вывода закомментаренные строки
        awk -F'#' '{if ($1!="") print $1}' NotificationBot.py > /tmp/awk.tmp; mv /tmp/awk.tmp NotificationBot.py
        if [ "$?" == 0 ]; then echo -e "\e[32mOK\e[39m"; else echo -e "\e[91mFAIL\e[39m";exit -1;fi
    else
        # Вставить логин и пароль
        awk -v username=$proxy_login -v sq="'" -F':' '{if (match($1,"'username'")) print $1" : " sq username sq ","; else print $0;}' NotificationBot.py > /tmp/awk.tmp; mv /tmp/awk.tmp NotificationBot.py
        awk -v password=$proxy_password -v sq="'" -F':' '{if (match($1,"'password'")) print $1" : " sq password sq ","; else print $0;}' NotificationBot.py > /tmp/awk.tmp; mv /tmp/awk.tmp NotificationBot.py
        # Убрать из символы комментариев
        awk -F'#' '{if ($1!="") print $1; else print substr($0,2);}' NotificationBot.py > /tmp/awk.tmp; mv /tmp/awk.tmp NotificationBot.py
        if [ "$?" == 0 ]; then echo -e "\e[32mOK\e[39m"; else echo -e "\e[91mFAIL\e[39m";exit -1;fi
    fi
fi

