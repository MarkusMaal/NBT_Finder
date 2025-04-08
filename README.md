# NBT finder

CLI tool that allows you to search for NBT data in the "sea of non-sense", a.k.a. all your computer's storage drives. Please use "Release" configuration and don't attach a debugger for optimal performance. A beefy CPU may be required.

Note: NBT detection is not 100% accurate, since by default this program just searches for the string "minecraft" in uncompressed data. This can be altered in code to look for other data, such as the name of a world or a NBT tag (case sensitive).

## Features

* Automatically decompress any detected NBT files for further inspection
* Multi-threaded operation
* Reparse point detection (i.e. avoid scanning symlinks, junctions, etc.)
* Real-time status report
* Creates a log file after a successful scan of all the files detected
* If a directory name is passed as a parameter, that will be used instead of root directory for every drive (recommended if you know where the NBT files might be)
