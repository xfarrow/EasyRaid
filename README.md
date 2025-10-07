# EasyRaid
EasyRaid is a file based Raid1-like application. 

EasyRaid offers several advantages in situations where traditional RAID systems may fall short:
* A proper Raid 1 is infeasible due to due to potential unreliable connections and therefore a desync is likely (e.g. non UASP-enabled USB enclosures)
* Synching solutions such as rsync require frequent syncs in order to keep the changes up to date, meanwhile EasyRaid replicates changes immediately
* Traditional RAID 1 implementatations, unlike EasyRaid, do not expose a transparent file system

Meanwhile EasyRaid is not useful in those conditions in which
* You want a block-level RAID
* You want CoW
* You want ECC correction / parity data
* You need asynchronous synching
* You prefer correctness and reliability (which should be the the main point to avoid this program altogether)

## How does it work?
EasyRaid is not a Raid application. It listens for Operating System notifications regarding changes on the file system and replicates those changes in the destination. For these reasons, EasyRaid is more like a script than a proper program.

## Is it reliable?
EasyRaid is not reliable. However, you can have a look at its code and its mathematical correctness.

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

## Correctness
Let S = <s1, s2, ...,sn> be a sequence of operations. We must prove that whatever the sequence is, the program behaves correctly. We'll assume that every sequence containing only one element is correct (which means that every operation is implemented correctly when it's the only operation being executed).
Therefore we just need to prove that every sequence of two elements is correct in order to prove the correctness of the algorithm.

Each operation can be one of the following: C = (file/folder) has changed ; N = (file/folder) has been created ; D = (file/folder) has been deleted ; R = (file/folder) has been renamed. There are 12 different sequences we must prove that are correct:

```
(C, N), (C, D), (C, R),
(N, C), (N, D), (N, R),
(D, C), (D, N), (D, R),
(R, N), (R, C), (R, D)
```
