
# Packages

The contents to be transferred to the controller are zipped into a package with the .wago extension.


## package.json

Example package configuration replacing controller's default website.

``` JSON
{
  "description": "test package",
  "version": "1.0.0",
  "system": {
    "product": "750-8202/0025-0001;750-8202/0025-0002"
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

