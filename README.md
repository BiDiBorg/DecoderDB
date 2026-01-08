# BiDiB-DecoderDB

DecoderDB is an open repository for decoder and firmware definitions

Definitions are currently used by BiDiB-Monitor to provide comfortable support with managing configuration variables (CVs) of the local decoder stack.

## Decoders & Firmware

The repository contains 2 kinds of data. Decoder definitions and firmware definitions.
The decoder definition describes the base specifications a decoder like product identifieres, dimensions, electrical limits but also references to products images and manuals.

The firmware definition contains the descriptions of all configuration variables available for the corresponding firmware version. It also contains a struture definition to navigate through the huge amout of variables in an easy way.

The reference between decoder definition and firmware definition is done via a list of decoder names inside the firmware definition.

![Decoder Firmware Reference](./docs/decoder-firmware-reference.png "Decoder Firmware Reference")

## Contribution

To contribute any changes to the decoder or firmware definitions just clone this repository and modify the json files within the `repo/[decoder/firmware]` folder and push the changes afterwards.
As this repository is meant to be a community project, quality contributions are highly appreciated!

To overcome the cumbersome manual editing of the json files there is an editor available.
The editor can be accessed either via the [published online version](https://decoderdb.bidib.org) or started from within the repository.

With the editor the local repo files can be edited in a comfortable way.
Once work is done and changes are saved they have to be committed and pushed manually.

In any case the editor application needs read and write access to the local repository `/repo` folder.
It will ask for it once you try to enable the edit mode for a decoder or firmware definition.
![Repo request](./docs/decoderdb-repo-request.png "Repo request")

The permission is also checked on start and indicated in the status bar.
If access is not granted it can be set via the button there.
![Repo access](./docs/decoderdb-repo-access.png "Repo access")

:warning: _Currently only Chromium based browsers are supporting the file access api which is used the modify the local files!_

## Deployment

Changes can be done manually direcly in this repository at any time.

Every commit / push within the `repo/` folder on main branch triggers the workflow to updates the repository.json reference and publish the changes.
The update source is currently available via [repository.json](https://decoderdb.bidib.org/repository.json)

Changes to the generator code does not trigger any workflow at the moment.

## BiDiB-Monitor

To be use the new repository with BiDiB-Monitor, the synchronization settings have to be changed. Replace the exisiting value for the service update url with `https://decoderdb.bidib.org/repository.json`.

![Update service url](./docs/bidib-monitor-decoderdb-update.png "Update service url")

The service authorization code can remain the same or be blank.

As the files are now provided in json format instead of xml a version of **BiBiB-Monitor 10.8.3** or newer has to be used. The general workflow remains the same. `Check for updates` -> download new files -> files are ready to use.

The download with not delete the old `*.decdb` format files as the synchronization with the original DecoderDB can be still used in parallel.
But the new BiBiB-Monitor version preferably loads the `*.json` files first.

### Testing changes

As long the editor does not support the full preview of the CV definitions, the **BiBiB-Monitor** can be use for testing.

Therefore the changed file have to be copied from the repository folder to the appropiated folder inside the BiDiB user data directory.

```
Decoder_145_MS450.json

[PathToRepository]\repo\decoder\145\Decoder_145_MS450.json -> C:\users\[USERNAME]\.bidib\decoder\145\Decoder_145_MS450.json
```

Afterwards start the **BiBiB-Monitor** or if already started goto the Decoder data repository settings dialog and press the `Reload definitions` button.
![Reload definitions](./docs/bidib-monitor-decoderdb-reload.png "Reload definitions")
The changes should now be visible in the decoder edit dialog.

## Generate single json firmware file from decdb file

To generate a single json firmware file from en existing decdb file, run the following command in the project root:

```
dotnet run --configuration Release --project .\src\RepoGenerator\ --repoPath .\repo\ --saveJson true --file Firmware_145_5.5
```

:grey_exclamation: This step is not required for the general use of editor!
