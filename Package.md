
# Packages

The contents to be transferred to the controller are zipped into a package with the .wago extension.


## package.json

{
    "users": [
        { "name": "admin", "password": "test" }
    ],
    "fileSystem": [
        {
            "condition": "exists /etc/lighttpd/wbm_redirect.conf",
            "source": "/etc/lighttpd/redirect.conf",
            "action": "replace"
        },
        {
            "condition": "exists /etc/lighttpd/redirect_default.conf",
            "source": "/etc/lighttpd/redirect.conf",
            "action": "replace"
        },
        {
            "source": "/var/www/wbm/usr/index.html",
            "action": "create"
        }

    ]
}
