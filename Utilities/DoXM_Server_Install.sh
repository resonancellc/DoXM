echo "If you haven't already, publish the DoXM Server app using the Publish.ps1 scipt
(e.g. pwsh -f <path to Publish.ps1> -hostname example.com -rid linux-x64 -outdir /var/www/doxm).  
The app root path is the output directory.
"
read -p "Enter app root path: " appRoot
read -p "Enter app host (e.g. example.com): " appHost



# Install .NET Core Runtime.
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin



# Install Nginx
apt-get update
apt-get install nginx

systemctl start nginx


# Configure Nginx
nginxConfig="server {
    listen        80;
    server_name   $appHost *.$appHost;
    location / {
        proxy_pass         http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade \$http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host \$host;
        proxy_cache_bypass \$http_upgrade;
        proxy_set_header   X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto \$scheme;
    }
}"

echo "$nginxConfig" > /etc/nginx/sites-available/doxm

ln -s /etc/nginx/sites-available/doxm /etc/nginx/sites-enabled/doxm

# Test config.
nginx -t

# Reload.
nginx -s reload




# Create service.

serviceConfig="[Unit]
Description=DoXM Server

[Service]
WorkingDirectory=$appRoot
ExecStart=/usr/bin/dotnet $appRoot/DoXM_Server.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
SyslogIdentifier=doxm
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target"

echo "$serviceConfig" > /etc/systemd/system/doxm.service


# Enable service.
systemctl enable doxm.service
# Start service.
systemctl start doxm.service


# Install Certbot and get SSL cert.
apt-get install certbot

certbot --nginx
