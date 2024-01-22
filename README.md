# KeePassOTP

[![Version](https://img.shields.io/github/release/rookiestyle/keepassotp)](https://github.com/rookiestyle/keepassotp/releases/latest)
[![Release date](https://img.shields.io/github/release-date/rookiestyle/keepassotp)](https://github.com/rookiestyle/keepassotp/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/rookiestyle/keepassotp/total?color=%2300cc00)](https://github.com/rookiestyle/keepassotp/releases/latest/download/keepassotp.plgx)
[![License](https://img.shields.io/github/license/rookiestyle/keepassotp)](https://www.gnu.org/licenses/gpl-3.0)

KeePassOTP allows you to generate one time passwords (OTP).
It supports both standard TOTP and standard HOTP as well as Steam OTP and Yandex (Yandex.Key < 3).

Once set up, you can copy or auto-type the OTP.
You may also create a QR code and scan it with an OTP app of your liking, e.g. andOTP, Google Authenticator etc.

Functionality provided

- Drag&Drop QR codes to add OTP
- Auto-Type placeholder
- Configurable hotkey to invoke Auto-Type OTP
- Column to display OTP or to indicate possible 2FA usage
- Compatibility with Google Authenticator, andOTP etc.
- Support TOTP and HOTP as well as Steam OTP and Yandex (Yandex.Key < 3)
- Secure storage of OTP secrets
- Auto-Type or Copy OTP using the KeePass tray icon
- Indicate possible usage of OTP / two factor authentication (requires [internet access](#two-factor-authentication--internet-access))

Proprietary formats like Steam and Yandex.Key are supported as well.  
Unfortunately Yandex changed their closed source algorithm starting with version 3. Due to lack of information, it's no longer possible to generate Yandex codes after installing / migrating to Yandex.Key version 3 or higher.

## Table of Contents

- [Configuration](#configuration)
- [Usage](#usage)
- [Two factor authentication / Why does KeePassOTP access the internet?](#two-factor-authentication--internet-access)
- [Translations](#translations)
- [Download & updates](#download--updates)
- [Requirements](#requirements)

## Configuration

KeePassOTP integrates into KeePass' options form.

<img src="images/KeePassOTP%20-%20options.png" alt="Options" height="50%" width="50%">

In the database-specific area you can choose the working mode of KeePassOTP.

<img src="images/KeePassOTP%20-%20options%202.png" alt="Options 2" height="50%" width="50%">

The general area lets you define settings that are valid for all databases.

## Usage

OTP settings are entry-specific.
To setup, change, copy or auto-type the OTP, you may use the context menu of the respective entry.

If you're interested in migration instructions to move from (or to) other OTP plugins or if you want to know more about some of the technical details, please have a look at the [Wiki](https://github.com/rookiestyle/keepassotp/wiki).

### Setup OTP data / Change OTP data

Use the entry's context menu to setup/change OTP data.

In the setup form, you can drag&drop the QR code image onto the QR picture in the setup form and have KeePassOTP parse it or you can enter the OTP secret manually (sometimes referred to as a seed) and you're ready to go.

In addition, you can click the screen capture button and KeePassOTP will search for an OTP QR code anywhere on the screen.

Got a string like this one?

```
otpauth://totp/Example:alice@google.com?secret=JBSWY3DPEHPK3PXP&issuer=Example
```

Paste the entire string, KeePassOTP can process this [otpauth-format](https://github.com/google/google-authenticator/wiki/Key-Uri-Format) as well.

As almost all sites require you to enter an OTP as part of the activation process, both current and next OTP are displayed here as well.

<img src="images/KeePassOTP%20-%20setup%20simple.png" alt="Setup">

### Use OTP data

There are multiple convenient ways to use the one time passwords.

- Copy OTP 
  - Using the context menu of the entry
  - Doubleclicking the OTP displayed in the optional KPOTP column
  - Using KeePass' tray icon
- Auto-Type
  - Using the context menu of the entry - *Show additional auto-type menu commands* needs to be active
  - Using a configurable placeholder - default placeholder: `{KPOTP}`
  - Using a configurable hotkey working inside and outside KeePass
  - Using KeePass' tray icon
 
## Two factor authentication / Internet access

KeePassOTP can use the [OTP column](#use-otp-data) to highlight entries where you can set up two-factor authentication.  If you doubleclick this hint, KeePassOTP will open the corresponding website to setup two-factor authentication.  

For that purpose, KeePassOTP will download a list of sites supporting two factor authentication during KeePass start from [https://2fa.directory/api/v2/tfa.json](https://api.2fa.directory/v3/tfa.json).  
This list is then compared it with all URLs defined in the database.  
Neither your URLs nor any other information from your database will be sent.

You can toggle this feature in [KeePassOTP's options](#configuration).

## Translations

KeePassOTP is provided with English language built-in and allows usage of translation files.
These translation files need to be placed in a folder called *Translations* inside your plugin folder.
If a text is missing in the translation file, it is backfilled with English text.
You're welcome to add additional translation files by creating a pull request as described in the [wiki](https://github.com/Rookiestyle/KeePassOTP/wiki/Create-or-update-translations).

Naming convention for translation files: `<plugin name>.<language identifier>.language.xml`

Example: `KeePassOTP.de.language.xml`

The language identifier in the filename must match the language identifier inside the KeePass language that you can select using *View -> Change language...*
This identifier is shown there as well, if you have [EarlyUpdateCheck](https://github.com/rookiestyle/earlyupdatecheck) installed.

## Download & updates

Please follow these links to download the plugin file itself.

- [Download newest release](https://github.com/rookiestyle/keepassotp/releases/latest/download/KeePassOTP.plgx)
- [Download history](https://github.com/rookiestyle/keepassotp/releases)

If you're interested in any of the available translations in addition, please download them from the [Translations](Translations) folder.

In addition to the manual way of downloading the plugin, you can use [EarlyUpdateCheck](https://github.com/rookiestyle/earlyupdatecheck/) to update both the plugin and its translations automatically.  
See the [one click plugin update wiki](https://github.com/Rookiestyle/EarlyUpdateCheck/wiki/One-click-plugin-update) for more details.

## Requirements

- KeePass version 2.42
- .NET framework version 4.0
