# EasyRaid
EasyRaid is a file based Raid1-like application. 

It is useful in those conditions in which:
* A proper Raid 1 is infeasible because the connection might be unreliable and therefore a desync raid is likely (e.g. non UASP-enabled USB enclosures)
* A Snapraid system requires frequent syncs, meanwhile EasyRaid replicates changes immediately

## Synopsis
```
EasyRaid [OPTIONS]

OPTIONS:
-v, --version
        Prints the current version and exits immediately.

-h, --help
        Displays this help information.

-n, --new-config <SOURCE_PATH> <DESTINATION_PATH>
        Creates a new configuration file.
        Both source and destination paths are required

-c, --config <CONFIG_FILE_PATH>
        Loads an existing configuration file.
```

**This piece of software is alpha-quality**

