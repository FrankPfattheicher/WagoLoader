
# Packages

The contents to be transferred to the controller are zipped into a package with the .wago extension.


## package.json

Example package configuration setting controller's clock to UTC and current PC date and time and all passwords to "test".

``` JSON
{
  "description": "test package",
  "version": "1.0.0",
  "system": {
    "product": "750-8202/0025-0001;750-8202/0025-0002",
    "timezone": "Etc/UTC",
    "setDateTime":  true 
  },
  "users": {
    "linux": [
      {
        "name": "root",
        "password": "test"
      },
      {
        "name": "admin",
        "password": "test"
      },
      {
        "name": "user",
        "password": "test"
      }
    ],
    "wbm": [
      {
        "name": "admin",
        "password": "test"
      },
      {
        "name": "user",
        "password": "test"
      }
    ]
  }
}
```

