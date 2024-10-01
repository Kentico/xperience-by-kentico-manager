[![Kentico Labs](https://img.shields.io/badge/Kentico_Labs-grey?labelColor=orange&logo=data:image/svg+xml;base64,PHN2ZyBjbGFzcz0ic3ZnLWljb24iIHN0eWxlPSJ3aWR0aDogMWVtOyBoZWlnaHQ6IDFlbTt2ZXJ0aWNhbC1hbGlnbjogbWlkZGxlO2ZpbGw6IGN1cnJlbnRDb2xvcjtvdmVyZmxvdzogaGlkZGVuOyIgdmlld0JveD0iMCAwIDEwMjQgMTAyNCIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxwYXRoIGQ9Ik05NTYuMjg4IDgwNC40OEw2NDAgMjc3LjQ0VjY0aDMyYzE3LjYgMCAzMi0xNC40IDMyLTMycy0xNC40LTMyLTMyLTMyaC0zMjBjLTE3LjYgMC0zMiAxNC40LTMyIDMyczE0LjQgMzIgMzIgMzJIMzg0djIxMy40NEw2Ny43MTIgODA0LjQ4Qy00LjczNiA5MjUuMTg0IDUxLjIgMTAyNCAxOTIgMTAyNGg2NDBjMTQwLjggMCAxOTYuNzM2LTk4Ljc1MiAxMjQuMjg4LTIxOS41MnpNMjQxLjAyNCA2NDBMNDQ4IDI5NS4wNFY2NGgxMjh2MjMxLjA0TDc4Mi45NzYgNjQwSDI0MS4wMjR6IiAgLz48L3N2Zz4=)](https://github.com/Kentico/.github/blob/main/SUPPORT.md#labs-limited-support)
[![Nuget](https://img.shields.io/nuget/v/Kentico.Xperience.Manager)](https://www.nuget.org/packages/Kentico.Xperience.Manager#versions-body-tab)
[![build](https://github.com/kentico/xperience-manager/actions/workflows/build.yml/badge.svg)](https://github.com/kentico/xperience-manager/actions/workflows/build.yml)

# Xperience Manager (xman)

This tool simplifies the process of installing and managing Xperience by Kentico instances by providing step-by-step wizards with default options provided.

<img src="https://raw.githubusercontent.com/kentico/xperience-manager/master/img/screenshot.png" width="350" />

## Installing the tool

Run the following command from a command prompt such as Powershell:

```bash
dotnet tool install Kentico.Xperience.Manager -g
```

## Updating the tool

Run the following command from a command prompt such as Powershell:

```bash
dotnet tool update Kentico.Xperience.Manager -g
```

## Getting started

This tool can be run from anywhere, as long as the directory contains the [configuration file](#configuration-file). If there is no configuration file, a new one will be created when you run the tool. When you [install](#installing-a-new-project) a new instance, a new profile is created in the configuration file, allowing you to manage multiple installations without changing directory.

## Configuration file

The `xman.json` file contains information about the tool, your default options, and [profiles](#managing-profiles). This file will be automatically created if it doesn't exist when you run a command like `xman p`.

```json
{
  "Version": "5.0.0.0",
  "Profiles": [
    {
      "ProjectName": "xbk29",
      "WorkingDirectory": "c:\\inetpub\\wwwroot\\xbk29"
    }
  ],
  "CurrentProfile": "xbk29",
  "DefaultInstallProjectOptions": {
    "Version": null, // Version cannot have a default value
    "Template": "kentico-xperience-sample-mvc",
    "ProjectName": "my-rpoject",
    "InstallRootPath": "c:\\inetpub\\wwwroot",
    "UseCloud": false
  },
  "DefaultInstallDatabaseOptions": {
    "UseExistingDatabase": false,
    "DatabaseName": "xperience",
    "ServerName": "my-server"
  },
  "CDRootPath": "C:\\inetpub\\wwwroot\\ContinuousDeployment"
}
```

You can edit this file to change the `DefaultInstallOptions` used when [installing](#installing-a-new-project) new Xperience by Kentico projects, and the location of the [Continuous Deployment](#running-continuous-deployment) files.

## Usage

The following commands can be executed using the `xman` tool name:

- `?`, `help`
- [`i`, `install`](#installing-a-new-project)
- [`u`, `update`](#updating-a-project-version)
- [`d`, `delete`](#deleting-a-project)
- [`m`, `macros`](#re-signing-macros)
- [`b`, `build`](#building-projects)
- [`g`, `generate`](#generating-code-for-object-types)
- [`s`, `settings`](#modifying-appsettingsjson)
- [`ci <store> <restore>`](#running-continuous-integration)
- [`cd <store> <restore> <config>`](#running-continuous-deployment)
- [`p`, `profile <add> <delete> <switch>`](#managing-profiles)

### Managing profiles

Certain commands such as `update` are executed against the installation indicated by the current profile. The `profile` command shows you the current profile, and allows you to switch profiles. If you only have one profile, that is automatically selected.

To __switch__ profiles, run the `profile` command from the directory containing the [configuration file](#configuration-file):

```bash
xman profile
```

<img src="https://raw.githubusercontent.com/kentico/xperience-manager/master/img/profiles.png" width="350" />

You can __add__ or __delete__ profiles using the corresponding commands. This can be useful to register Xperience by Kentico installations that weren't installed using the tool.

```bash
xman p add
xman p delete
```

### Installing a new project

When installing a new project, a new folder will be created in the `InstallRootPath` of the [configuration file](#configuration-file), or in a custom directory that you specify in the installation wizard. After installation, a new [profile](#managing-profiles) is created for the instance.

The installation wizard will automatically generate an administrator password for you, but you can enter your own password during installation if needed.

1. Run the `install` command from the directory containing the [configuration file](#configuration-file) which will begin the installation wizard:

   ```bash
   xman install
   ```

Installing a new project automatically includes a database as well. If you want to _only_ install a database and not the project files, use the __db__ parameter: `xman install db`.

### Updating a project version

1. (optional) Select a profile with the [`profile`](#managing-profiles) command
1. Run the `update` command from the directory containing the [configuration file](#configuration-file), which will begin the update wizard:

   ```bash
   xman update
   ```

### Deleting a project

> :warning: The `delete` command will drop the database and delete the files. Use with caution!

1. (optional) Select a profile with the [`profile`](#managing-profiles) command
1. Run the `delete` command from the directory containing the [configuration file](#configuration-file):

   ```bash
   xman delete
   ```

### Modifying appsettings.json

This tool can assist with changing the _CMSConnectionString_, supported [configuration keys](https://docs.xperience.io/xp/developers-and-admins/configuration/reference-configuration-keys), and the [headless API](https://docs.xperience.io/xp/developers-and-admins/configuration/headless-channel-management#Headlesschannelmanagement-ConfiguretheheadlessAPI).

1. (optional) Select a profile with the [`profile`](#managing-profiles) command
1. Run the `settings` command from the directory containing the [configuration file](#configuration-file), which will begin the settings wizard:

   ```bash
   xman settings
   ```

<img src="https://raw.githubusercontent.com/kentico/xperience-manager/master/img/settings.png" width="350" />

### Re-signing macros

See [our documentation](https://docs.xperience.io/xp/developers-and-admins/configuration/macro-expressions/macro-signatures) for more information about macro signatures and the available options.

1. (optional) Select a profile with the [`profile`](#managing-profiles) command
1. Run the `macros` command from the directory containing the [configuration file](#configuration-file), which will begin the macro wizard:

   ```bash
   xman macros
   ```

### Building projects

1. (optional) Select a profile with the [`profile`](#managing-profiles) command
1. Run the `build` command from the directory containing the [configuration file](#configuration-file) to build the current profile's instance:

   ```bash
   xman build
   ```

### Generating code for object types

See [our documentation](https://docs.xperience.io/xp/developers-and-admins/api/generate-code-files-for-system-objects) for more information about code file generation.

1. (optional) Select a profile with the [`profile`](#managing-profiles) command
1. Run the `generate` command from the directory containing the [configuration file](#configuration-file) which will begin the generation wizard:

   ```bash
   xman generate
   ```

### Running Continuous Integration

You can use the `ci` command to serialize the database or restore the CI repository to the database. Your project must have been built at least once to run CI commands.

1. (optional) Select a profile with the [`profile`](#managing-profiles) command
2. Run the desired command from the directory containing the [configuration file](#configuration-file) to begin the CI process:

   - `xman ci store`
   - `xman ci restore`

### Running Continuous Deployment

This tool can help you manage a local [Continuous Deployment](https://docs.xperience.io/xp/developers-and-admins/ci-cd/continuous-deployment) environment. For example, if you are self-hosting your website and you have __DEV__ and __PROD__ Xperience by Kentico instances, the tool simplifies the process of migrating database changes from lower environments to production.

You can customize the location of the CD files by changing the __CDRootPath__ property in the [configuration file](#configuration-file):

```json
{
    "CDRootPath": "C:\\XperienceCDFiles"
}
```

Your project's [CD configuration file](https://docs.xperience.io/xp/developers-and-admins/ci-cd/exclude-objects-from-ci-cd) is automatically created when you run the `cd` command and can be manually edited to fine-tune the CD process. You can also run the `config` command to edit the configuration file using a wizard. For example, you may want to change the [__RestoreMode__](https://docs.xperience.io/xp/developers-and-admins/ci-cd/exclude-objects-from-ci-cd#ExcludeobjectsfromCI/CD-CDrestoremode) before restoring CD data to the database.

1. Select a profile with the [`profile`](#managing-profiles) command. This determines which configuration file is modified
1. Run the `config` command from the directory containing the [configuration file](#configuration-file), which will begin the configuration wizard:

   ```bash
   xman cd config
   ```

When you are finished development and wish to serialize the CD data to the filesystem, use the `store` command:

1. Select a profile with the [`profile`](#managing-profiles) command. This determines which project's database is serialized
1. Run the `store` command from the directory containing the [configuration file](#configuration-file):

   ```bash
   xman p # switch to DEV profile
   xman cd store # serialize DEV database
   ```

To migrate the changes from development to production, run the `restore` command:

1. Select a profile with the [`profile`](#managing-profiles) command. This determines which project's database is updated
1. Run the `restore` command from the directory containing the [configuration file](#configuration-file). The tool will display a list of profiles to choose as the __source__ for the restore process (in this example, the DEV profile):

   ```bash
   xman p # switch to PROD profile
   xman cd restore # restore DEV CD files to PROD database
   ```

## Support

[![Kentico Labs](https://img.shields.io/badge/Kentico_Labs-grey?labelColor=orange&logo=data:image/svg+xml;base64,PHN2ZyBjbGFzcz0ic3ZnLWljb24iIHN0eWxlPSJ3aWR0aDogMWVtOyBoZWlnaHQ6IDFlbTt2ZXJ0aWNhbC1hbGlnbjogbWlkZGxlO2ZpbGw6IGN1cnJlbnRDb2xvcjtvdmVyZmxvdzogaGlkZGVuOyIgdmlld0JveD0iMCAwIDEwMjQgMTAyNCIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxwYXRoIGQ9Ik05NTYuMjg4IDgwNC40OEw2NDAgMjc3LjQ0VjY0aDMyYzE3LjYgMCAzMi0xNC40IDMyLTMycy0xNC40LTMyLTMyLTMyaC0zMjBjLTE3LjYgMC0zMiAxNC40LTMyIDMyczE0LjQgMzIgMzIgMzJIMzg0djIxMy40NEw2Ny43MTIgODA0LjQ4Qy00LjczNiA5MjUuMTg0IDUxLjIgMTAyNCAxOTIgMTAyNGg2NDBjMTQwLjggMCAxOTYuNzM2LTk4Ljc1MiAxMjQuMjg4LTIxOS41MnpNMjQxLjAyNCA2NDBMNDQ4IDI5NS4wNFY2NGgxMjh2MjMxLjA0TDc4Mi45NzYgNjQwSDI0MS4wMjR6IiAgLz48L3N2Zz4=)](https://github.com/Kentico/.github/blob/main/SUPPORT.md#labs-limited-support)

This project has **Kentico Labs limited support**.

See [`SUPPORT.md`](https://github.com/Kentico/.github/blob/main/SUPPORT.md#full-support) for more information.

For any security issues see [`SECURITY.md`](https://github.com/Kentico/.github/blob/main/SECURITY.md).
